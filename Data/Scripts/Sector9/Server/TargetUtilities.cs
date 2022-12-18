using Sandbox.Game.Entities;
using Sector9.Server.Targets;

namespace Sector9.Server
{
    /// <summary>
    /// Class containing several utility functions for target selection
    /// </summary>
    internal static class TargetUtilities
    {
        public static ITarget SelectClosestTarget()
        {
            return null; //not implemented
        }

        /// <summary>
        /// Get the nearest player to a remote control
        /// </summary>
        /// <param name="remoteControl">Remote control to look out from</param>
        /// <returns>the target</returns>
        public static ITarget SelectClosestPlayer(MyRemoteControl remoteControl)
        {
            return new PlayerTarget(remoteControl.GetNearestPlayer());
        }

        public static ITarget SelectClosestShip()
        {
            return null; //not implemented
        }

        public static ITarget SelectClosestStation()
        {
            return null; //not implemented
        }
    }
}