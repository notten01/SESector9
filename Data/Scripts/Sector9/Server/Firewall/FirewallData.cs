using System;

namespace Sector9.Server.Firewall
{
    public class FirewallData
    {
        public long GridId { get; set; }
        public int FirewallCountdown { get; set; }
        public bool GameOver { get; set; }

        private const int MissingFirewallTimeout = 7200; //countdown is poked every 60 UPS (assuming every 1 second)

        public FirewallData()
        {
            GridId = -1;
            FirewallCountdown = MissingFirewallTimeout;
            GameOver = false;
        }

        public void ResetCountdownn()
        {
            FirewallCountdown = MissingFirewallTimeout;
        }

        public void IncreaseCountdown()
        {
            FirewallCountdown = Math.Min(FirewallCountdown++, MissingFirewallTimeout);
        }
    }
}