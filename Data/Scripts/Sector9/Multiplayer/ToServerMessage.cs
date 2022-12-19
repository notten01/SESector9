using ProtoBuf;

namespace Sector9.Multiplayer
{
    [ProtoContract]
    public class ToServerMessage
    {
        [ProtoMember(1)]
        public ulong SenderSteamId;

        [ProtoMember(2)]
        public long SenderIdentityId;

        [ProtoMember(3)]
        public string Message;

        public ToServerMessage()
        {
            SenderSteamId = 0;
            SenderIdentityId = 0;
            Message = "";
        }
    }
}