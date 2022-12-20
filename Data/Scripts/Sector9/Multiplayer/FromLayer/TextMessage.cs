using ProtoBuf;

namespace Sector9.Multiplayer.FromLayer
{
    [ProtoContract]
    public struct TextMessage
    {
        [ProtoMember(1)]
        public string Sender;

        [ProtoMember(2)]
        public string Text;
    }
}