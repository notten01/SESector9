using VRage.Game.ModAPI;
using VRageMath;

namespace Sector9.Server.Targets
{
    /// <summary>
    /// Class that represents a grid (ship or station) as a target
    /// </summary>
    public class GridTarget : ITarget
    {
        private readonly IMyCubeGrid Grid;

        public GridTarget(IMyCubeGrid PrimaryGrid)
        {
            Grid = PrimaryGrid;
        }

        public Vector3D GetPosition()
        {
            return Grid.GetPosition();
        }

        public bool IsValid()
        {
            return !Grid.Closed;
        }
    }
}