
namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Viper : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => false;
        public int GameStage => 1;

        public string GetGridName(UnitType type)
        {
            switch (type)
            {
                case UnitType.Atmosphere:
                    return "ZmxhdGZpc2ggYXRtbw==";
                default:
                    return "";
            }
        }
    }
}
