using Sandbox.ModAPI;
using Sector9.Core;

namespace Sector9.Server
{
    /// <summary>
    /// Session for the server, keeping track of game stats. Can exist togheter with <see cref="PlayerSession"/> if its a locally hosted game
    /// </summary>
    internal class ServerSession
    {
        private readonly MESApi MesApi;

        public ServerSession()
        {
            MesApi = new MESApi();
            TryLoad();
        }

        private void TryLoad()
        {
            //todo: try loading from disk
            MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "registered server session");
        }
    }
}