using Sector9.Core;
using System.Collections.Generic;

namespace Sector9.Client.Hud
{
    internal interface IHudEvent : ITickable
    {
        /// <summary>
        /// Is action completed
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Write the current state of the event to the ui string
        /// </summary>
        /// <param name="uiString">string that is shown in the ui</param>
        void DrawToString(List<string> uiString);
    }
}