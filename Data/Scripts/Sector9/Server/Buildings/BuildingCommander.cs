using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;
using System.Collections.Generic;

namespace Sector9.Server.Buildings
{
    public class BuildingCommander : ITickable
    {
        private readonly List<Building> Buildings;
        private readonly List<Building> ToRemove;
        private readonly DamageHandler DamageHandler;

        public BuildingCommander(DamageHandler damageHandler)
        {
            Buildings = new List<Building>();
            ToRemove = new List<Building>();
            DamageHandler = damageHandler;
        }

        public void RegisterBuilding(Building building)
        {
            Logger.Log("Registered new building to commander", Logger.Severity.Info, Logger.LogType.Server);
            building.SetDamageHandler(DamageHandler);
            Buildings.Add(building);
        }

        public void UnregisterBuilding(Building building)
        {
            Logger.Log("Unregistered building from commander", Logger.Severity.Info, Logger.LogType.Server);
            DamageHandler.UnTrackEntity(building.GetId());
            ToRemove.Add(building);
        }

        public void Tick()
        {
            MyAPIGateway.Parallel.ForEach(Buildings, (building) => { building?.Tick(); }); //perhaps not do in parralel? not sure if that is save

            if (ToRemove.Count > 0)
            {
                foreach (Building building in ToRemove)
                {
                    Buildings.Remove(building);
                }
                ToRemove.Clear();
            }
        }
    }
}