using Sector9.Core.Logging;
using System;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi.Actions
{
    public class EnhanceAction : AiAction
    {
        public EnhanceAction() : base("Enhance")
        {
        }

        public override double CalulateUtility(GameState gameState)
        {
            return gameState.Resources * 0.4 + (1.0 / (gameState.GameStage + 1)) * 0.6;
        }

        public override void Execute(GameState gameState)
        {
            Logger.Log("The ai has choosen to increase its efforts", Logger.Severity.Info, Logger.LogType.Server);
            gameState.IdleCycles = 0;
            gameState.GameStage++;
        }

        public override int GetCost(GameState gameState)
        {
            return (int)(1000 * Math.Pow(gameState.GameStage, 2));
        }
    }
}
