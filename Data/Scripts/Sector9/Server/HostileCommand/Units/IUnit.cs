using Sector9.Server;
using Sector9.Server.Targets;
using Sector9.Server.Units.Control;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal interface IUnit
    {
        /// <summary>
        /// Get the filename for the grid to spawn
        /// </summary>
        /// <param name="type">Type where it will spawn</param>
        /// <returns>Gridname that should be spawned</returns>
        string GetGridName(UnitType type);

        /// <summary>
        /// Get the captain for the unit
        /// </summary>
        /// <param name="target">target to attack</param>
        /// <param name="planets">reference to the planets</param>
        /// <returns>A captain to control the unit</returns>
        ICaptain GetBehaviour(ITarget target, Planets planets);

        /// <summary>
        /// Can fly in space
        /// </summary>
        bool SpaceAllowed { get; }

        /// <summary>
        /// can fly on planets
        /// </summary>
        bool AtmoAllowed { get; }

        /// <summary>
        /// minimal gamestage to spawn
        /// </summary>
        int GameStage { get; }
    }


    public enum UnitType
    {
        Space,
        Atmosphere
    }
}
