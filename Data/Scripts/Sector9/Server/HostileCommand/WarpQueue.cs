using Sandbox.ModAPI;
using Sector9.Core;
using System.Collections.Generic;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand
{
    /// <summary>
    /// Units don't spawn emediatly, they get added 'into the warp' so that the player can be warned before things go wrong
    /// </summary>
    public class WarpQueue : ITickable
    {
        private readonly List<IWarpQueueItem> Items = new List<IWarpQueueItem>();
        private readonly List<IWarpQueueItem> ToDelete = new List<IWarpQueueItem>();

        public void Tick()
        {
            MyAPIGateway.Parallel.ForEach(Items, x =>
            {
                if (x.ShouldExecuteTick())
                {
                    x.Execute();
                    ToDelete.Add(x);
                }
            });
            if (ToDelete.Count > 0)
            {
                foreach (IWarpQueueItem item in ToDelete)
                {
                    Items.Remove(item);
                }
                ToDelete.Clear();
            }
        }

        public void AddItem(IWarpQueueItem item)
        {
            Items.Add(item);
        }
    }

    public interface IWarpQueueItem
    {
        void Execute();
        bool ShouldExecuteTick();
    }
}
