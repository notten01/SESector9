using Sector9.Server;
using Sector9.Server.Targets;
using Sector9.Server.Units.Control;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class Viper : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => false;
        public int GameStage => 1;

        public ICaptain GetBehaviour(ITarget target, Planets planets)
        {
            return new EncirkleTargetCaptain(target, BaseCaptain.TargetPreference.Player, planets);
        }

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
