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

namespace Sector9.Server.Buildings
{
    public class Building : ITickable
    {
        private readonly BuildingCommander Commander;
        private readonly List<IMyEntity> Grids;
        private readonly DefinitionLibrary Library;
        private readonly string PrefabName;
        private readonly Wc WeaponsCore;
        private DamageHandler DamageHandler;
        private bool Init = false;
        private Task InitWorker;
        private bool IsValid = true;
        private MyCubeGrid PrimaryGrid;
        private Provider Provider;
        private MyRemoteControl RemoteControl;
        private int resuplyCounter = 0;

        public Building(List<IMyEntity> grids, BuildingCommander commander, string prefabName, Wc weaponsCore, DefinitionLibrary library)
        {
            Grids = grids;
            PrefabName = prefabName;
            WeaponsCore = weaponsCore;
            Library = library;
            Commander = commander;
            InitWorker = MyAPIGateway.Parallel.StartBackground(Initialize);
        }

        public void SetDamageHandler(DamageHandler damageHandler)
        {
            DamageHandler = damageHandler;
        }

        public long GetId()
        {
            return PrimaryGrid.EntityId;
        }

        public void Tick()
        {
            if (!IsValid)
            {
                Commander.UnregisterBuilding(this);
                return;
            }

            if (!Init)
            {
                if (InitWorker.IsComplete && InitWorker.Exceptions?.Length > 0)
                {
                    //error
                    StringBuilder message = new StringBuilder();
                    message.AppendLine("Could not initialize building with prefab ").Append(PrefabName).Append(" Init worker crashed with exceptions: ");
                    foreach (Exception e in InitWorker.Exceptions)
                    {
                        message.AppendLine(e.ToString());
                    }
                    Logger.Log(message.ToString(), Logger.Severity.Error, Logger.LogType.Server);
                    IsValid = false;
                }
                else if (InitWorker.IsComplete)
                {
                    Logger.Log($"Building {PrefabName} Initialized!", Logger.Severity.Info, Logger.LogType.Server);
                    Init = true;
                }
                return;
            }

            if (RemoteControl == null || RemoteControl.Closed || !RemoteControl.IsWorking)
            {
                IsValid = false;
                return;
            }

            if (resuplyCounter > 600)
            {
                Provider.Tick();
                resuplyCounter = 0;
            }
            else
            {
                resuplyCounter++;
            }
        }

        private void Initialize()
        {
            PrimaryGrid = Grids.First() as MyCubeGrid;
            ListReader<MyCubeBlock> fatblocks = PrimaryGrid.GetFatBlocks();
            RemoteControl = fatblocks.OfType<MyRemoteControl>().FirstOrDefault();
            Provider = new Provider(fatblocks, WeaponsCore, Library);
            DamageHandler.TrackEntity(PrimaryGrid.EntityId);
        }
    }
}