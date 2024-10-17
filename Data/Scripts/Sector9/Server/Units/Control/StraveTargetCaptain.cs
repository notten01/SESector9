using Sector9.Server;
using Sector9.Server.Targets;
using Sector9.Server.Units.Control;

namespace Sector9.Data.Scripts.Sector9.Server.Units.Control
{
    internal class StraveTargetCaptain : BaseCaptain
    {
        private readonly ITarget Target;

        public StraveTargetCaptain(ITarget target, TargetPreference preferedTarget, Planets planets) : base(preferedTarget, planets)
        {
            Target = target;
        }

        public override void UnitTick()
        {
            //todo
        }
    }
}
