namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi.Actions
{
    public abstract class AiAction
    {
        public string Name { get; private set; }
        protected AiAction(string name)
        {
            Name = name;
        }

        public abstract double CalulateUtility(GameState gameState);

        public abstract void Execute(GameState gameState);

        public abstract int GetCost(GameState gameState);
    }
}
