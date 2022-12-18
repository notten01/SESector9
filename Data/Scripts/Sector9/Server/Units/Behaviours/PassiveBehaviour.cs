namespace Sector9.Server.Units.Behaviours
{
    /// <summary>
    /// test behaviour that does absolutely nothing
    /// </summary>
    internal class PassiveBehaviour : IBehaviour
    {
        public bool IsReady
        {
            get
            {
                return true;
            }
        }

        public bool IsComplete
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return "passive";
            }
        }

        public void AttachPilot(Pilot pilot)
        {
            //empty
        }

        public void Interrupt()
        {
            //empty
        }

        public void SetUnit(Unit unit)
        {
            //empty
        }

        public void Tick()
        {
            //empty
        }
    }
}