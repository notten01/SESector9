namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Flatfish : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => true;
        public int GameStage => 3;


        public string GetGridName(UnitType type)
        {
            switch(type)
            {
                case UnitType.Atmosphere:
                    return "ZmxhdGZpc2ggYXRtbw==";
                case UnitType.Space:
                    return "ZmxhdGZpc2ggc3BhY2U=";
                default:
                    return "";
            }
        }
    }
}
