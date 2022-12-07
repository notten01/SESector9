using Sandbox.ModAPI;
using Sector9.Core;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Sector9.Server
{
    /// <summary>
    /// Session for the server, keeping track of game stats. Can exist togheter with <see cref="PlayerSession"/> if its a locally hosted game
    /// </summary>
    internal class ServerSession
    {
        private const string cDataFileName = "S9ServerSession.xml";
        private ServerData Data;
        private readonly FactionManager Factions;
        private readonly GridSpawner Spawner;

        public ServerSession()
        {
            TryLoad();
            Factions = new FactionManager();
            Spawner = new GridSpawner();
        }

        public bool EnableLog => Data.EnableLog;

        public void Save()
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(cDataFileName, typeof(ServerData)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML<ServerData>(Data));
            }
        }

        /// <summary>
        /// Dispose all events and or session based stuff
        /// </summary>
        public void Shutdown()
        {
            Factions.Shutdown();
        }

        /// <summary>
        /// Spawn a ship belonging to the hostile faction
        /// </summary>
        /// <returns>True or false is the spawning was successfull</returns>
        public bool SpawnHostileShip()
        {
            Vector3 pos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
            pos.Add(Vector3D.Up * 20);
            var positionMatrix = MatrixD.CreateWorld(pos, Vector3D.Forward, Vector3D.Up);
            List<IMyEntity> createdGrids;
            Spawner.TrySpawnGrid("QnVsbGRvZyBicmF3bGVy", positionMatrix, out createdGrids);
            if (createdGrids != null)
            {
                foreach (var grid in createdGrids)
                {
                    Factions.AssignGridToHostileFaction(grid as IMyCubeGrid);
                }
                return true;
            }
            return false;
        }

        private void TryLoad()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cDataFileName, typeof(ServerData)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cDataFileName, typeof(ServerData)))
                {
                    Data = MyAPIGateway.Utilities.SerializeFromXML<ServerData>(reader.ReadToEnd());
                }
            }
            else
            {
                Data = new ServerData();
            }
            Data.Version = S9Constants.Version;
        }
    }
}