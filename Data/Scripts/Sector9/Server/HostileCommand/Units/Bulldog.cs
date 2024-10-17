using Sector9.Core.Logging;
using Sector9.Server;
using Sector9.Server.Targets;
using Sector9.Server.Units.Control;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Bulldog : IUnit
    {
        public bool SpaceAllowed => false;
        public bool AtmoAllowed => true;
        public int GameStage => 2;

        public ICaptain GetBehaviour(ITarget target, Planets planets)
        {
            return new EncirkleTargetCaptain(target, BaseCaptain.TargetPreference.Player, planets);
        }

        public string GetGridName(UnitType type)
        {
            switch (type)
            {
                case UnitType.Atmosphere:
                    return "QnVsbGRvZyBicmF3bGVy";
                default:
                    Logger.Log("Bulldog does not have a space variant", Logger.Severity.Warning, Logger.LogType.System);
                    return "";
            }
        }
    }
}
