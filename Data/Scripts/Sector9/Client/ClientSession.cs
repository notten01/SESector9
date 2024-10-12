using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
using Sector9.Server;
using System;
using VRage.Scripting;

namespace Sector9.Client
{
    internal enum FirewallState
    {
        Operational,
        Broken,
        Missing
    }

    /// <summary>
    /// session handing player related items. Can existin togheter with <see cref="ServerSession"/> if its locally hosted
    /// </summary>
    public class ClientSession : ITickable
    {
        private const string cDataFileName = "S9PlayerSession.xml";
        private ClientData Data;
        public SoundPlayer SoundPlayer { get; }
        private ClientMessageHandler MessageHandler;
        private bool FirstTime = false;
        //public ClientHud Hud { get; }

        public ClientSession()
        {
            TryLoad();
            MessageHandler = new ClientMessageHandler(this);
            SoundPlayer = new SoundPlayer();
            //Hud = new ClientHud(this);
        }

        public bool EnableLog => Data.EnableLog;

        public void Save()
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(cDataFileName, typeof(ClientData)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML<ClientData>(Data));
            }
        }

        public void HandleServerMessage(FromServerMessage message)
        {
            MessageHandler.HandleIncommingCommand(message);
        }

        public void Tick()
        {
            //Hud.Tick();
            SoundPlayer.Tick();
        }

        public void Startup(SyncManager syncManager)
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(SyncManager.SyncIdToClient, syncManager.NetworkClientMessageRecieved);
            MessageHandler.Startup();
            if (FirstTime)
            {
                SoundPlayer.PlaySoundInQueue(MyAPIGateway.Session.Player.GetPosition(), "first-greeting");
            }
            MyAPIGateway.Utilities.ShowMessage("B.O.B", "Use /eva help for more suit information and functionality");
        }

        public void Shutdown(SyncManager syncManager)
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(SyncManager.SyncIdToClient, syncManager.NetworkClientMessageRecieved);
            MessageHandler.Shutdown();
        }

        private void TryLoad()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cDataFileName, typeof(ClientData)))
            {
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cDataFileName, typeof(ClientData)))
                    {
                        Data = MyAPIGateway.Utilities.SerializeFromXML<ClientData>(reader.ReadToEnd());
                    }
                }
                catch (InvalidOperationException)
                {
                    Logger.Log("The ClientSession settings could not be loaded for some reason, they have been overwritten with the default!", Logger.Severity.Error, Logger.LogType.System);
                }
            }
            if (Data == null)
            {
                Data = new ClientData();
                FirstTime = true;
            }
            Data.Version = S9Constants.Version;
        }
    }
}