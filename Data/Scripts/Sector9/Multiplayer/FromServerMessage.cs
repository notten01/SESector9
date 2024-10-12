using ProtoBuf;

namespace Sector9.Multiplayer
{
    public enum FromLayerType : int
    {
        Notification,
        Sound,
        Message,
        GameOver
    }

    [ProtoContract]
    public struct FromServerMessage
    {
        [ProtoMember(1)]
        public int LayerType;

        [ProtoMember(2)]
        public long? PlayerId;

        [ProtoMember(3)]
        public byte[] Payload;
    }
}