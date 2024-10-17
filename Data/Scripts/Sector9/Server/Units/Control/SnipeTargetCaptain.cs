using Sector9.Server;
using Sector9.Server.Targets;
using Sector9.Server.Units.Control;

namespace Sector9.Data.Scripts.Sector9.Server.Units.Control
{
    internal class SnipeTargetCaptain : BaseCaptain
    {
        private readonly ITarget Target;

        public SnipeTargetCaptain(ITarget target, TargetPreference preferedTarget, Planets planets) : base(preferedTarget, planets)
        {
            target = Target;
        }

        public override void UnitTick()
        {
            //todo
        }
    }
}
