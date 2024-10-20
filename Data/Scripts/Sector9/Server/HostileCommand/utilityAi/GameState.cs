namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi
{
    public class GameState
    {
        public long Resources { get; set; }
        public int GameStage { get; set; }
        public int IdleCycles { get; set; }
        public int Cooldown { get; set; }
        public long HumanStrength { get; set; }

        public GameState(long humanStrength)
        {
            Resources = 0;
            GameStage = 1;
            IdleCycles = 0;
            Cooldown = 0;
            HumanStrength = humanStrength;
        }
    }
}
