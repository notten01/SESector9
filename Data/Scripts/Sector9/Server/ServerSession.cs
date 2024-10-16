using ParallelTasks;
using Sandbox.ModAPI;
using Sector9.Api;
using Sector9.Core;
using Sector9.Core.Logging;
using Sector9.Data.Scripts.Sector9.Server.HostileCommand;
using Sector9.Multiplayer;
using Sector9.Server.Buildings;
using Sector9.Server.FireWall;
using Sector9.Server.Units;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Sector9.Server
{
    /// <summary>
    /// Session for the server, keeping track of game stats. Can exist togheter with <see cref="PlayerSession"/> if its a locally hosted game
    /// </summary>
    public class ServerSession : ITickable
    {
        private const string cDataFileName = "S9ServerSession.xml";
        private readonly FactionManager Factions;
        public FirewallHandler Firewall { get; private set; }
        private readonly GridSpawner Spawner;
        private readonly DamageHandler DamageHandler;
        private readonly AdmCommandHandler CommandHandler;
        private readonly OverallCommander Commander;


        private ServerData Data;

        public ServerSession(CoreSession core)
        {
            TryLoad();
            DamageHandler = new DamageHandler();
            Factions = new FactionManager();
            Planets = new Planets();
            Spawner = new GridSpawner(Planets);
            Firewall = new FirewallHandler(Factions);
            WeaponsCore = new Wc();
            WeaponsCore.Load(null, true);
            BlockLibrary = new DefinitionLibrary(WeaponsCore);
            UnitCommander = new UnitCommander(DamageHandler);
            BuildingCommander = new BuildingCommander(DamageHandler);
            Commander = new OverallCommander(Factions);
            CommandHandler = new AdmCommandHandler(core);
        }

        public DefinitionLibrary BlockLibrary { get; private set; }
        public bool EnableLog => Data.EnableLog;
        public Planets Planets { get; }
        public UnitCommander UnitCommander { get; }
        public BuildingCommander BuildingCommander { get; }
        public Wc WeaponsCore { get; }

        public void Save()
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(cDataFileName, typeof(ServerData)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML<ServerData>(Data));
            }
            Factions.Save();
            Firewall.Save();
        }

        //todo: test code
        public long GetPlayerPoints()
        {
            return Commander.GetPoints();
        }

        /// <summary>
        /// Dispose all events and or session based stuff
        /// </summary>
        public void Shutdown(SyncManager syncManager)
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(SyncManager.SyncIdToServer, syncManager.NetworkServerMessageRecieved);
            CommandHandler.Shutdown();
            Factions.Shutdown();
            WeaponsCore.Unload();
            Firewall.Shutdown();
            Commander.Shutdown();
        }

        /// <summary>
        /// Spawn a ship belonging to the hostile faction
        /// </summary>
        /// <param name="shipName">Name of the prefab to spawn</param>
        /// <param name="callback">Callback when grid with subgrids is ready</param>
        /// <returns>True or false is the spawning was successfull</returns>
        public void SpawnHostileShip(string shipName, Action<WorkData> callback, Vector3 desiredPostion)
        {
            var positionMatrix = MatrixD.CreateWorld(desiredPostion, Vector3D.Forward, Vector3D.Up);
            List<IMyEntity> createdGrids;
            HostileCallback wrapper = new HostileCallback(Factions, callback);
            wrapper.TotalGrids = Spawner.TrySpawnGrid(shipName, positionMatrix, 500, out createdGrids, wrapper.Run);
        }

        public sealed class HostileCallback : WorkData
        {
            private readonly FactionManager Factions;
            public Action<IMyEntity> Run { get; }

            public List<IMyEntity> AllGrids { get; private set; }
            public int TotalGrids { get; set; }
            private int ReportedGrids = 0;
            public bool Ready { get; private set; }
            public Action<WorkData> FullCallback { get; }

            private readonly object SpawnRequestLock = new object();

            public HostileCallback(FactionManager factions, Action<WorkData> fullCallback)
            {
                Factions = factions;
                Run = InternalCallback;
                AllGrids = new List<IMyEntity>();
                Ready = false;
                FullCallback = fullCallback;
            }


            private void InternalCallback(IMyEntity entity)
            {
                lock (SpawnRequestLock)
                {
                    if (entity == null)
                    {
                        return;
                    }
                    Factions.AssignGridToHostileFaction(entity as IMyCubeGrid);
                    AllGrids.Add(entity);
                    ReportedGrids++;
                    if (ReportedGrids == TotalGrids && !Ready)
                    {
                        Ready = true;
                        AllGrids = AllGrids.OrderBy(x => {
                            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                            ((IMyCubeGrid)x).GetBlocks(blocks);
                            return blocks.Count;
                        }).ToList();
                        MyAPIGateway.Parallel.Start(FullCallback, EmptyMethod, this);
                    }
                }
            }

            private void EmptyMethod(WorkData data)
            {//empty on purpose
            }
        }

        public void Startup(SyncManager syncManager)
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(SyncManager.SyncIdToServer, syncManager.NetworkServerMessageRecieved);
            CommandHandler.Startup();
        }

        public void TestSpawn(string name, Action<WorkData> callBack)
        {
            var player = MyAPIGateway.Session.LocalHumanPlayer;
            Vector3 pos = player.GetPosition();
            var playerMatrix = player.Character.GetHeadMatrix(true);
            pos.Add(playerMatrix.Forward * 50);
            var positionMatrix = MatrixD.CreateWorld(pos, playerMatrix.Forward, playerMatrix.Up);
            List<IMyEntity> createdGrids;
            TestSpawnWrapper wrapper = new TestSpawnWrapper(callBack);
            Spawner.TrySpawnGrid(name, positionMatrix, 500, out createdGrids, wrapper.Run);
            wrapper.TotalGrids = createdGrids.Count;
        }

        public class TestSpawnWrapper : WorkData
        {
            public Action<IMyEntity> Run { get; }

            public List<IMyEntity> AllGrids { get; private set; }
            public int TotalGrids { get; set; }
            private int ReportedGrids = 0;
            public bool Ready { get; private set; }
            public Action<WorkData> FullCallback { get; }

            private readonly object SpawnRequestLock = new object();

            public TestSpawnWrapper(Action<WorkData> fullCallback)
            {
                Run = InternalCallback;
                AllGrids = new List<IMyEntity>();
                Ready = false;
                FullCallback = fullCallback;
            }


            private void InternalCallback(IMyEntity entity)
            {
                lock (SpawnRequestLock)
                {
                    if (entity == null)
                    {
                        return;
                    }
                    AllGrids.Add(entity);
                    ReportedGrids++;
                    if (ReportedGrids == TotalGrids && !Ready)
                    {
                        Ready = true;
                        AllGrids = AllGrids.OrderBy(x => {
                            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                            ((IMyCubeGrid)x).GetBlocks(blocks);
                            return blocks.Count;
                        }).ToList();
                        MyAPIGateway.Parallel.Start(FullCallback, EmptyMethod, this);
                    }
                }
            }

            private void EmptyMethod(WorkData data)
            {//empty on purpose
            }
        }

        /// <summary>
        /// Test spawn a structure
        /// </summary>
        /// <param name="name">Name of blueprint to spawn as structure</param>
        /// <returns>List of entities created, main entry on [0]</returns>
        public void TestBuild(string name, Action<WorkData> callBack)
        {
            var player = MyAPIGateway.Session.LocalHumanPlayer;
            var playerMatrix = player.Character.GetHeadMatrix(true);
            Vector3 pos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
            pos.Add(playerMatrix.Forward * 50);
            MatrixD positionMatrix = MatrixD.CreateWorld(pos, playerMatrix.Forward, playerMatrix.Up);
            List<IMyEntity> createdGrids;
            TestSpawnWrapper wrapper = new TestSpawnWrapper(callBack);
            Spawner.TrySpawnGrid(name, positionMatrix, 0, out createdGrids, wrapper.Run);
            wrapper.TotalGrids = createdGrids.Count;
        }

        public void Tick()
        {
            Commander.Tick();
            UnitCommander.Tick();
            BuildingCommander.Tick();
            Firewall.Tick();
        }

        internal void HandleServerMessage(ToServerMessage message)
        {
            CommandHandler.HandleIncommingCommand(message);
        }

        private void TryLoad()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cDataFileName, typeof(ServerData)))
            {
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cDataFileName, typeof(ServerData)))
                    {
                        Data = MyAPIGateway.Utilities.SerializeFromXML<ServerData>(reader.ReadToEnd());
                    }
                }
                catch (InvalidOperationException)
                {
                    Logger.Log("The ServerSession settings could not be loaded for some reason, they have been overwritten with the default!", Logger.Severity.Error, Logger.LogType.System);
                }
            }
            if (Data == null)
            {
                Data = new ServerData();
            }
            Data.Version = S9Constants.Version;
        }
    }
}