using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Multiplayer;

namespace Sector9.Client
{
    internal class ClientMessageHandler
    {
        public ClientMessageHandler()
        {
        }

        public void HandleMessage(FromServerMessage message)
        {
            Logger.Log($"Handling client message of type {message.PayloadType}", Logger.Severity.Info, Logger.LogType.Client);

            switch (message.PayloadType)
            {
                case FromServerMessage.MessageType.Notification:
                    MyAPIGateway.Utilities.ShowNotification(message.Payload);
                    break;
            }
        }
    }
}