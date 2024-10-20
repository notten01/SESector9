using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi.Actions;
using Sector9.Server.Firewall;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi
{
    public class Ai
    {
        private readonly List<AiAction> Actions;
        public GameState GameState { get; }

        private const string cDataFileName = "AiState";


        public Ai(long humanStrength, WarpQueue warpQueue)
        {
            GameState = TryGetGameState(humanStrength);
            Actions = new List<AiAction>
            {
                new SaveUpAction(),
                new AttackFirewall(warpQueue),
                new EnhanceAction(),
                new BuildAction(warpQueue)
            };
        }

        public void TakeAction()
        {
            AiAction action = PickAction();
            action.Execute(GameState);
        }

        public void Save()
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(cDataFileName, typeof(GameState)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML<GameState>(GameState));
            }
        }

        public static GameState TryGetGameState(long humanStrength)
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cDataFileName, typeof(FirewallData)))
            {
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cDataFileName, typeof(FirewallData)))
                    {
                        GameState state = MyAPIGateway.Utilities.SerializeFromXML<GameState>(reader.ReadToEnd());
                        state.HumanStrength = humanStrength;
                        return state;
                    }
                }
                catch (InvalidOperationException)
                {
                    Logger.Log("The game ai settings could not be loaded for some reason, the game stage was reset to 1!", Logger.Severity.Error, Logger.LogType.System);
                }
            }
            return new GameState(humanStrength);
        }

        private AiAction PickAction()
        {
            var possibleActions = Actions.Where(x => x.GetCost(GameState) < GameState.Resources);
            AiAction bestAction = null;
            double highestUtility = double.MinValue;

            foreach (AiAction action in possibleActions)
            {
                double utility = action.CalulateUtility(GameState);
                if (utility > highestUtility)
                {
                    highestUtility = utility;
                    bestAction = action;
                }
            }

            return bestAction;
        }
    }
}
