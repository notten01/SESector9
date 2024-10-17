using Sector9.Data.Scripts.Sector9.Server.Units.Control;
using Sector9.Server;
using Sector9.Server.Targets;
using Sector9.Server.Units.Control;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Stump : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => true;
        public int GameStage => 1;

        public ICaptain GetBehaviour(ITarget target, Planets planets)
        {
            return new StraveTargetCaptain(target, BaseCaptain.TargetPreference.Grid, planets);
        }

        public string GetGridName(UnitType type)
        {
            switch (type)
            {
                case UnitType.Space:
                    return "c3R1bXAgc3BhY2U=";
                case UnitType.Atmosphere:
                    return "c3R1bXAgYXRtbw===";
                default:
                    return "";
            }
        }
    }
}
