using Sandbox.Game.Entities;

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

        public void AttachRemoteControl(MyRemoteControl control)
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