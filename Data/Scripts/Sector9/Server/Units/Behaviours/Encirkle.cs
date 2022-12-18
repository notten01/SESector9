using VRageMath;

namespace Sector9.Server.Units.Behaviours
{
    internal class Encirkle : BaseBehaviour
    {
        private bool Init = false;
        private readonly Vector3D[] Waypoints;

        public override bool IsComplete
        {
            get { return false; } //never completes
        }

        public override string Name
        {
            get
            {
                return nameof(Encirkle);
            }
        }

        public Encirkle(Vector3D[] waypoints)
        {
            Waypoints = waypoints;
        }

        public override void Interrupt()
        {
            Pilot.Stop();
        }

        public override void Tick()
        {
            if (!Init)
            {
                Pilot.SetAutoPilotBasicEncirkle(Waypoints);
                Init = true;
            }
        }
    }
}