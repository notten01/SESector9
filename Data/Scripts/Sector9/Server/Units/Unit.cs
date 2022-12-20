using ParallelTasks;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Api;
using Sector9.Core;
using Sector9.Core.Logging;
using Sector9.Server.Units.Behaviours;
using Sector9.Server.Units.Control;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Collections;
using VRage.ModAPI;

namespace Sector9.Server.Units
{
    public class Unit : ITickable
    {
        private readonly ICaptain Captain;
        private readonly UnitCommander Commander;
        private readonly List<IMyEntity> Grids;
        private readonly DefinitionLibrary Library;
        private readonly Planets Planets;
        private readonly string PrefabName;
        private readonly Wc WeaponsCore;
        private IBehaviour ActiveBehaviour;
        private int BehaviourCounter = 0;
        private DamageHandler DamageHandler;
        private bool Init = false;
        private Task InitWorker;
        private Pilot Pilot;
        private MyCubeGrid PrimaryGrid;
        private Provider Provider;
        private MyRemoteControl RemoteControl;
        private int ResuplyCounter = 0;
        private List<Thruster> Thrusters;

        public Unit(List<IMyEntity> grids, UnitCommander commander, string prefabName, Wc weaponsCore, DefinitionLibrary library, ICaptain captain, Planets planets) : this(grids, commander, prefabName, weaponsCore, library, planets)
        {
            Captain = captain;
        }

        public Unit(List<IMyEntity> grids, UnitCommander commander, string prefabName, Wc weaponsCore, DefinitionLibrary library, IBehaviour behaviour, Planets planets) : this(grids, commander, prefabName, weaponsCore, library, planets)
        {
            ActiveBehaviour = behaviour;
            ActiveBehaviour.SetUnit(this);
            Captain = null;
        }

        private Unit(List<IMyEntity> grids, UnitCommander commander, string prefabName, Wc weaponsCore, DefinitionLibrary library, Planets planets)
        {
            Planets = planets;
            Commander = commander;
            PrefabName = prefabName;
            WeaponsCore = weaponsCore;
            Grids = grids;
            Library = library;
            InitWorker = MyAPIGateway.Parallel.StartBackground(Initialize);
            IsValid = true;
            commander.RegisterUnit(this);
        }

        public bool IsValid { get; private set; }

        public long GetId()
        {
            return PrimaryGrid.EntityId;
        }

        public bool IsExecutingBehaviour()
        {
            return ActiveBehaviour != null && !ActiveBehaviour.IsComplete;
        }

        public void SetDamageHandler(DamageHandler damageHandler)
        {
            DamageHandler = damageHandler;
        }

        public void Tick()
        {
            if (!IsValid)
            {
                Commander.UnregisterUnit(this);
                return;
            }

            if (!Init)
            {
                if (InitWorker.IsComplete && InitWorker.Exceptions?.Length > 0)
                {
                    //error
                    StringBuilder message = new StringBuilder();
                    message.AppendLine("Could not initialize unit, init worker crashed for prefab").Append(PrefabName).Append("with excpetion:");
                    foreach (Exception e in InitWorker.Exceptions)
                    {
                        message.AppendLine(e.ToString());
                    }
                    Logger.Log(message.ToString(), Logger.Severity.Error, Logger.LogType.Server);
                    IsValid = false;
                }
                else if (InitWorker.IsComplete)
                {
                    Logger.Log("Unit initialized!", Logger.Severity.Info, Logger.LogType.Server);
                    Init = true;
                }
                return;
            }

            if (RemoteControl == null || RemoteControl.Closed || !RemoteControl.IsWorking)
            {
                IsValid = false;
                return;
            }

            if (ActiveBehaviour != null && !ActiveBehaviour.IsReady)
            {
                ActiveBehaviour.AttachPilot(Pilot);
                BehaviourCounter = int.MaxValue; //ensure it will trigger the first cycle of the behaviour
            }

            if (ResuplyCounter > 600)
            {
                Provider.Tick();
                ResuplyCounter = 0;
            }
            else
            {
                ResuplyCounter++;
            }

            if (BehaviourCounter > 60)
            {
                Captain?.Tick();
                ActiveBehaviour?.Tick();
                BehaviourCounter = 0;
            }
            else
            {
                BehaviourCounter++;
            }
        }

        internal void SetBehaviour(IBehaviour behaviour)
        {
            if (ActiveBehaviour != null && !ActiveBehaviour.IsReady)
            {
                ActiveBehaviour.Interrupt();
            }
            Logger.Log($"Unit {RemoteControl.CubeGrid.EntityId} Switching to new behaviour {behaviour.Name}", Logger.Severity.Info, Logger.LogType.Server);
            ActiveBehaviour = behaviour;
            ActiveBehaviour.SetUnit(this);
            ActiveBehaviour.AttachPilot(Pilot);
        }

        private void Initialize()
        {
            Thrusters = new List<Thruster>();
            PrimaryGrid = Grids.First() as MyCubeGrid;
            ListReader<MyCubeBlock> fatblocks = PrimaryGrid.GetFatBlocks();
            RemoteControl = fatblocks.OfType<MyRemoteControl>().FirstOrDefault();
            foreach (IMyThrust thrust in fatblocks.OfType<IMyThrust>())
            {
                Thrusters.Add(new Thruster(thrust));
            }
            Provider = new Provider(fatblocks, WeaponsCore, Library);
            Pilot = new Pilot(RemoteControl, Planets);
            if (Captain != null)
            {
                Captain.SetCaptainData(RemoteControl, this);
            }
            DamageHandler.TrackEntity(PrimaryGrid.EntityId);
        }
    }
}