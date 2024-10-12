using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
using Sector9.Multiplayer.FromLayer;
using Sector9.Server.Firewall;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Sector9.Server.FireWall
{
    public class FirewallHandler : ITickable
    {
        private const string cDataFilename = "S9FirewallTracking.xml";
        private const string FirewallName = "Firewall";
        private readonly FactionManager FactionManager;
        private FirewallData Data;
        private IMyCubeBlock Firewall;
        private bool HasFirewall;
        private int tickIndex = 7;
        private bool ValidFirewall;
        private bool GameEnded = false;
        //small offset so not every system tick on the same cycle
        //todo: you can save the grid with the firewall with the entity id

        public FirewallHandler(FactionManager factionManager)
        {
            TryLoad();
            FactionManager = factionManager;
            ScanForFirewall();
            if (HasFirewall && Data.GridId > 0)
            {
                Data.GridId = -1;
                ScanForFirewall();
            }

            if (HasFirewall)
            {
                if (Firewall.GetOwnerFactionTag() != factionManager.HostileFaciton.Tag)
                {
                    ValidFirewall = Firewall.IsWorking;
                    Firewall.IsWorkingChanged += FirewallChanged;
                    Firewall.OnMarkForClose += FirewallClosing;
                    Logger.Log($"Found existing firewall in savegame, valid: {ValidFirewall}, countdown: {Data.FirewallCountdown}", Logger.Severity.Info, Logger.LogType.Server);
                }
                else
                {
                    Firewall.CubeGrid.RemoveBlock(Firewall.SlimBlock);
                    HasFirewall = false;
                    ValidFirewall = false;
                    Logger.Log("Firewall was found to was not part of the human faction", Logger.Severity.Warning, Logger.LogType.Server);
                }
            }
            BeginGridTracking();
        }

        /// <summary>
        /// reset the current firewall countdown (cheat!)
        /// </summary>
        public void ResetCountdown()
        {
            Data.ResetCountdownn();
        }

        /// <summary>
        /// Does the game session have a valid firewall
        /// </summary>
        /// <returns>Valid firewall is tracked</returns>
        public bool IsFirewallValid()
        {
            return HasFirewall && ValidFirewall;
        }

        public void Save()
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(cDataFilename, typeof(FirewallData)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML<FirewallData>(Data));
            }
        }

        public void Shutdown()
        {
            DisableGridTracking();

            if (HasFirewall)
            {
                Firewall.OnMarkForClose -= FirewallClosing;
                Firewall.IsWorkingChanged -= FirewallChanged;
            }
        }

        public void Tick()
        {
            if (Data.GameOver && !GameEnded)
            {
                GameEnded = true;
                SyncManager.Instance.SendPayloadFromServer(FromLayerType.GameOver, new GameOver());
                Firewall.OnMarkForClose -= FirewallClosing;
                Firewall.IsWorkingChanged -= FirewallChanged;
            }
            if (tickIndex < 60)
            {
                tickIndex++;
                return;
            }
            tickIndex = 0;

            if (!IsFirewallValid())
            {
                Data.FirewallCountdown--;
                if (Data.FirewallCountdown <= 0)
                {
                    Logger.Log("Game over!", Logger.Severity.Warning, Logger.LogType.System);
                    Data.GameOver = true;
                }
            }
            else
            {
                Data.IncreaseCountdown();
            }
        }

        private void AddedEntity(IMyEntity newEntity)
        {
            IMyCubeGrid grid = newEntity as IMyCubeGrid;

            if (grid == null) { return; }

            grid.OnBlockAdded += CheckIfIsFirewall;
            grid.OnMarkForClose += UnsubscribeGrid;
        }

        private void BeginGridTracking()
        {
            Logger.Log("Setting up firewall tracking", Logger.Severity.Info, Logger.LogType.Server);
            Data.GridId = -1;
            HashSet<IMyEntity> allEntities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(allEntities);
            foreach (IMyEntity entity in allEntities)
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;
                if (grid == null) { continue; }
                grid.OnBlockAdded += CheckIfIsFirewall;
            }
            MyEntities.OnEntityAdd += AddedEntity;
        }

        private void CheckIfIsFirewall(IMySlimBlock newBlock)
        {
            if (newBlock.CubeGrid == null) { return; } //block is not yet build

            if (newBlock.FatBlock != null && newBlock.FatBlock.BlockDefinition.SubtypeId.Equals(FirewallName))
            {
                IMyCubeBlock newFirewall = newBlock.FatBlock;

                if (HasFirewall)
                {
                    Notification message = new Notification() { Duration = 4000, Font = "red", Message = "You cant have more then 1 firewall block!" };
                    SyncManager.Instance.SendPayloadFromServer(FromLayerType.Notification, message, newBlock.BuiltBy);
                    newFirewall.CubeGrid.RemoveBlock(newBlock);
                }
                else
                {
                    if (!newFirewall.CubeGrid.IsStatic)
                    {
                        Notification message = new Notification() { Duration = 4000, Font = "red", Message = "Firewall can only be build on a static grid!" };
                        SyncManager.Instance.SendPayloadFromServer(FromLayerType.Notification, message, newBlock.BuiltBy);
                        newFirewall.CubeGrid.RemoveBlock(newBlock);
                        return;
                    }
                    if (newFirewall.GetOwnerFactionTag() == FactionManager.HostileFaciton.Tag)
                    {
                        Notification message = new Notification() { Duration = 5000, Font = "red", Message = "wait what...how?! No, NO! you cannot build a firewall on a HOSTILE grid" };
                        SyncManager.Instance.SendPayloadFromServer(FromLayerType.Notification, message, newBlock.BuiltBy);
                        newFirewall.CubeGrid.RemoveBlock(newBlock);
                        return;
                    }

                    Logger.Log("Firewall added!", Logger.Severity.Info, Logger.LogType.Server);
                    Firewall = newFirewall;
                    HasFirewall = true;
                    ValidFirewall = Firewall.IsWorking;
                    Data.GridId = Firewall.CubeGrid.EntityId;
                    Firewall.IsWorkingChanged += FirewallChanged;
                    Firewall.OnMarkForClose += FirewallClosing;
                }
            }
        }

        private void DisableGridTracking()
        {
            MyEntities.OnEntityAdd -= AddedEntity;
        }

        private void FirewallChanged(IMyCubeBlock obj)
        {
            ValidFirewall = obj.IsWorking;
            if (!ValidFirewall)
            {
                SyncManager.Instance.SendPayloadFromServer(FromLayerType.Sound, new Sound() { Queue = true, SoundName = "3urgentbeep" }); //firewall stopped functioning
                SyncManager.Instance.SendPayloadFromServer(FromLayerType.Sound, new Sound() { Queue = true, SoundName = "firewall-offline" });//firewall offline
            }
            else
            {
                SyncManager.Instance.SendPayloadFromServer(FromLayerType.Sound, new Sound() { Queue = true, SoundName = "firewallOn" }); //firewall online
                SyncManager.Instance.SendPayloadFromServer(FromLayerType.Sound, new Sound() { Queue = true, SoundName = "firewall-online" });//firewall online
            }
            Logger.Log($"Firewall functioning state changed to {ValidFirewall}", Logger.Severity.Info, Logger.LogType.Server);
        }

        private void FirewallClosing(IMyEntity obj)
        {
            ValidFirewall = false;
            HasFirewall = false;
            Firewall.IsWorkingChanged -= FirewallChanged;
            Firewall.OnMarkForClose -= FirewallClosing;
            SyncManager.Instance.SendPayloadFromServer(FromLayerType.Sound, new Sound() { Queue = true, SoundName = "3hardbeep" });
            SyncManager.Instance.SendPayloadFromServer(FromLayerType.Sound, new Sound() { Queue = true, SoundName = "firewall-destroyed" });
            Logger.Log("Firewall is destroyed!", Logger.Severity.Info, Logger.LogType.Server);
        }

        private HashSet<IMyEntity> GetGridList()
        {
            HashSet<IMyEntity> allEntities = new HashSet<IMyEntity>();
            if (Data.GridId < 0)
            {
                MyAPIGateway.Entities.GetEntities(allEntities);
            }
            else
            {
                allEntities.Add(MyAPIGateway.Entities.GetEntityById(Data.GridId));
            }
            return allEntities;
        }

        private void ScanForFirewall()
        {
            HashSet<IMyEntity> scanEntities = GetGridList();
            foreach (IMyEntity entity in scanEntities)
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;
                if (grid == null)
                {
                    continue;
                }

                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                grid.GetBlocks(blocks, x => x.FatBlock != null && x.FatBlock.BlockDefinition.SubtypeId.Equals(FirewallName));
                if (blocks.Count == 0)
                {
                    continue;
                }
                else if (blocks.Count > 1)
                {
                    Logger.Log("There is more then one firewall found on a grid!, defaulting to first found", Logger.Severity.Error, Logger.LogType.Server);
                }
                Firewall = blocks.First().FatBlock;
                HasFirewall = true;
            }
        }

        private void TryLoad()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cDataFilename, typeof(FirewallData)))
            {
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cDataFilename, typeof(FirewallData)))
                    {
                        Data = MyAPIGateway.Utilities.SerializeFromXML<FirewallData>(reader.ReadToEnd());
                    }
                }
                catch (InvalidOperationException)
                {
                    Logger.Log("The firewall settings could not be loaded for some reason, they have been overwritten by the default!", Logger.Severity.Error, Logger.LogType.System);
                }
            }
            if (Data == null)
            {
                Data = new FirewallData();
            }
        }

        private void UnsubscribeGrid(IMyEntity closingGrid)
        {
            IMyCubeGrid grid = closingGrid as IMyCubeGrid;
            grid.OnBlockAdded -= CheckIfIsFirewall;
            grid.OnMarkForClose -= UnsubscribeGrid;
        }
    }
}