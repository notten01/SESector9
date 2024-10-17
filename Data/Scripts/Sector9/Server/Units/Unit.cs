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
using VRageMath;

namespace Sector9.Server.Units
{
    public class Unit : ITickable
    {
        private readonly UnitCommander Commander;
        private readonly List<IMyEntity> Grids;
        private readonly DefinitionLibrary Library;
        private readonly Planets Planets;
        private readonly string PrefabName;
        private readonly Wc WeaponsCore;
        private DamageHandler DamageHandler;
        private bool Init = false;
        private Task InitWorker;
        private MyCubeGrid PrimaryGrid;
        private Provider Provider;
        private IMyRemoteControl RemoteControl;
        private int ResuplyCounter = 0;
        private List<Thruster> Thrusters;
        private IMyOffensiveCombatBlock CombatAi;
        private IMyFlightMovementBlock FlightAi;
        private bool InGameAi = false;
        private bool Autopiloting = false;
        private IMyEntity Target;
        private int AutoPilotTimeout = 0;
        private int OnLocationTimeout = 0;
        private Vector3D TargetPosition;


        public Unit(List<IMyEntity> grids, UnitCommander commander, string prefabName, Wc weaponsCore, DefinitionLibrary library, Planets planets, IMyEntity target)
        {
            target = Target;
            Planets = planets;
            Commander = commander;
            PrefabName = prefabName;
            WeaponsCore = weaponsCore;
            Grids = grids;
            Library = library;
            IsValid = true;
            commander.RegisterUnit(this);
        }

        public bool IsValid { get; private set; }

        public long GetId()
        {
            return PrimaryGrid.EntityId;
        }

        public void SetDamageHandler(DamageHandler damageHandler)
        {
            DamageHandler = damageHandler;
            InitWorker = MyAPIGateway.Parallel.StartBackground(Initialize);
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
                if (DamageHandler != null && InitWorker.IsComplete && InitWorker.Exceptions?.Length > 0)
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
                else if (DamageHandler != null && InitWorker.IsComplete)
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

            if (Autopiloting)
            {
                if (OnLocationTimeout > 60)
                {
                    OnLocationTimeout = 0;
                    Vector3D myLocation = RemoteControl.GetPosition();
                    double distance = Vector3D.Distance(TargetPosition, myLocation);
                    if (distance < 2000)
                    {
                        SwitchToIngameAi();
                        return;
                    }
                }

                if (AutoPilotTimeout > 1800)
                {
                    //check if still on location
                    AutoPilotTimeout = 0;
                    Vector3D newTargetPosition = Target.GetPosition();
                    if( Vector3D.Distance(newTargetPosition, TargetPosition) > 100)
                    {
                        RemoteControl.ClearWaypoints();
                        RemoteControl.AddWaypoint(newTargetPosition, "Target");
                        RemoteControl.SetAutoPilotEnabled(true);
                        TargetPosition = newTargetPosition;
                    }
                } else
                {
                    AutoPilotTimeout++; //idle
                }
            } else if (!InGameAi)
            {
                //set autopilot
                TargetPosition = Target.GetPosition();
                RemoteControl.AddWaypoint(TargetPosition, "Target");
                RemoteControl.FlightMode = Sandbox.ModAPI.Ingame.FlightMode.OneWay;
                RemoteControl.SpeedLimit = 100;
                RemoteControl.SetAutoPilotEnabled(true);
                Autopiloting = true;
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

        }

        private void SwitchToIngameAi()
        {
            Autopiloting = false;
            InGameAi = true;
            RemoteControl.SetAutoPilotEnabled(false);
            if (FlightAi != null)
            {
                FlightAi.Enabled = true;
            }
            if (CombatAi != null)
            {
                CombatAi.Enabled = true;
            }
        }

        private void Initialize()
        {
            Thrusters = new List<Thruster>();
            PrimaryGrid = Grids[0] as MyCubeGrid;
            ListReader<MyCubeBlock> fatblocks = PrimaryGrid.GetFatBlocks();
            RemoteControl = fatblocks.OfType<MyRemoteControl>().FirstOrDefault();
            CombatAi = fatblocks.OfType<IMyOffensiveCombatBlock>().FirstOrDefault();
            FlightAi = fatblocks.OfType<IMyFlightMovementBlock>().FirstOrDefault();
            foreach (IMyThrust thrust in fatblocks.OfType<IMyThrust>())
            {
                Thrusters.Add(new Thruster(thrust));
            }
            Provider = new Provider(fatblocks, WeaponsCore, Library);
            DamageHandler.TrackEntity(PrimaryGrid.EntityId);
        }
    }
}