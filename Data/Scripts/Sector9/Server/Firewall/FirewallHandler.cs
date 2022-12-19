using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;
using Sector9.Server.Firewall;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Sector9.Server.FireWall
{
    internal class FirewallHandler : ITickable
    {
        private const string cDataFilename = "S9FirewallTracking.xml";
        private const string FirewallName = "Firewall";
        private readonly FactionManager FactionManager;
        private FirewallData Data;
        private IMyCubeBlock Firewall;
        private bool HasFirewall;
        private bool ValidFirewall;
        private bool WasValidLastCycle;
        private int tickIndex = 7; //small offset so not every system tick on the same cycle
        //todo: you can save the grid with the firewall with the entity id

        public FirewallHandler(FactionManager factionManager)
        {
            WasValidLastCycle = true;
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
                    Logger.Log("Found existing firewall in savegame", Logger.Severity.Info, Logger.LogType.Server);
                    ValidFirewall = Firewall.IsWorking;
                    Firewall.IsWorkingChanged += FirewallChanged;
                    Firewall.OnMarkForClose += FirewallClosing;
                }
                else
                {
                    Firewall.Close(); //belongs to a none human faction
                    HasFirewall = false;
                    Logger.Log("Firewall was found to was not part of the human faction", Logger.Severity.Warning, Logger.LogType.Server);
                }
            }
            BeginGridTracking();
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

        private void AddedEntity(IMyEntity newEntity)
        {
            IMyCubeGrid grid = newEntity as IMyCubeGrid;

            if (grid == null) { return; }

            Logger.Log("Tracking new entity for firewall", Logger.Severity.Info, Logger.LogType.Server);
            grid.OnBlockAdded += CheckIfIsFirewall;
            grid.OnMarkForClose += UnsubscribeGrid;
        }

        private void UnsubscribeGrid(IMyEntity closingGrid)
        {
            IMyCubeGrid grid = closingGrid as IMyCubeGrid;
            grid.OnBlockAdded -= CheckIfIsFirewall;
            grid.OnMarkForClose -= UnsubscribeGrid;
        }

        private void BeginGridTracking()
        {
            Logger.Log("Setting up firewall tracking", Logger.Severity.Info, Logger.LogType.Server);
            Data.GridId = -1;
            HashSet<IMyEntity> allEntities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(allEntities);
            Logger.Log("got all entries", Logger.Severity.Info, Logger.LogType.Server);
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

            Logger.Log($"Checking if new block is firewall", Logger.Severity.Info, Logger.LogType.Server);
            if (newBlock.FatBlock != null && newBlock.FatBlock.BlockDefinition.SubtypeId.Equals(FirewallName))
            {
                Logger.Log("New block seems to be firewall type", Logger.Severity.Info, Logger.LogType.Server);
                IMyCubeBlock newFirewall = newBlock.FatBlock;

                if (HasFirewall)
                {
                    Logger.Log("Can't have more then 1 firewall!", Logger.Severity.Warning, Logger.LogType.Server);
                    //newFirewall.Close(); //not allowed to build two firewalls
                    newFirewall.CubeGrid.RemoveBlock(newBlock);
                }
                else
                {
                    if (!newFirewall.CubeGrid.IsStatic)
                    {
                        Logger.Log("Firewall must be on static grid!", Logger.Severity.Warning, Logger.LogType.Server);
                        //newFirewall.Close(); //must be build on a static grid
                        newFirewall.CubeGrid.RemoveBlock(newBlock);
                        return;
                    }
                    if (newFirewall.GetOwnerFactionTag() == FactionManager.HostileFaciton.Tag)
                    {
                        Logger.Log("Firewall must belong to human faction!", Logger.Severity.Warning, Logger.LogType.Server);
                        //newFirewall.Close(); //must be human faction
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
            Logger.Log($"Firewall functioning state changed to {ValidFirewall}", Logger.Severity.Info, Logger.LogType.Server);
        }

        private void FirewallClosing(IMyEntity obj)
        {
            ValidFirewall = false;
            HasFirewall = false;
            Firewall.IsWorkingChanged -= FirewallChanged;
            Firewall.OnMarkForClose -= FirewallClosing;
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
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cDataFilename, typeof(FirewallData)))
                {
                    Data = MyAPIGateway.Utilities.SerializeFromXML<FirewallData>(reader.ReadToEnd());
                }
            }
            if (Data == null)
            {
                Data = new FirewallData();
            }
        }

        public void Tick()
        {
            if (tickIndex < 60)
            {
                tickIndex++;
                return;
            }
            tickIndex = 0;

            if (!IsFirewallValid())
            {
                Data.FirewallCountdown--;
                WasValidLastCycle = false;
                if (Data.FirewallCountdown <= 0)
                {
                    Data.GameOver = true;
                }
            }
            else
            {
                if (!WasValidLastCycle)
                {
                    Data.ResetCountdownn();
                }
            }
        }
    }
}