using Sector9.Core;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.actions
{
    internal interface IQueuedAction : ITickable
    {
        void Execute();
    }
}
