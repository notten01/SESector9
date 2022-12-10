using ParallelTasks;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Api;
using Sector9.Core;
using Sector9.Core.Logging;
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
        private readonly List<IMyEntity> Grids;
        private MyCubeGrid PrimaryGrid;
        private readonly string PrefabName;
        private readonly Wc WeaponsCore;
        private readonly DefinitionLibrary Library;
        private readonly UnitCommander Commander;

        private MyRemoteControl RemoteControl;
        private Task InitWorker;
        private List<Thruster> Thrusters;
        private Provider Provider;

        private bool Init = false;
        private int ResuplyCounter = 0;
        private int BehaviourCounter = 0;
        public bool IsValid { get; private set; }

        private readonly IBehaviour ActiveBehaviour;

        public Unit(List<IMyEntity> grids, UnitCommander commander, string prefabName, Wc weaponsCore, DefinitionLibrary library, IBehaviour behaviour)
        {
            Commander = commander;
            PrefabName = prefabName;
            WeaponsCore = weaponsCore;
            Grids = grids;
            Library = library;
            ActiveBehaviour = behaviour;
            ActiveBehaviour.SetUnit(this);
            InitWorker = MyAPIGateway.Parallel.StartBackground(Initialize);
            IsValid = true;
            commander.RegisterUnit(this);
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

            if (ActiveBehaviour == null || Provider == null)
            {
                Logger.Log($"UNIT incorrect data: behaviour: {ActiveBehaviour != null}. provider: {Provider != null}", Logger.Severity.Fatal, Logger.LogType.Server);
                return;
            }

            if (!ActiveBehaviour.IsReady)
            {
                ActiveBehaviour.AttachRemoteControl(RemoteControl);
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
                ActiveBehaviour.Tick();
                BehaviourCounter = 0;
            }
            else
            {
                BehaviourCounter++;
            }
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
        }
    }
}