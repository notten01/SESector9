using Sandbox.ModAPI;
using Sector9.Multiplayer;
using Sector9.Server;
using SpaceEngineers.Game.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand
{
    /// <summary>
    /// scans faction for strength
    /// </summary>
    public class PlayerScanner
    {
        private readonly FactionManager Factions;
        private readonly HashSet<IMyCubeGrid> SubscribedEntities;

        public PlayerScanner(FactionManager factions)
        {
            SubscribedEntities = new HashSet<IMyCubeGrid>();
            Factions = factions;
            //clone existing world to memory
            MyAPIGateway.Entities.OnEntityAdd += Entities_OnEntityAdd;
            DoFullScan();
        }

        public long Points { get; private set; }

        public void DoFullScan()
        {
            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, x => x is IMyCubeGrid);
            HashSet<long> players = GetPlayerHashes();
            long points = 0;
            var grids = entities.Where(x => x is IMyCubeGrid).Cast<IMyCubeGrid>();

            foreach (IMyCubeGrid grid in grids)
            {
                if (IsHumanOwned(grid, players))
                {
                    continue;
                }
                int multiplayer = grid.GridSizeEnum == VRage.Game.MyCubeSize.Large ? 3 : 1;
                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                grid.GetBlocks(blocks);
                foreach (IMyCubeBlock block in blocks.Where(x => x.FatBlock != null).Select(x => x.FatBlock))
                {
                    points += GetPointsForBlock(block) * multiplayer;
                }
            }
            foreach (var ent in entities)
            {
                IMyCubeGrid grid = ent as IMyCubeGrid;
                if (grid == null) { continue; }
                grid.OnBlockAdded += Grid_OnBlockAdded;
                grid.OnBlockRemoved += Grid_OnBlockRemoved;
                grid.OnClose += Grid_OnClose;
                SubscribedEntities.Add(grid);
            }
            Points = points;
        }

        public void Shutdown()
        {
            MyAPIGateway.Entities.OnEntityAdd -= Entities_OnEntityAdd;
            foreach (IMyCubeGrid grid in SubscribedEntities)
            {
                UnsubscribeGrid(grid);
            }
        }

        private static int GetPointsForBlock(IMyCubeBlock block)
        {
            if (block is IMyRefinery)
            {
                return 10;
            }
            else if (block is IMyAssembler)
            {
                return 15;
            }
            else if (block is IMyCockpit)
            {
                return 5;
            }
            else if (block is IMyCargoContainer || block is IMyGasTank)
            {
                return 1;
            }
            else if (block is IMyGasGenerator)
            {
                return 5;
            }
            else if (block is IMyReactor)
            {
                return 10;
            }
            else if (block is IMyLargeMissileTurret || block is IMySmallMissileLauncher)
            {
                return 5;
            }
            else if (block is IMyLargeGatlingTurret || block is IMySmallGatlingGun)
            {
                return 3;
            }
            else if (block is IMyWindTurbine || block is IMySolarPanel || block is IMyBatteryBlock)
            {
                return 1;
            }
            else if (block is IMyBeacon || block is IMyRadioAntenna || block is IMyLaserAntenna)
            {
                return 1;
            }
            else if (block is IMyThrust)
            {
                return 1;
            }

            return 0;
        }

        private static bool IsHumanOwned(IMyCubeGrid grid, HashSet<long> knownPlayers)
        {
            bool isHumanOwned = false;
            foreach (var owner in grid.BigOwners)
            {
                if (knownPlayers.Contains(owner))
                {
                    isHumanOwned = true;
                    break;
                }
            }
            return isHumanOwned;
        }

        private void Entities_OnEntityAdd(IMyEntity entity)
        {
            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid == null)
            {
                return;
            }
            grid.OnBlockAdded += Grid_OnBlockAdded;
            grid.OnBlockRemoved += Grid_OnBlockRemoved;
            grid.OnMarkForClose += Grid_OnClose;
            SubscribedEntities.Add(grid);
        }
        private HashSet<long> GetPlayerHashes()
        {
            List<IMyPlayer> sessionPlayers = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(sessionPlayers);
            HashSet<long> PlayerIds = new HashSet<long>();
            PlayerIds.UnionWith(sessionPlayers.Select(x => x.Identity.IdentityId));
            foreach (IMyFaction faction in Factions.PlayerFactions)
            {
                PlayerIds.UnionWith(Factions.HumanFaction.Members.Select(x => x.Value.PlayerId));
            }
            return PlayerIds;
        }

        private void Grid_OnBlockAdded(IMySlimBlock block)
        {
            if (block.CubeGrid == null) { return; }
            if (block.FatBlock == null)
            {
                return;
            }
            IMyCubeGrid grid = block.CubeGrid;
            if (!IsHumanOwned(grid, GetPlayerHashes()))
            {
                return;
            }
            int points = GetPointsForBlock(block.FatBlock);
            int multiplayer = block.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large ? 3 : 1;
            Points += points * multiplayer;
        }

        private void Grid_OnBlockRemoved(IMySlimBlock block)
        {
            if (block.FatBlock == null)
            {
                return;
            }
            int points = GetPointsForBlock(block.FatBlock);
            int multiplayer = block.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large ? 3 : 1;
            Points -= points * multiplayer;
        }

        private void Grid_OnClose(IMyEntity entity)
        {
            IMyCubeGrid grid = entity as IMyCubeGrid;
            SubscribedEntities.Remove(grid);
            UnsubscribeGrid(grid);
        }
        private void UnsubscribeGrid(IMyCubeGrid grid)
        {
            if (grid != null)
            {
                grid.OnBlockAdded -= Grid_OnBlockAdded;
                grid.OnBlockRemoved -= Grid_OnBlockRemoved;
                grid.OnMarkForClose -= Grid_OnClose;
            }
        }
    }
}