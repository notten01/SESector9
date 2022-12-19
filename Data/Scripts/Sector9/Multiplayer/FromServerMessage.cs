using ProtoBuf;

namespace Sector9.Multiplayer
{
    [ProtoContract]
    public class FromServerMessage
    {
        public enum MessageType
        {
            Notification,
            Sound
        }

        [ProtoMember(1)]
        public MessageType PayloadType;

        [ProtoMember(2)]
        public string Payload;

        public FromServerMessage()
        {
            PayloadType = MessageType.Notification;
            Payload = "";
        }
    }
}