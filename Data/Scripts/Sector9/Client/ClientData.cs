namespace Sector9.Client
{
    public class ClientData
    {
        public ClientData()
        {
            EnableLog = true;
        }

        public string Version { get; set; }
        public bool EnableLog { get; set; }
    }
}