using Sector9.Core.Logging;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi.Actions
{
    public class SaveUpAction : AiAction
    {
        public SaveUpAction() : base("Saving resources")
        {

        }

        public override double CalulateUtility(GameState gameState)
        {
            // Increase utility if resources are low
            double utility = (100 - gameState.Resources) * 0.5;
            // Decrease utility if there have been multiple consecutive saves
            utility -= gameState.IdleCycles * 0.5;
            return utility;
        }

        public override void Execute(GameState gameState)
        {
            Logger.Log("The ai is saving up resources", Logger.Severity.Info, Logger.LogType.Server);
            gameState.IdleCycles++;
        }

        public override int GetCost(GameState gameState)
        {
            return 0; //saving resources is free
        }
    }
}
