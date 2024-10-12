using Sandbox.ModAPI;
using Sector9.Api;
using Sector9.Core;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
using Sector9.Server.Buildings;
using Sector9.Server.FireWall;
using Sector9.Server.Units;
using Server.Data;
using System;
using System.Collections.Generic;
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
            Firewall.Save();
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
        }

        /// <summary>
        /// Spawn a ship belonging to the hostile faction
        /// </summary>
        /// <param name="shipName">Name of the prefab to spawn</param>
        /// <returns>True or false is the spawning was successfull</returns>
        public List<IMyEntity> SpawnHostileShip(string shipName)
        {
            Vector3 pos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
            pos.Add(new Vector3(10, 10, 10));
            var positionMatrix = MatrixD.CreateWorld(pos, Vector3D.Forward, Vector3D.Up);
            List<IMyEntity> createdGrids;
            Spawner.TrySpawnGrid(shipName, positionMatrix, 500, out createdGrids);
            if (createdGrids != null)
            {
                foreach (var grid in createdGrids)
                {
                    Factions.AssignGridToHostileFaction(grid as IMyCubeGrid);
                }
                return createdGrids;
            }
            return null;
        }

        public void Startup(SyncManager syncManager)
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(SyncManager.SyncIdToServer, syncManager.NetworkServerMessageRecieved);
            CommandHandler.Startup();
        }

        public List<IMyEntity> TestSpawn(string name)
        {
            var player = MyAPIGateway.Session.LocalHumanPlayer;
            Vector3 pos = player.GetPosition();
            var playerMatrix = player.Character.GetHeadMatrix(true);
            pos.Add(playerMatrix.Forward * 50);
            var positionMatrix = MatrixD.CreateWorld(pos, playerMatrix.Forward, playerMatrix.Up);
            List<IMyEntity> createdGrids;
            Spawner.TrySpawnGrid(name, positionMatrix, 500, out createdGrids);
            return createdGrids;
        }

        /// <summary>
        /// Test spawn a structure
        /// </summary>
        /// <param name="name">Name of blueprint to spawn as structure</param>
        /// <returns>List of entities created, main entry on [0]</returns>
        public List<IMyEntity> TestBuild(string name)
        {
            var player = MyAPIGateway.Session.LocalHumanPlayer;
            var playerMatrix = player.Character.GetHeadMatrix(true);
            Vector3 pos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
            pos.Add(playerMatrix.Forward * 50);
            MatrixD positionMatrix = MatrixD.CreateWorld(pos, playerMatrix.Forward, playerMatrix.Up);
            List<IMyEntity> createdGrids;
            Spawner.TrySpawnGrid(name, positionMatrix, 0, out createdGrids);
            return createdGrids;
        }

        public void Tick()
        {
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