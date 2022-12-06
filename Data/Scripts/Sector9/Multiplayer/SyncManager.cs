using Sandbox.ModAPI;
using Sector9.Core;

namespace Sector9.Multiplayer
{
    /// <summary>
    /// Class that will sync between client server (if multiplayer)...also known as nightmare.cs
    /// </summary>
    internal class SyncManager
    {
        public const int SyncId = 11557; //s9
        private readonly CommandHandler CommandHandler;

        public SyncManager(CommandHandler commandHandler)
        {
            CommandHandler = commandHandler;
        }

        public void ChatMessageRecieved(string recievedMessage, ref bool sendToOthers)
        {
            var player = MyAPIGateway.Session.LocalHumanPlayer;

            if (player == null)
            {
                return; //dafuq?
            }

            bool isServerMessage = recievedMessage.StartsWith("/s9");
            if (isServerMessage)
            {
                sendToOthers = false;
                var message = new CommandMessage();
                message.SenderSteamId = player.SteamUserId;
                message.SenderIdentityId = player.IdentityId;
                message.Message = recievedMessage;
                var serializedData = MyAPIGateway.Utilities.SerializeToBinary(message);
                MyAPIGateway.Multiplayer.SendMessageToServer(SyncId, serializedData);
            }
        }

        internal void NetworkMessageRecieved(ushort id, byte[] rawMessage, ulong senderId, bool ArrivedFromServer)
        {
            //todo: no idea what to do with those ulong and bool
            var message = MyAPIGateway.Utilities.SerializeFromBinary<CommandMessage>(rawMessage);
            MyAPIGateway.Utilities.ShowMessage("sys.sector9", $"Handling message {message}");
            CommandHandler.HandleCommand(message);
        }
    }
}