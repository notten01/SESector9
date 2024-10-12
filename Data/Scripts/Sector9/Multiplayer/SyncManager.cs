using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Multiplayer.FromLayer;

namespace Sector9.Multiplayer
{
    /// <summary>
    /// Class that will sync between client server (if multiplayer)...also known as nightmare.cs
    /// </summary>
    internal class SyncManager
    {
        public const int SyncIdToServer = 11557; //s9
        public const int SyncIdToClient = 11558; //s9+1
        private readonly CoreSession Session;

        public static SyncManager Instance { get; private set; }

        public SyncManager(CoreSession session)
        {
            Session = session;
            Instance = this;
        }

        /// <summary>
        /// Send a content to the server
        /// </summary>
        /// <param name="payload">Playload struct you want to ship</param>
        public void SendPayloadToServer(ToLayerType type, object payload)
        {
            ToServerMessage wrapper = new ToServerMessage() { FromPlayerId = MyAPIGateway.Session.LocalHumanPlayer.IdentityId, LayerType = (int)type, PayLoad = MyAPIGateway.Utilities.SerializeToBinary(payload) };
            MyAPIGateway.Multiplayer.SendMessageToServer(SyncIdToServer, MyAPIGateway.Utilities.SerializeToBinary(wrapper));
        }

        /// <summary>
        /// Send a payload from the server to one or more clients
        /// </summary>
        /// <param name="type">Type of payload to send</param>
        /// <param name="payload">payload itself</param>
        /// <param name="playerId">optional player id if the message should only go to that player</param>
        public void SendPayloadFromServer(FromLayerType type, object payload, long? playerId = null)
        {
            FromServerMessage wrapper = new FromServerMessage() { PlayerId = playerId, LayerType = (int)type, Payload = MyAPIGateway.Utilities.SerializeToBinary(payload) };
            SendServerPayload(wrapper);
        }

        /// <summary>
        /// Send a chatlog message to one or more players
        /// </summary>
        /// <param name="content">Message to send</param>
        /// <param name="palyerId">Optional player id if you only want to send it to a specific person</param>
        public void SendMessageFromServer(string content, long? palyerId = null)
        {
            TextMessage message = new TextMessage() { Sender = S9Constants.SystemName, Text = content };
            FromServerMessage wrapper = new FromServerMessage() { LayerType = (int)FromLayerType.Message, Payload = MyAPIGateway.Utilities.SerializeToBinary(message), PlayerId = palyerId };
            SendServerPayload(wrapper);
        }

        private void SendServerPayload(FromServerMessage content)
        {
            var serializedData = MyAPIGateway.Utilities.SerializeToBinary(content);
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
            Session.ServerSession.HandleServerMessage(message);
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
            if (message.PlayerId == null || message.PlayerId == MyAPIGateway.Session.LocalHumanPlayer.IdentityId)
            {
                Session.ClientSession.HandleServerMessage(message);
            }
        }
    }
}