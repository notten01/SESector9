using Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units;
using System;
using System.Collections.Generic;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.actions
{
    internal class AttackAction : IQueuedAction
    {
        private readonly List<IUnit> Units;
        private int Countdown;

        public AttackAction(List<IUnit> units, int countdown)
        {
            Units = units;
            Countdown = countdown;
        }

        public void Execute()
        {
        }

        public void Tick()
        {
            Countdown--;
            if (Countdown == 0)
            {
                Execute();
            }
        }
    }
}
