using Sandbox.Game.Entities;
using Sector9.Core;

namespace Sector9.Server.Units.Control
{
    /// <summary>
    /// A captain is the thing that controls the behaviours of a ship. It can queue togheter special behaviours in order to make the ship fufill a certain role
    /// </summary>
    public interface ICaptain : ITickable
    {
        void SetCaptainData(MyRemoteControl remoteControl, Unit unit);
    }
}