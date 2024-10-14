using ParallelTasks;
using Sandbox.ModAPI;
using Sector9.Multiplayer;
using Sector9.Server;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand
{
    /// <summary>
    /// Class containing the primary commander that judges player strength and supplies the sub commanders with resources
    /// </summary>
    public class OverallCommander
    {
        private readonly FactionManager Factions;
        private PlayerScanner Scanner;
        private long Points;

        public OverallCommander(FactionManager factions)
        {
            Factions = factions;
        }

        public void RunFullScan()
        {
            if (Scanner != null)
            {
                //scan is already running
                return;
            }

            Scanner = new PlayerScanner(Factions.PlayerFactions);
            MyAPIGateway.Parallel.Start(Scanner.BeginScan, ScanComplete, null);
        }

        private void ScanComplete(WorkData data)
        {
            SyncManager.Instance.SendMessageFromServer($"Faction score is now {Scanner.Points}");
            Points = Scanner.Points;
            Scanner = null;
        }
    }
}
