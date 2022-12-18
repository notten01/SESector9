namespace Sector9.Server.Units.Behaviours
{
    internal abstract class BaseBehaviour : IBehaviour
    {
        protected Unit Unit;
        protected Pilot Pilot;

        public bool IsReady
        {
            get { return Pilot != null; }
        }

        public abstract bool IsComplete { get; }
        public abstract string Name { get; }

        public void AttachPilot(Pilot pilot)
        {
            Pilot = pilot;
        }

        public abstract void Interrupt();

        public void SetUnit(Unit unit)
        {
            Unit = unit;
        }

        public abstract void Tick();
    }
}