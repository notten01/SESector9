using Sandbox.ModAPI;
using Sector9.Client;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
using Sector9.Server;
using VRage.Game.Components;

namespace Sector9.Core
{
    /// <summary>
    /// Contains the primary hookup point to the game api
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CoreSession : MySessionComponentBase
    {
        public ServerSession ServerSession { get; private set; }
        public PlayerSession PlayerSession { get; private set; }
        private CommandHandler CommandHandler;
        private bool StartupComplete = false;
        private SyncManager SyncManager;
        private int LogRotationCounter = 0;

        public override void BeforeStart()
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                ServerSession = new ServerSession();
            }
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                PlayerSession = new PlayerSession();
            }
            CommandHandler = new CommandHandler(this);
            SyncManager = new SyncManager(CommandHandler);
            Logger.SetLogTypes(ServerSession?.EnableLog == true, PlayerSession?.EnableLog == true);
            Logger.Log($"Startup complete! Server: {ServerSession != null}. Player: {PlayerSession != null}. version: {S9Constants.Version}", Logger.Severity.Info, Logger.LogType.System);
            Logger.CycleLogs();
        }

        public override void UpdateBeforeSimulation()
        {
            if (!StartupComplete)
            {
                Startup();
            }

            ServerSession?.Tick();
            PlayerSession?.Tick();

            RotateLog();
        }

        private void RotateLog()
        {
            LogRotationCounter++;
            if (LogRotationCounter > 600) //600 rotations is 1 second if simspeed == 1
            {
                Logger.CycleLogs();
                LogRotationCounter = 0;
            }
        }

        public override void SaveData()
        {
            ServerSession?.Save();
            PlayerSession?.Save();
            Logger.Log("Saved session", Logger.Severity.Info, Logger.LogType.System);
        }

        protected override void UnloadData()
        {
            Logger.Log("Shutting down", Logger.Severity.Info, Logger.LogType.System);
            if (SyncManager != null)
            {
                MyAPIGateway.Utilities.MessageEntered -= SyncManager.ChatMessageRecieved;
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(SyncManager.SyncId, SyncManager.NetworkMessageRecieved);
            }
            ServerSession?.Shutdown();
            Logger.CycleLogsBlocking();
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