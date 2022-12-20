using ProtoBuf;

namespace Sector9.Multiplayer
{
    public enum ToLayerType : int
    {
        Spawn,
        TestSpawn
    }

    [ProtoContract]
    public struct ToServerMessage
    {
        [ProtoMember(1)]
        public int LayerType;

        [ProtoMember(2)]
        public byte[] PayLoad;

        [ProtoMember(3)]
        public long FromPlayerId;
    }
}