using ProtoBuf;

namespace Sector9.Multiplayer
{
    [ProtoContract]
    internal class CommandMessage
    {
        [ProtoMember(1)]
        public ulong SenderSteamId;

        [ProtoMember(2)]
        public long SenderIdentityId;

        [ProtoMember(3)]
        public string Message;

        public CommandMessage()
        {
            SenderSteamId = 0;
            SenderIdentityId = 0;
            Message = "";
        }
    }
}