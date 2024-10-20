namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    public interface IUnit
    {
        /// <summary>
        /// Get the filename for the grid to spawn
        /// </summary>
        /// <param name="type">Type where it will spawn</param>
        /// <returns>Gridname that should be spawned</returns>
        string GetGridName(UnitType type);

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
