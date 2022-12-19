using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;

namespace Sector9.Multiplayer
{
    /// <summary>
    /// Class that will sync between client server (if multiplayer)...also known as nightmare.cs
    /// </summary>
    public class SyncManager
    {
        public const int SyncIdToServer = 11557; //s9
        public const int SyncIdToClient = 11558; //s9+1
        private readonly CommandHandler CommandHandler;
        private readonly CoreSession Session;

        public SyncManager(CommandHandler commandHandler, CoreSession session)
        {
            Session = session;
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
                var message = new ToServerMessage();
                message.SenderSteamId = player.SteamUserId;
                message.SenderIdentityId = player.IdentityId;
                message.Message = recievedMessage.Substring(4);
                var serializedData = MyAPIGateway.Utilities.SerializeToBinary(message);
                MyAPIGateway.Multiplayer.SendMessageToServer(SyncIdToServer, serializedData);
            }
        }

        /// <summary>
        /// Send a system message from the server to the clients
        /// </summary>
        /// <param name="message">Message payload</param>
        public void SendSystemMessage(FromServerMessage message)
        {
            Logger.Log($"Sending message type {message.PayloadType} to clients", Logger.Severity.Info, Logger.LogType.Server);
            var serializedData = MyAPIGateway.Utilities.SerializeToBinary(message);
            MyAPIGateway.Multiplayer.SendMessageToOthers(SyncIdToClient, serializedData);
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                MyAPIGateway.Multiplayer.SendMessageToServer(SyncIdToClient, serializedData); //also send to self if not dedicated
            }
        }

        /// <summary>
        /// Call when a message is received from the network (other client/server)
        /// </summary>
        /// <param name="id">id of sync client</param>
        /// <param name="rawMessage">serialized message</param>
        /// <param name="senderId">id is sender</param>
        /// <param name="ArrivedFromServer">is the message is from the server</param>
        internal void NetworkServerMessageRecieved(ushort id, byte[] rawMessage, ulong senderId, bool ArrivedFromServer)
        {
            var message = MyAPIGateway.Utilities.SerializeFromBinary<ToServerMessage>(rawMessage);
            CommandHandler.HandleCommand(message);
        }

        /// <summary>
        /// Call when a message is received from the network (other client/server)
        /// </summary>
        /// <param name="id">id of sync client</param>
        /// <param name="rawMessage">serialized message</param>
        /// <param name="senderId">id is sender</param>
        /// <param name="ArrivedFromServer">is the message is from the server</param>
        internal void NetworkClientMessageRecieved(ushort id, byte[] rawMessage, ulong senderId, bool ArrivedFromServer)
        {
            var message = MyAPIGateway.Utilities.SerializeFromBinary<FromServerMessage>(rawMessage);
            Session.ClientSession.HandleServerMessage(message);
        }
    }
}