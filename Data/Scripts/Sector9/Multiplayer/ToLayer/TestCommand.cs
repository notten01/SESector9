using ProtoBuf;

namespace Sector9.Data.Scripts.Sector9.Multiplayer.ToLayer
{
    [ProtoContract]
    public struct TestCommand
    {
        [ProtoMember(1)]
        public string Command { get; set; }
    }
}
