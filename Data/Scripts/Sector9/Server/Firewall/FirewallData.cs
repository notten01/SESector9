namespace Sector9.Server.Firewall
{
    public class FirewallData
    {
        public long GridId { get; set; }
        public int FirewallCountdown { get; set; }
        public bool GameOver { get; set; }

        private const int MissingFirewallTimeout = 432000;

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
    }
}