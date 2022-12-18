using Sector9.Core;
using Sector9.Server.Units.Behaviours;

namespace Sector9.Server.Units
{
    /// <summary>
    /// Current behaviour/order that is being executed
    /// </summary>
    public interface IBehaviour : ITickable
    {
        /// <summary>
        /// Attach the pilot to the behaviour
        /// </summary>
        /// <param name="pilot">The pilot class used to move the ship</param>
        void AttachPilot(Pilot pilot);

        /// <summary>
        /// Attach the unit instance to the behaviour
        /// </summary>
        /// <param name="unit">reference to the unit that is getting controlled</param>
        void SetUnit(Unit unit);

        /// <summary>
        /// Behaviour is ready to execute
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Behaviour has completed its order
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Interrupt and stop the behaviour so it can be safely replaced
        /// </summary>
        void Interrupt();

        /// <summary>
        /// Name of the behaviour, mostly used for debugging purposse
        /// </summary>
        string Name { get; }
    }
}