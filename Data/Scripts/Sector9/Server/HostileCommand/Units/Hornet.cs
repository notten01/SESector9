
namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Hornet : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => true;
        public int GameStage => 1;

        public string GetGridName(UnitType type)
        {
            switch (type)
            {
                case UnitType.Atmosphere:
                    return "SG9ybmV0IGF0bW8=";
                case UnitType.Space:
                    return "SG9ybmV0IHNwYWNl";
                default:
                    return "";
            }
        }
    }
}
