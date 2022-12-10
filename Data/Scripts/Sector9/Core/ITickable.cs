namespace Sector9.Core
{
    public interface ITickable
    {
        /// <summary>
        /// Tick with every itteration of the simulation
        /// </summary>
        void Tick();
    }
}