using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
using Sector9.Server.Targets;
using Sector9.Server.Units;
using Sector9.Server.Units.Behaviours;
using Sector9.Server.Units.Control;
using System.Collections.Generic;
using System.Linq;
using VRage.ModAPI;
using VRageMath;

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
                    Unit unit = new Unit(shipParts, Core.ServerSession.UnitCommander, "testspawn", Core.ServerSession.WeaponsCore, Core.ServerSession.BlockLibrary, new EncirkleTargetCaptain(new PlayerTarget(MyAPIGateway.Session.LocalHumanPlayer), Core.ServerSession.Planets), Core.ServerSession.Planets);
                    MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "Force spawned ship");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "Failed to spawn ship!");
                }
            }
            else if (command.Message.StartsWith("testSpawn"))
            {
                string[] parts = command.Message.Split(' ');
                if (parts.Length != 2)
                {
                    MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "Expected 1 argument (name of grid)");
                }
                List<IMyEntity> gridParts = Core.ServerSession.TestSpawn(parts.Last());
                if (gridParts != null)
                {
                    Vector3 movetoPos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
                    movetoPos.Add(Vector3D.Up * 20);
                    Unit unit = new Unit(gridParts, Core.ServerSession.UnitCommander, "testspawn", Core.ServerSession.WeaponsCore, Core.ServerSession.BlockLibrary, new MoveTo(movetoPos), Core.ServerSession.Planets);
                    MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, $"Spawned {parts.Last()} from prefabs folder for testing");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, $"Coudl not spawn {parts.Last()} from prefabs folder");
                }
            }
            else if (command.Message == "beep")
            {
                var player = MyAPIGateway.Session.LocalHumanPlayer;
                Core.PlayerSession?.SoundPlayer.PlaySoundInQueue(player.GetPosition(), "s9lurking1");
                MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "beep!");
            }
            else if (command.Message == "beep!")
            {
                var player = MyAPIGateway.Session.LocalHumanPlayer;
                Core.PlayerSession?.SoundPlayer.PlaySound(player.Controller.ControlledEntity.Entity, player.GetPosition(), "s9lurking2");
                MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "live beep!");
            }
        }
    }
}