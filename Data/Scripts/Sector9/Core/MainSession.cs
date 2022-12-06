using Sandbox.ModAPI;
using Sector9.Client;
using Sector9.Multiplayer;
using Sector9.Server;
using VRage.Game.Components;

namespace Sector9.Core
{
    /// <summary>
    /// Contains the primary hookup point to the game api
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MainSession : MySessionComponentBase
    {
        private ServerSession ServerSession;
        private PlayerSession PlayerSession;
        private CommandHandler CommandHandler;
        private bool StartupComplete = false;
        private SyncManager SyncManager;

        public override void LoadData()
        {
            CommandHandler = new CommandHandler();
            SyncManager = new SyncManager(CommandHandler);
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                ServerSession = new ServerSession();
            }
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                PlayerSession = new PlayerSession();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (!StartupComplete)
            {
                Startup();
            }

            //todo: cycle tasks here
        }

        public override void SaveData()
        {
            ServerSession?.Save();
            PlayerSession?.Save();
        }

        private void Startup()
        {
            StartupComplete = true;
            //register sync system on messages
            MyAPIGateway.Utilities.MessageEntered += SyncManager.ChatMessageRecieved;
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(SyncManager.SyncId, SyncManager.NetworkMessageRecieved);
            MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, $"S9 version {S9Constants.Version} loaded!");
            MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, $"Running server: {ServerSession != null}. Running client: {PlayerSession != null}.");
        }
    }
}