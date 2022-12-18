using VRage.Game.ModAPI;
using VRageMath;

namespace Sector9.Server.Targets
{
    /// <summary>
    /// Class that represents a player character as target
    /// </summary>
    public class PlayerTarget : ITarget
    {
        private readonly IMyPlayer Player;

        public PlayerTarget(IMyPlayer player)
        {
            Player = player;
        }

        public Vector3D GetPosition()
        {
            return Player.GetPosition();
        }

        public bool IsValid()
        {
            bool? dead = Player?.Character?.IsDead;
            return dead == false;
        }
    }
}