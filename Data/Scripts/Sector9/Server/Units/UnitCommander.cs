using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;
using System.Collections.Generic;

namespace Sector9.Server.Units
{
    public class UnitCommander : ITickable
    {
        private readonly List<Unit> Units;
        private readonly List<Unit> ToRemove;
        private readonly DamageHandler DamageHandler;

        public UnitCommander(DamageHandler damageHandler)
        {
            Units = new List<Unit>();
            ToRemove = new List<Unit>();
            DamageHandler = damageHandler;
        }

        public void RegisterUnit(Unit unit)
        {
            Logger.Log("Registered new unit to commander", Logger.Severity.Info, Logger.LogType.Server);
            unit.SetDamageHandler(DamageHandler);
            Units.Add(unit);
        }

        public void UnregisterUnit(Unit unit)
        {
            Logger.Log("Unregistered unit from commander", Logger.Severity.Info, Logger.LogType.Server);
            DamageHandler.UnTrackEntity(unit.GetId());
            ToRemove.Add(unit);
        }

        public void Tick()
        {
            MyAPIGateway.Parallel.ForEach(Units, (unit) => { unit?.Tick(); });

            /*foreach (var unit in Units)
            {
                unit.Tick();
            }*/

            //remove invalid units
            if (ToRemove.Count > 0)
            {
                foreach (Unit unit in ToRemove)
                {
                    Units.Remove(unit);
                }
                ToRemove.Clear();
            }
        }
    }
}