using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
using Sector9.Server.Units;
using Sector9.Server.Units.Behaviours;
using System.Collections.Generic;
using VRage.ModAPI;

namespace Sector9.Core
{
    /// <summary>
    /// class handles commands received by text
    /// </summary>
    internal class CommandHandler
    {
        private readonly CoreSession Core;

        public CommandHandler(CoreSession core)
        {
            Core = core;
        }

        public void HandleCommand(CommandMessage command)
        {
            Logger.Log($"Handling custom message '{command.Message}'", Logger.Severity.Info, Logger.LogType.System);
            if (command.Message == "spawn")
            {
                List<IMyEntity> shipParts = Core.ServerSession.SpawnHostileShip();
                if (shipParts != null)
                {
                    Unit unit = new Unit(shipParts, Core.ServerSession.UnitCommander, "testspawn", Core.ServerSession.WeaponsCore, Core.ServerSession.BlockLibrary, new PassiveBehaviour());
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