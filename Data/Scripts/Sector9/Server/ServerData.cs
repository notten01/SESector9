namespace Sector9.Server
{
    public class ServerData
    {
        public string Version { get; set; }
        public bool EnableLog { get; set; }

        public ServerData()
        {
            EnableLog = true;
        }
    }
}