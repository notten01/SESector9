using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;
using System.Collections.Generic;
using VRageMath;

namespace Sector9.Server
{
    /// <summary>
    /// Session for the server, keeping track of game stats. Can exist togheter with <see cref="PlayerSession"/> if its a locally hosted game
    /// </summary>
    internal class ServerSession
    {
        private const string cDataFileName = "S9ServerSession.xml";
        private readonly MESApi MesApi;
        private ServerData Data;

        public ServerSession()
        {
            MesApi = new MESApi();
            TryLoad();
        }

        public bool EnableLog => Data.EnableLog;

        public void Save()
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(cDataFileName, typeof(ServerData)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML<ServerData>(Data));
            }
        }

        public void Shutdown()
        {
            MesApi.UnregisterListener();
        }

        public bool SpawnShip()
        {
            List<string> spawnGroups = new List<string>();
            spawnGroups.Add("s9-System-bulldog");
            spawnGroups.Add("SpawnGroups-SystemLigtAtmo");
            Vector3 pos = MyAPIGateway.Session.LocalHumanPlayer.GetPosition();
            pos.Add(Vector3D.Up * 20);
            var positionMatrix = MatrixD.CreateWorld(pos, Vector3D.Forward, Vector3D.Up);

            if (MesApi.CustomSpawnRequest(spawnGroups, positionMatrix, Vector3D.Zero, true, "TheProgram", S9Constants.SystemName))
            {
                Logger.Log($"Spawned ship on {pos.X} {pos.Y} {pos.Z}", Logger.Severity.Info, Logger.LogType.Server);
                return true;
            }
            else
            {
                Logger.Log($"Failed to spawn ship on {pos.X} {pos.Y} {pos.Z}!", Logger.Severity.Error, Logger.LogType.Server);
                return false;
            }
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