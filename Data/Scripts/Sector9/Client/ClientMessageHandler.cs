﻿using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
using Sector9.Multiplayer.FromLayer;

namespace Sector9.Client
{
    internal class ClientMessageHandler
    {
        private readonly ClientSession Session;

        public ClientMessageHandler(ClientSession session)
        {
            Session = session;
        }

        public void HandleIncommingCommand(FromServerMessage message)
        {
            //unlayer the pakcage
            FromLayerType layerEnumType = (FromLayerType)message.LayerType;
            switch (layerEnumType)
            {
                case FromLayerType.Notification:
                    HandleNotification(MyAPIGateway.Utilities.SerializeFromBinary<Notification>(message.Payload));
                    break;

                case FromLayerType.Message:
                    ShowTextMessage(MyAPIGateway.Utilities.SerializeFromBinary<TextMessage>(message.Payload));
                    break;

                case FromLayerType.Sound:
                    PlaySound(MyAPIGateway.Utilities.SerializeFromBinary<Sound>(message.Payload));
                    break;

                case FromLayerType.GameOver:
                    MyAPIGateway.Utilities.ShowMissionScreen("GAME OVER", "Your firewall was destroyed, the game has ended");
                    break;

                default:
                    Logger.Log($"System did not know how to handle message type {message.LayerType}", Logger.Severity.Error, Logger.LogType.Client);
                    break;
            }
        }

        private static void HandleNotification(Notification notification)
        {
            MyAPIGateway.Utilities.ShowNotification(notification.Message, notification.Duration, notification.Font);
        }

        private static void ShowTextMessage(TextMessage message)
        {
            MyAPIGateway.Utilities.ShowMessage(message.Sender, message.Text);
        }

        private void PlaySound(Sound sound)
        {
            Session.SoundPlayer.PlaySoundInQueue(MyAPIGateway.Session.LocalHumanPlayer.GetPosition(), sound.SoundName);
        }

        public void Startup()
        {
            MyAPIGateway.Utilities.MessageEntered += HandleOutgoingMessage;
        }

        private void HandleOutgoingMessage(string messageText, ref bool sendToOthers)
        {
            //todo, nothing yet
        }

        public void Shutdown()
        {
            MyAPIGateway.Utilities.MessageEntered -= HandleOutgoingMessage;
        }
    }
}