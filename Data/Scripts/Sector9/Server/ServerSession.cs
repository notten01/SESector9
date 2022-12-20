﻿using Sandbox.ModAPI;
using Sector9.Api;
using Sector9.Core;
using Sector9.Core.Logging;
using Sector9.Multiplayer;
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
        private readonly FirewallHandler Firewall;
        private readonly GridSpawner Spawner;
        private readonly DamageHandler DamageHandler;

        private ServerData Data;

        public ServerSession()
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
        }

        public DefinitionLibrary BlockLibrary { get; private set; }
        public bool EnableLog => Data.EnableLog;
        public Planets Planets { get; }
        public UnitCommander UnitCommander { get; }
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
            MyAPIGateway.Utilities.MessageEntered -= syncManager.ChatMessageRecieved;
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(SyncManager.SyncIdToServer, syncManager.NetworkServerMessageRecieved);
            Factions.Shutdown();
            WeaponsCore.Unload();
            Firewall.Shutdown();
        }

        /// <summary>
        /// Spawn a ship belonging to the hostile faction
        /// </summary>
        /// <returns>True or false is the spawning was successfull</returns>
        public List<IMyEntity> SpawnHostileShip()
        {
            Vector3 pos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
            pos.Add(new Vector3(10, 10, 10));
            var positionMatrix = MatrixD.CreateWorld(pos, Vector3D.Forward, Vector3D.Up);
            List<IMyEntity> createdGrids;
            Spawner.TrySpawnGrid("QnVsbGRvZyBicmF3bGVy", positionMatrix, out createdGrids);
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
            MyAPIGateway.Utilities.MessageEntered += syncManager.ChatMessageRecieved;
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(SyncManager.SyncIdToServer, syncManager.NetworkServerMessageRecieved);
        }

        public List<IMyEntity> TestSpawn(string name)
        {
            Vector3 pos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
            pos.Add(Vector3D.Up * 20);
            var positionMatrix = MatrixD.CreateWorld(pos, Vector3D.Forward, Vector3D.Up);
            List<IMyEntity> createdGrids;
            Spawner.TrySpawnGrid(name, positionMatrix, out createdGrids);
            return createdGrids;
        }

        public void Tick()
        {
            UnitCommander.Tick();
            Firewall.Tick();
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