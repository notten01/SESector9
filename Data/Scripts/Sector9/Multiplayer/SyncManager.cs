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

        /// <summary>
        /// call when a chat message (so from local client to sync) is received
        /// </summary>
        /// <param name="recievedMessage">Message that was entered by user</param>
        /// <param name="sendToOthers">Send message as text to other players</param>
        public void ChatMessageRecieved(string recievedMessage, ref bool sendToOthers)
        {
            var player = MyAPIGateway.Session.LocalHumanPlayer;

            if (player == null)
            {
                return; //dafuq?
            }

            bool isServerMessage = recievedMessage.StartsWith("/s9 ");
            if (isServerMessage)
            {
                sendToOthers = false;
                var message = new CommandMessage();
                message.SenderSteamId = player.SteamUserId;
                message.SenderIdentityId = player.IdentityId;
                message.Message = recievedMessage.Substring(4);
                var serializedData = MyAPIGateway.Utilities.SerializeToBinary(message);
                MyAPIGateway.Multiplayer.SendMessageToServer(SyncId, serializedData);
            }
        }

        /// <summary>
        /// Call when a message is received from the network (other client/server)
        /// </summary>
        /// <param name="id">id of sync client</param>
        /// <param name="rawMessage">serialized message</param>
        /// <param name="senderId">id is sender</param>
        /// <param name="ArrivedFromServer">is the message is from the server</param>
        internal void NetworkMessageRecieved(ushort id, byte[] rawMessage, ulong senderId, bool ArrivedFromServer)
        {
            //todo: no idea what to do with those ulong and bool
            var message = MyAPIGateway.Utilities.SerializeFromBinary<CommandMessage>(rawMessage);
            MyAPIGateway.Utilities.ShowMessage("sys.sector9", $"Handling message {message.Message}");
            CommandHandler.HandleCommand(message);
        }
    }
}