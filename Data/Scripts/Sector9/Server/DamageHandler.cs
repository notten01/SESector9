using Sandbox.ModAPI;
using Sector9.Core.Logging;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Sector9.Server
{
    public class DamageHandler
    {
        private readonly HashSet<long> TrackedGrids;

        public DamageHandler()
        {
            TrackedGrids = new HashSet<long>();
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(1, CheckGrindDamage);
        }

        private void CheckGrindDamage(object target, ref MyDamageInformation info)
        {
            if (info.Type == MyDamageType.Grind)
            {
                IMySlimBlock damagedBlock = target as IMySlimBlock;
                IMyCubeGrid damagedGrid = damagedBlock.CubeGrid;
                if (TrackedGrids.Contains(damagedGrid.EntityId) && !(damagedBlock.FatBlock is IMyRemoteControl))
                {
                    info.Amount = 0;
                }
            }
        }

        /// <summary>
        /// Start tracking the given grid id to stop grind damage
        /// </summary>
        /// <param name="id">Id of the entity to protect</param>
        public void TrackEntity(long id)
        {
            TrackedGrids.Add(id);
            Logger.Log($"Now protecting entity {id} from grind damage", Logger.Severity.Info, Logger.LogType.Server);
        }

        /// <summary>
        /// Stop preventing grind damge on grid
        /// </summary>
        /// <param name="id">Id of entity to stop protecting</param>
        public void UnTrackEntity(long id)
        {
            TrackedGrids.Remove(id);
            Logger.Log($"No longer protecting entity {id} from grind damage", Logger.Severity.Info, Logger.LogType.Server);
        }
    }
}