using System.Collections.Generic;

namespace Sector9.Server
{
    public class PlayerFactionData
    {
        public List<string> FactionNames { get; set; }
        public PlayerFactionData ()
        {
            FactionNames = new List<string>();
        }
    }
}