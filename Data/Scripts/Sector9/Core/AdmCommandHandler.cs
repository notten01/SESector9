using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Data.Scripts.Sector9.Multiplayer.ToLayer;
using Sector9.Multiplayer;
using Sector9.Server.Buildings;
using Sector9.Server.Targets;
using Sector9.Server.Units;
using Sector9.Server.Units.Behaviours;
using Sector9.Server.Units.Control;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Sector9.Core
{
    /// <summary>
    /// class handles commands received by text
    /// </summary>
    internal class AdmCommandHandler
    {
        private readonly CoreSession Core;

        public AdmCommandHandler(CoreSession core)
        {
            Core = core;
        }

        public void HandleIncommingCommand(ToServerMessage command)
        {
            Logger.Log($"Handling custom message type '{command.LayerType}'", Logger.Severity.Info, Logger.LogType.System);
            ToLayerType type = (ToLayerType)command.LayerType;
            switch (type)
            {
                case ToLayerType.Spawn:
                    SpawnEnemy(MyAPIGateway.Utilities.SerializeFromBinary<Spawn>(command.PayLoad), command.FromPlayerId);
                    break;

                case ToLayerType.TestSpawn:
                    SpawnTestGrid(MyAPIGateway.Utilities.SerializeFromBinary<Spawn>(command.PayLoad), command.FromPlayerId);
                    break;

                case ToLayerType.TestBuild:
                    SpawnTestBuilding(MyAPIGateway.Utilities.SerializeFromBinary<Spawn>(command.PayLoad), command.FromPlayerId);
                    break;

                case ToLayerType.ResetCountdown:
                    ResetFirewallCountdown(command.FromPlayerId);
                    break;

                default:
                    Logger.Log($"Did not know how to handle to layer type {type}", Logger.Severity.Error, Logger.LogType.Server);
                    break;
            }
        }

        private void ResetFirewallCountdown(long playerId)
        {
            Core.ServerSession.Firewall.ResetCountdown();
            SyncManager.Instance.SendMessageFromServer($"Firewall countdown is reset", playerId);
        }

        private void SpawnEnemy(Spawn content, long playerId)
        {
            if (!EnsureAdmin(playerId)) { return; }

            List<IMyEntity> shipParts = Core.ServerSession.SpawnHostileShip(content.Name);
            if (shipParts != null)
            {
                Unit unit = new Unit(shipParts, Core.ServerSession.UnitCommander, "testspawn", Core.ServerSession.WeaponsCore, Core.ServerSession.BlockLibrary, new EncirkleTargetCaptain(new PlayerTarget(MyAPIGateway.Session.LocalHumanPlayer), Core.ServerSession.Planets), Core.ServerSession.Planets);
                SyncManager.Instance.SendMessageFromServer($"Force spawned ship {content.Name}", playerId);
            }
            else
            {
                SyncManager.Instance.SendMessageFromServer($"Failed to force spawn ship {content.Name}!", playerId);
            }
        }

        private void SpawnTestGrid(Spawn content, long playerId)
        {
            if (!EnsureAdmin(playerId)) { return; }

            List<IMyEntity> gridParts = Core.ServerSession.TestSpawn(content.Name);
            if (gridParts != null)
            {
                Vector3 movetoPos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
                movetoPos.Add(Vector3D.Up * 20);
                Unit unit = new Unit(gridParts, Core.ServerSession.UnitCommander, "testspawn", Core.ServerSession.WeaponsCore, Core.ServerSession.BlockLibrary, new MoveTo(movetoPos), Core.ServerSession.Planets);
                SyncManager.Instance.SendMessageFromServer($"Spawned {content.Name} from prefabs folder for testing", playerId);
            }
            else
            {
                SyncManager.Instance.SendMessageFromServer($"Could not spawn {content.Name} from prefab folder, most likely the file does not exist (typo in name?)", playerId);
            }
        }

        public void SpawnTestBuilding(Spawn content, long playerId)
        {
            if (!EnsureAdmin(playerId)) { return; }

            List<IMyEntity> gridParts = Core.ServerSession.TestBuild(content.Name);
            if (gridParts != null)
            {
                Building building = new Building(gridParts, Core.ServerSession.BuildingCommander, "testspawn", Core.ServerSession.WeaponsCore, Core.ServerSession.BlockLibrary);
                SyncManager.Instance.SendMessageFromServer($"Spawned {content.Name} from prefabs folder for testing", playerId);
            }
            else
            {
                SyncManager.Instance.SendMessageFromServer($"Could not spawn {content.Name} from prefab folder, most likely the file does not exist (typo in name?)", playerId);
            }
        }

        private static bool EnsureAdmin(long id)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, x => x.IdentityId == id);
            bool isAdmin = (players.Count != 0 && players[0].PromoteLevel == MyPromoteLevel.Admin || players[0].PromoteLevel == MyPromoteLevel.Owner);
            if (!isAdmin)
            {
                SyncManager.Instance.SendMessageFromServer("You have to be moderator to do this!", id);
            }
            return isAdmin;
        }

        public void Startup()
        {
            MyAPIGateway.Utilities.MessageEntered += HandleOutgoingMessage;
        }

        private void HandleOutgoingMessage(string messageText, ref bool sendToOthers)
        {
            if (!messageText.StartsWith("/s9adm ")) { return; } //not for us to handle

            sendToOthers = false; //don't send the messages, others dont' have to see it
            string[] parts = messageText.Split(' ');
            if (parts.Length == 3 && parts[1] == "testSpawn")
            {
                Spawn spawn = new Spawn() { Name = parts[2] };
                SyncManager.Instance.SendPayloadToServer(ToLayerType.TestSpawn, spawn);
            }
            else if (parts.Length == 3 && parts[1] == "spawn")
            {
                Spawn spawn = new Spawn() { Name = parts[2] };
                SyncManager.Instance.SendPayloadToServer(ToLayerType.Spawn, spawn);
            }
            else if (parts.Length == 3 && parts[1] == "testBuild")
            {
                Spawn spawn = new Spawn() { Name = parts[2] };
                SyncManager.Instance.SendPayloadToServer(ToLayerType.TestBuild, spawn);
            }
            else if (parts.Length == 3 && parts[1] == "reset" && parts[2] == "countdown")
            {
                SyncManager.Instance.SendPayloadToServer(ToLayerType.ResetCountdown, new ResetCountdown());
            }
            else
            {
                SyncManager.Instance.SendMessageFromServer("Unknown command, try /s9adm help", MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
            }
        }

        public void Shutdown()
        {
            MyAPIGateway.Utilities.MessageEntered -= HandleOutgoingMessage;
        }
    }
}