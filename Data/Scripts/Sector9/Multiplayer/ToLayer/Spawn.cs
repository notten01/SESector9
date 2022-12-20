using ProtoBuf;

namespace Sector9.Data.Scripts.Sector9.Multiplayer.ToLayer
{
    [ProtoContract]
    public struct Spawn
    {
        [ProtoMember(1)]
        public string Name;
    }
}