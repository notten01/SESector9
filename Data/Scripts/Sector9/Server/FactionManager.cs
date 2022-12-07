using Sandbox.ModAPI;
using Sector9.Core.Logging;
using System.Linq;
using VRage.Game.ModAPI;

namespace Sector9.Server
{
    /// <summary>
    /// System that handles factions within the game session
    /// </summary>
    internal class FactionManager
    {
        private const string cSystemName = "VGhlU3lzdGVt";
        private const string cHumanityName = "Sector 1";
        private readonly IMyFaction hostileFaciton;
        private readonly IMyFaction humanFactiopn;

        /// <summary>
        /// ctor, ensures that factions are created to the liking of this mods. Resets all stats incorrect
        /// </summary>
        public FactionManager()
        {
            hostileFaciton = MyAPIGateway.Session.Factions.TryGetFactionByName(cSystemName);
            humanFactiopn = MyAPIGateway.Session.Factions.TryGetFactionByName(cHumanityName);
            bool createdFaction = false;
            if (hostileFaciton == null)
            {
                MyAPIGateway.Session.Factions.CreateNPCFaction("SYS", cSystemName, "U2VjdG9yIDEgd2lsbCBmYWxs", "you should not be able to read this?");
                hostileFaciton = MyAPIGateway.Session.Factions.TryGetFactionByName(cSystemName);
                MyAPIGateway.Session.Factions.AddNewNPCToFaction(hostileFaciton.FactionId, "cm9vdA==");
                createdFaction = true;
                Logger.Log("Created hostile faction", Logger.Severity.Info, Logger.LogType.Server);
            }
            if (humanFactiopn == null)
            {
                MyAPIGateway.Session.Factions.CreateNPCFaction("S1", cHumanityName, "Terra", "Again, you should not be able to read this?");
                humanFactiopn = MyAPIGateway.Session.Factions.TryGetFactionByName(cHumanityName);
                MyAPIGateway.Session.Factions.AddNewNPCToFaction(humanFactiopn.FactionId, "Operator");
                createdFaction = true;
                Logger.Log("Created human faction", Logger.Severity.Info, Logger.LogType.Server);
            }

            if (hostileFaciton == null || humanFactiopn == null)
            {
                Logger.Log("Could not properly create factions on game setup!", Logger.Severity.Fatal, Logger.LogType.Server);
                MyAPIGateway.Utilities.ShowMissionScreen("Fatal error", "Sector 9 crash", "on:", "The system was unable to produce the factions required for the game to run properly.\r\n" +
                    "Please save and close your game as soon as possible!\r\n" +
                    "Your game is now at risk of crashing to desktop pretty soon!\r\nIf this problem presist then enable logging and check the logfiles for the exact cause. Also file a report on either Github or the Steam workshop");
                return;
            }

            if (createdFaction)
            {
                Logger.Log("Setting up deplomicy of new factions", Logger.Severity.Info, Logger.LogType.Server);
                foreach (var faction in MyAPIGateway.Session.Factions.Factions.Values.Select(x => x.FactionId))
                {
                    MyAPIGateway.Session.Factions.DeclareWar(hostileFaciton.FactionId, faction);
                    MyAPIGateway.Session.Factions.SendPeaceRequest(humanFactiopn.FactionId, faction);
                }
            }

            MyAPIGateway.Session.Factions.SetReputation(hostileFaciton.FactionId, humanFactiopn.FactionId, -1000);
            MyAPIGateway.Session.Factions.DeclareWar(hostileFaciton.FactionId, humanFactiopn.FactionId);
            MyAPIGateway.Session.Factions.DeclareWar(humanFactiopn.FactionId, hostileFaciton.FactionId);

            MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
        }

        public void Shutdown()
        {
            MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
        }

        private void FactionCreated(long factionId)
        {
            MyAPIGateway.Session.Factions.DeclareWar(hostileFaciton.FactionId, factionId);
            MyAPIGateway.Session.Factions.SetReputation(hostileFaciton.FactionId, factionId, -1000);
            MyAPIGateway.Session.Factions.SendPeaceRequest(humanFactiopn.FactionId, factionId);
        }

        /// <summary>
        /// Assign a grid to the friendly (human) faction
        /// </summary>
        /// <param name="grid">Grid to set ownership</param>
        public void AssignGridToHumanFaction(IMyCubeGrid grid)
        {
            AssignGridToFaction(grid, humanFactiopn.FounderId);
        }

        /// <summary>
        /// Assign a grid to the hostile (system) faction
        /// </summary>
        /// <param name="grid">Grid to change ownership</param>
        public void AssignGridToHostileFaction(IMyCubeGrid grid)
        {
            AssignGridToFaction(grid, hostileFaciton.FounderId);
        }

        private static void AssignGridToFaction(IMyCubeGrid grid, long ownerId)
        {
            grid.ChangeGridOwnership(ownerId, VRage.Game.MyOwnershipShareModeEnum.None);
        }
    }
}