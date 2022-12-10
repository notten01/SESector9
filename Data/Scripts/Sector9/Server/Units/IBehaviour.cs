using Sandbox.Game.Entities;
using Sector9.Core;

namespace Sector9.Server.Units
{
    public interface IBehaviour : ITickable
    {
        void AttachRemoteControl(MyRemoteControl control);

        void SetUnit(Unit unit);

        bool IsReady { get; }
    }
}