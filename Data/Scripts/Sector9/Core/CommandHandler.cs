using Sandbox.ModAPI;
using Sector9.Client;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
using Sector9.Server;

namespace Sector9.Core
{
    /// <summary>
    /// class handles commands received by text
    /// </summary>
    internal class CommandHandler
    {
        private readonly ServerSession Server;
        private readonly PlayerSession Player;

        public CommandHandler(ServerSession server, PlayerSession player)
        {
            Server = server;
            Player = player;
        }

        public void HandleCommand(CommandMessage command)
        {
            Logger.Log($"Handling custom message '{command.Message}'", Logger.Severity.Info, Logger.LogType.System);
            if (command.Message == "spawn")
            {
                if (Server.SpawnHostileShip())
                {
                    MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "Force spawned ship");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "Failed to spawn ship!");
                }
            }
        }
    }
}