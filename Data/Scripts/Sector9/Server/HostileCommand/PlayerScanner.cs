using ParallelTasks;
using Sandbox.ModAPI;
using Sector9.Multiplayer;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand
{
    /// <summary>
    /// scans faction for strength
    /// </summary>
    public class PlayerScanner
    {
        private readonly List<IMyCubeGrid> ClonedGrids;
        private readonly HashSet<long> HumanFactionPlayers;

        public long Points { get; private set; }

        public PlayerScanner(List<IMyFaction> playerFactions)
        {
            ClonedGrids = new List<IMyCubeGrid>();
            //clone existing world to memory
            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, x => x is IMyCubeGrid);
            HumanFactionPlayers = new HashSet<long>();
            foreach(IMyFaction faction in playerFactions)
            {
                HumanFactionPlayers.UnionWith(faction.Members.Keys);
            }

            foreach (var entity in entities)
            {
                IMyCubeGrid cubeGrid = (IMyCubeGrid)entity;
                ClonedGrids.Add((IMyCubeGrid)MyAPIGateway.Entities.CreateFromObjectBuilder(cubeGrid.GetObjectBuilder()));
            }
        }

        public void BeginScan(WorkData data)
        {
            long points = 0;
            SyncManager.Instance.SendMessageFromServer($"Scanning {ClonedGrids.Count} grids");
            foreach (var entity in ClonedGrids)
            {
                int multiplayer = entity.GridSizeEnum == VRage.Game.MyCubeSize.Large ? 3 : 1;
                if (!entity.BigOwners.Exists(x => HumanFactionPlayers.Contains(x)))
                {
                    SyncManager.Instance.SendMessageFromServer($"skipping grid, nonehuman");
                    continue; //not a human grid
                }
                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                entity.GetBlocks(blocks);
                SyncManager.Instance.SendMessageFromServer($"Scanning {blocks.Count} blocks");
                foreach (var block in blocks)
                {
                    if (block.FatBlock == null)
                    {
                        continue;
                    }
                    IMyCubeBlock fatBlock = block.FatBlock;

                    //done prettier things then this if statement...
                    if (fatBlock is IMyRefinery)
                    {
                        //refinary
                        points += 10 * multiplayer;
                    }
                    else if (fatBlock is IMyAssembler)
                    {
                        points += 15 * multiplayer;
                    }
                    else if (fatBlock is IMyCockpit)
                    {
                        points += 5 * multiplayer;
                    }
                    else if (fatBlock is IMyCargoContainer || fatBlock is IMyGasTank)
                    {
                        points += 1 * multiplayer;
                    }
                    else if (fatBlock is IMyGasGenerator)
                    {
                        points += 5 * multiplayer;
                    }

                }
            }
            Points = points;
        }



    }
}
