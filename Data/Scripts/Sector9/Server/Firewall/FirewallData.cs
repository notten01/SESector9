namespace Sector9.Server.Firewall
{
    public class FirewallData
    {
        public long GridId { get; set; }
        public int FirewallCountdown { get; set; }

        public FirewallData()
        {
            GridId = -1;
            FirewallCountdown = 0;
        }
    }
}