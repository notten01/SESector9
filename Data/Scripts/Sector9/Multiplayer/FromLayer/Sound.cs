using ProtoBuf;

namespace Sector9.Multiplayer.FromLayer
{
    [ProtoContract]
    public struct Sound
    {
        [ProtoMember(1)]
        public bool Queue { get; set; }

        [ProtoMember(2)]
        public string SoundName { get; set; }
    }
}