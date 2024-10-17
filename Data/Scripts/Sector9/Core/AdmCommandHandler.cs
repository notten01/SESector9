using ParallelTasks;
using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Data.Scripts.Sector9.Multiplayer.ToLayer;
using Sector9.Multiplayer;
using Sector9.Server;
using Sector9.Server.Buildings;
using Sector9.Server.Targets;
using Sector9.Server.Units;
using System.Collections.Generic;
using VRage.Game.ModAPI;
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

                case ToLayerType.TestCommand:
                    HandleDebugStuff(command.FromPlayerId, MyAPIGateway.Utilities.SerializeFromBinary<TestCommand>(command.PayLoad));
                        break;

                default:
                    Logger.Log($"Did not know how to handle to layer type {type}", Logger.Severity.Error, Logger.LogType.Server);
                    break;
            }
        }

        private void HandleDebugStuff(long playerId, TestCommand command)
        {
            switch(command.Command)
            {
                case "points":
                    SyncManager.Instance.SendMessageFromServer($"Current thread score is now {Core.ServerSession.GetPlayerPoints()}", playerId);
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

            Vector3 pos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition(); //bug: should be given in the spawn info
            pos += MyAPIGateway.Session.LocalHumanPlayer.Character.WorldMatrix.Forward * 2200;
            Core.ServerSession.SpawnHostileShip(content.Name, SpawnEnemyCallback, pos);
        }

        private void SpawnEnemyCallback(WorkData data)
        {
            ServerSession.HostileCallback callbackData = (ServerSession.HostileCallback)data;
            if (callbackData.AllGrids != null)
            {
                Unit unit = new Unit(callbackData.AllGrids, Core.ServerSession.UnitCommander, "testspawn", Core.ServerSession.WeaponsCore, Core.ServerSession.BlockLibrary, Core.ServerSession.Planets, MyAPIGateway.Session.LocalHumanPlayer.Character);
                SyncManager.Instance.SendMessageFromServer($"Force spawned ship");
            }
            else
            {
                SyncManager.Instance.SendMessageFromServer($"Failed to force spawn ship");
            }
        }

        private void SpawnTestGrid(Spawn content, long playerId)
        {
            if (!EnsureAdmin(playerId)) { return; }

            Core.ServerSession.TestSpawn(content.Name, SpawnTestgridCallback);
        }

        private static void SpawnTestgridCallback(WorkData data)
        {
            ServerSession.TestSpawnWrapper wrapper = (ServerSession.TestSpawnWrapper)data;
            if (wrapper.AllGrids != null)
            {
                SyncManager.Instance.SendMessageFromServer($"Spawned for testing");
            }
            else
            {
                SyncManager.Instance.SendMessageFromServer($"could not complete test spawn");
            }
        }

        public void SpawnTestBuilding(Spawn content, long playerId)
        {
            if (!EnsureAdmin(playerId)) { return; }

            Core.ServerSession.TestBuild(content.Name, BuildTestGridCallback);

        }

        private void BuildTestGridCallback(WorkData data)
        {
            ServerSession.TestSpawnWrapper wrapper = (ServerSession.TestSpawnWrapper)data;
            if (wrapper.AllGrids != null)
            {
                Building building = new Building(wrapper.AllGrids, Core.ServerSession.BuildingCommander, "testspawn", Core.ServerSession.WeaponsCore, Core.ServerSession.BlockLibrary);
                SyncManager.Instance.SendMessageFromServer("Build test grid on player");
            }
            else
            {
                SyncManager.Instance.SendMessageFromServer("Failed to build test grid");
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
            if (parts.Length == 2 && parts[1] == "points")
            {
                TestCommand test = new TestCommand() { Command = "points" };
                SyncManager.Instance.SendPayloadToServer(ToLayerType.TestCommand, test);
            }
            else if (parts.Length == 3 && parts[1] == "testSpawn")
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