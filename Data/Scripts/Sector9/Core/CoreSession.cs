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
        public ClientSession ClientSession { get; private set; }
        private bool StartupComplete = false;
        public SyncManager SyncManager { get; private set; }
        private int LogRotationCounter = 0;

        /// <summary>
        /// Gets the instance of the core mod session.
        /// BUT ONLY AFTER THE BEFORESTART HAS RUN!
        /// </summary>
        public static CoreSession Instance { get; private set; }

        public override void BeforeStart()
        {
            Instance = this;
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                ServerSession = new ServerSession(this);
            }
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                ClientSession = new ClientSession();
            }
            SyncManager = new SyncManager(this);
            Logger.SetLogTypes(ServerSession?.EnableLog == true, ClientSession?.EnableLog == true);
            Logger.Log($"Startup complete! Server: {ServerSession != null}. Player: {ClientSession != null}. version: {S9Constants.Version}", Logger.Severity.Info, Logger.LogType.System);
            Logger.CycleLogs();
        }

        public override void UpdateBeforeSimulation()
        {
            if (!StartupComplete)
            {
                Startup();
            }

            ServerSession?.Tick();
            ClientSession?.Tick();

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
            ClientSession?.Save();
            Logger.Log("Saved session", Logger.Severity.Info, Logger.LogType.System);
        }

        protected override void UnloadData()
        {
            Logger.Log("Shutting down", Logger.Severity.Info, Logger.LogType.System);
            ServerSession?.Shutdown(SyncManager);
            ClientSession?.Shutdown(SyncManager);
            Logger.CycleLogsBlocking();
        }

        private void Startup()
        {
            StartupComplete = true;
            //register sync system on messages
            ServerSession?.Startup(SyncManager);
            ClientSession?.Startup(SyncManager);
            MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, $"S9 version {S9Constants.Version} loaded!");
            MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, $"Running server: {ServerSession != null}. Running client: {ClientSession != null}.");
        }
    }
}