using Sector9.Core;
using Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi;
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
        private readonly Ai Ai;
        private readonly GameState GameState;
        private readonly WarpQueue WarpQueue;

        private int cooldown = 3600;


        public OverallCommander(FactionManager factions)
        {
            Factions = factions;
            WarpQueue = new WarpQueue();
            Scanner = new PlayerScanner(Factions);
            Ai = new Ai(Scanner.Points, WarpQueue);
            GameState = Ai.GameState;
        }

        public long GetPoints()
        {
            return Scanner.Points;
        }

        public void Tick()
        {
            cooldown--;
            WarpQueue.Tick();
            if (cooldown == 0)
            {
                Ai.GameState.Resources += Scanner.Points;
                Ai.GameState.HumanStrength = Scanner.Points;
                Ai.TakeAction();
                cooldown = 3600;
            }
        }

        public void Shutdown()
        {
            Scanner?.Shutdown();
        }

        public void Save()
        {
            Ai.Save();
        }
    }
}
