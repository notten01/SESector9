
namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Twinblade : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => true;
        public int GameStage => 3;

        public string GetGridName(UnitType type)
        {
            switch (type)
            {
                case UnitType.Atmosphere:
                    return "VHdpbmJsYWRlIGF0bW8=";
                case UnitType.Space:
                    return "VHdpbmJsYWRlIHNwYWNl";
                default:
                    return "";
            }
        }
    }
}
