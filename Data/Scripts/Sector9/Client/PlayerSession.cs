using Sandbox.ModAPI;
using Sector9.Core;

namespace Sector9.Client
{
    /// <summary>
    /// session handing player related items. Can existin togheter with <see cref="ServerSession"/> if its locally hosted
    /// </summary>
    internal class PlayerSession
    {
        public PlayerSession()
        {
            TryLoad();
        }

        private void TryLoad()
        {
            MyAPIGateway.Utilities.ShowMessage(S9Constants.SystemName, "registered player session");
        }
    }
}