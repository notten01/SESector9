using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Server;

namespace Sector9.Client
{
    /// <summary>
    /// session handing player related items. Can existin togheter with <see cref="ServerSession"/> if its locally hosted
    /// </summary>
    internal class PlayerSession
    {
        private const string cDataFileName = "S9PlayerSession.xml";
        private PlayerData Data;

        public PlayerSession()
        {
            TryLoad();
        }

        public bool EnableLog => Data.EnableLog;

        public void Save()
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(cDataFileName, typeof(PlayerData)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML<PlayerData>(Data));
            }
        }

        private void TryLoad()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cDataFileName, typeof(PlayerData)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cDataFileName, typeof(PlayerData)))
                {
                    Data = MyAPIGateway.Utilities.SerializeFromXML<PlayerData>(reader.ReadToEnd());
                }
            }
            else
            {
                Data = new PlayerData();
            }
            Data.Version = S9Constants.Version;
        }
    }
}