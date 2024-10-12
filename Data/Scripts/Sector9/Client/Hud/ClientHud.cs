using Sandbox.ModAPI;
using Sector9.Client;
using Sector9.Client.Hud;
using Sector9.Core;
using System.Collections;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace Scripts.Sector9.Client.Hud
{
    public class ClientHud : ITickable
    {
        private readonly ClientSession Session;
        private readonly Queue Events;
        private IHudEvent CurrentEvent;
        private IMyHudObjectiveLine UiLine;

        private int FrameCounter = 3;

        public ClientHud(ClientSession session)
        {
            Session = session;
            Events = new Queue();
            UiLine = MyAPIGateway.Utilities.GetObjectiveLine();
            UiLine.Show();
        }

        public void Tick()
        {
            if (FrameCounter < 60)
            {
                FrameCounter++;
                return;
            }
            FrameCounter = 0;

            List<string> UiString = new List<string>();
            if (CurrentEvent != null)
            {
                CurrentEvent.Tick();
            }
            else if (Events.Count > 0)
            {
                CurrentEvent = (IHudEvent)Events.Dequeue();
            }

            UpdateFirewallState();
        }

        private void UpdateFirewallState()
        {
        }
    }
}