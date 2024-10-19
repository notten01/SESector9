using Sector9.Core.Logging;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Bulldog : IUnit
    {
        public bool SpaceAllowed => false;
        public bool AtmoAllowed => true;
        public int GameStage => 2;

        public string GetGridName(UnitType type)
        {
            switch (type)
            {
                case UnitType.Atmosphere:
                    return "QnVsbGRvZyBicmF3bGVy";
                case UnitType.Space:
                    return "QnVsbGRvZyBzcGFjZQ==";
                default:
                    Logger.Log("Bulldog does not have a space variant", Logger.Severity.Warning, Logger.LogType.System);
                    return "";
            }
        }
    }
}
