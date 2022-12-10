using Sector9.Api;
using Sector9.Core.Logging;
using System.Collections.Generic;
using VRage.Game;

namespace Server.Data
{
    /// <summary>
    /// contains meta data about types of blocks such as weapons and custom items
    /// </summary>
    public class DefinitionLibrary
    {
        public List<MyDefinitionId> AllWeapons { get; }
        public List<MyDefinitionId> StaticWeapons { get; }
        public List<MyDefinitionId> TurretWeapons { get; }

        public DefinitionLibrary(Wc weaponsCore)
        {
            AllWeapons = new List<MyDefinitionId>();
            StaticWeapons = new List<MyDefinitionId>();
            TurretWeapons = new List<MyDefinitionId>();

            LoadWc(weaponsCore);
            Logger.Log("Loaded Definition library", Logger.Severity.Info, Logger.LogType.Server);
        }

        private void LoadWc(Wc weaponsCore)
        {
            weaponsCore.GetAllCoreWeapons(AllWeapons);
            weaponsCore.GetAllCoreTurrets(TurretWeapons);
            weaponsCore.GetAllCoreStaticLaunchers(StaticWeapons);
        }
    }
}