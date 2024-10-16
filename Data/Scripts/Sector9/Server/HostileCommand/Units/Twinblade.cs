using Sector9.Data.Scripts.Sector9.Server.Units.Control;
using Sector9.Server;
using Sector9.Server.Targets;
using Sector9.Server.Units.Control;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Twinblade : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => true;
        public int GameStage => 3;

        public ICaptain GetBehaviour(ITarget target, Planets planets)
        {
            return new SnipeTargetCaptain(target, BaseCaptain.TargetPreference.Station, planets);
        }

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
