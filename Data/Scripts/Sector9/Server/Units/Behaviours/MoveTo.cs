using Sector9.Core.Logging;
using VRageMath;

namespace Sector9.Server.Units.Behaviours
{
    internal class MoveTo : BaseBehaviour
    {
        private bool OnLocation;
        private readonly Vector3D Target;
        private bool Init = false;

        private const int Tolerance = 25;

        public override bool IsComplete
        {
            get
            {
                return OnLocation;
            }
        }

        public override string Name
        {
            get
            {
                return nameof(MoveTo);
            }
        }

        public MoveTo(Vector3D target)
        {
            Target = target;
        }

        public override void Tick()
        {
            if (OnLocation)
            {
                return;
            }

            if (!Init)
            {
                Pilot.SetAutoPilotBasicTarget(Target);
                Init = true;
            }
            Pilot.Tick();

            var distance = Vector3D.Distance(Target, Pilot.CurrentPosition);
            bool onPosition = distance < Tolerance;

            if (onPosition)
            {
                OnLocation = true;
                Logger.Log("Onlocation is now true", Logger.Severity.Info, Logger.LogType.Server);
                Pilot.Stop();
            }
        }

        public override void Interrupt()
        {
            Pilot.Stop();
        }
    }
}