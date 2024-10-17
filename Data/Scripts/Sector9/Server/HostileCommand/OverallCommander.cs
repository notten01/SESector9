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

        private long CurrentPoints; //todo: make persistant
        private int cooldown = 36000;
        private int resourceTick = 3600;


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
            cooldown--;
            resourceTick--;
            if (resourceTick == 0)
            {
                CurrentPoints += Scanner.Points;
                resourceTick = 3600;
            }
            if (cooldown > 0)
            {
                return; // no action needed yet
            }
            cooldown = NextAction();
        }

        public void Shutdown()
        {
            Scanner?.Shutdown();
        }

        public int NextAction()
        {
            return 36000;
        }
    }
}
