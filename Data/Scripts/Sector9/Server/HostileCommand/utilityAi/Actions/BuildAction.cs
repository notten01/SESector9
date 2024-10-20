using Sector9.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi.Actions
{
    public class BuildAction : AiAction
    {
        private readonly WarpQueue WarpQueue;

        public BuildAction(WarpQueue warpQueue) : base("Building")
        {
            WarpQueue = warpQueue;
        }

        public override double CalulateUtility(GameState gameState)
        {
            return gameState.Resources * 0.5;
        }

        public override void Execute(GameState gameState)
        {
            Logger.Log("The ai has choosen to build a structure", Logger.Severity.Info, Logger.LogType.Server);
            gameState.IdleCycles = 0;
        }

        public override int GetCost(GameState gameState)
        {
            return 200 * (gameState.GameStage * 2);
        }
    }
}