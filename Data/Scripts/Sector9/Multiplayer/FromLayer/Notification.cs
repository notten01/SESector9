using ProtoBuf;

namespace Sector9.Multiplayer.FromLayer
{
    [ProtoContract]
    public struct Notification
    {
        [ProtoMember(1)]
        public string Message;

        [ProtoMember(2)]
        public int Duration;

        [ProtoMember(3)]
        public string Font;
    }
}