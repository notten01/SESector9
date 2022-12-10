using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace Sector9.Server.Units
{
    internal class Thruster
    {
        public MyBlockOrientation Orientation { get; set; }
        private readonly IMyThrust Block;

        public Thruster(IMyCubeBlock thruster)
        {
            Block = thruster as IMyThrust;
            Orientation = Block.Orientation;
        }

        public bool IsWroking()
        {
            return (Block != null && Block.IsWorking);
        }
    }
}