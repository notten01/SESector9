using Sector9.Server;
using Sector9.Server.Targets;
using Sector9.Server.Units.Control;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Goose : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => true;
        public int GameStage => 1;

        public ICaptain GetBehaviour(ITarget target, Planets planets)
        {
            return new EncirkleTargetCaptain(target, BaseCaptain.TargetPreference.Player, planets);
        }

        public string GetGridName(UnitType type)
        {
            switch (type) {
            case UnitType.Atmosphere:
                return "Z29vc2UgYXRtbw==";
            case UnitType.Space:
                return "Z29vc2Ugc3BhY2U=";
            default:
                return "";
            }
        }
    }
}
