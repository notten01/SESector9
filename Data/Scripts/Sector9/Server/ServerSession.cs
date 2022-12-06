using Sandbox.ModAPI;
using Sector9.Core;

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