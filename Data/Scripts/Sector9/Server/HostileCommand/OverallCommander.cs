using Sector9.Core;
using Sector9.Server;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand
{
    /// <summary>
    /// Class containing the primary commander that judges player strength and supplies the sub commanders with resources
    /// </summary>
    public class OverallCommander : ITickable
    {
        private readonly FactionManager Factions;
        private readonly PlayerScanner Scanner;

        public OverallCommander(FactionManager factions)
        {
            Factions = factions;
            Scanner = new PlayerScanner(Factions);
        }

        public long GetPoints()
        {
            return Scanner.Points;
        }

        public void Tick()
        {
        }

        public void Shutdown()
        {
            Scanner?.Shutdown();
        }
    }
}
