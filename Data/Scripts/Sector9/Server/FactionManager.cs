using Sandbox.ModAPI;
using Sector9.Core.Logging;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

namespace Sector9.Server
{
    /// <summary>
    /// System that handles factions within the game session
    /// </summary>
    public class FactionManager
    {
        private const string cDataFileName = "S9factionsTracking.xml";

        private const string cSystemName = "VGhlU3lzdGVt";
        private const string cHumanityName = "Sector 1";
        public IMyFaction HostileFaciton { get; }
        public IMyFaction HumanFaction { get; }
        public List<IMyFaction> PlayerFactions { get; }

        /// <summary>
        /// ctor, ensures that factions are created to the liking of this mods. Resets all stats incorrect
        /// </summary>
        public FactionManager()
        {
            PlayerFactions = LoadPlayerFactions();
            HostileFaciton = MyAPIGateway.Session.Factions.TryGetFactionByName(cSystemName);
            HumanFaction = MyAPIGateway.Session.Factions.TryGetFactionByName(cHumanityName);
            bool createdFaction = false;
            if (HostileFaciton == null)
            {
                MyAPIGateway.Session.Factions.CreateNPCFaction("SYS", cSystemName, "U2VjdG9yIDEgd2lsbCBmYWxs", "you should not be able to read this?");
                HostileFaciton = MyAPIGateway.Session.Factions.TryGetFactionByName(cSystemName);
                MyAPIGateway.Session.Factions.AddNewNPCToFaction(HostileFaciton.FactionId, "cm9vdA==");
                createdFaction = true;
                Logger.Log("Created hostile faction", Logger.Severity.Info, Logger.LogType.Server);
            }
            if (HumanFaction == null)
            {
                MyAPIGateway.Session.Factions.CreateNPCFaction("S1", cHumanityName, "Terra", "Again, you should not be able to read this?");
                HumanFaction = MyAPIGateway.Session.Factions.TryGetFactionByName(cHumanityName);
                MyAPIGateway.Session.Factions.AddNewNPCToFaction(HumanFaction.FactionId, "Operator");
                createdFaction = true;
                Logger.Log("Created human faction", Logger.Severity.Info, Logger.LogType.Server);
            }

            if (HostileFaciton == null || HumanFaction == null)
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
                    MyAPIGateway.Session.Factions.DeclareWar(HostileFaciton.FactionId, faction);
                    MyAPIGateway.Session.Factions.SendPeaceRequest(HumanFaction.FactionId, faction);
                }
            }

            MyAPIGateway.Session.Factions.SetReputation(HostileFaciton.FactionId, HumanFaction.FactionId, -1000);
            MyAPIGateway.Session.Factions.DeclareWar(HostileFaciton.FactionId, HumanFaction.FactionId);
            MyAPIGateway.Session.Factions.DeclareWar(HumanFaction.FactionId, HostileFaciton.FactionId);

            foreach(long palyerFactionId in PlayerFactions.Select(x => x.FactionId))
            {
                MyAPIGateway.Session.Factions.SetReputation(HostileFaciton.FactionId, palyerFactionId, -1000);
                MyAPIGateway.Session.Factions.SetReputation(palyerFactionId, HostileFaciton.FactionId, -1000);
                MyAPIGateway.Session.Factions.SetReputation(HumanFaction.FactionId, palyerFactionId, 1000);
                MyAPIGateway.Session.Factions.SetReputation(palyerFactionId, HumanFaction.FactionId, 1000);
            }

            MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
            Logger.Log("Stared faction manager", Logger.Severity.Info, Logger.LogType.Server);
        }

        public void Shutdown()
        {
            MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
        }

        private void FactionCreated(long factionId)
        {
            MyAPIGateway.Session.Factions.DeclareWar(HostileFaciton.FactionId, factionId);
            MyAPIGateway.Session.Factions.SetReputation(HostileFaciton.FactionId, factionId, -1000);
            MyAPIGateway.Session.Factions.SendPeaceRequest(HumanFaction.FactionId, factionId);
        }

        /// <summary>
        /// Assign a grid to the friendly (human) faction
        /// </summary>
        /// <param name="grid">Grid to set ownership</param>
        public void AssignGridToHumanFaction(IMyCubeGrid grid)
        {
            AssignGridToFaction(grid, HumanFaction.FounderId);
        }

        /// <summary>
        /// Assign a grid to the hostile (system) faction
        /// </summary>
        /// <param name="grid">Grid to change ownership</param>
        public void AssignGridToHostileFaction(IMyCubeGrid grid)
        {
            AssignGridToFaction(grid, HostileFaciton.FounderId);
        }

        private static void AssignGridToFaction(IMyCubeGrid grid, long ownerId)
        {
            grid.ChangeGridOwnership(ownerId, VRage.Game.MyOwnershipShareModeEnum.None);
        }

        public void Save()
        {
            PlayerFactionData data = new PlayerFactionData();
            data.FactionNames = PlayerFactions.Select(x => x.Name).ToList();
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(cDataFileName, typeof(PlayerFactionData)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
            }
        }

        private static List<IMyFaction> LoadPlayerFactions()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cDataFileName, typeof(PlayerFactionData)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cDataFileName, typeof(PlayerFactionData)))
                {
                    PlayerFactionData data = MyAPIGateway.Utilities.SerializeFromXML<PlayerFactionData>(reader.ReadToEnd());
                    List<IMyFaction> factions = new List<IMyFaction>();
                    foreach(string factionName in data.FactionNames)
                    {
                        IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByName(factionName);
                        if (faction != null)
                        {
                            factions.Add(faction);
                        }
                    }
                    return factions;
                }
            } else
            {
                return new List<IMyFaction>();
            }

        }
    }
}