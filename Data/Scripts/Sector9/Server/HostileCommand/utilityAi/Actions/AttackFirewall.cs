using ParallelTasks;
using Sector9.Core;
using Sector9.Core.Logging;
using Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units;
using Sector9.Server;
using Sector9.Server.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRageMath;

namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.utilityAi.Actions
{
    internal class AttackFirewall : AiAction
    {
        private readonly List<IUnit> PossibleUnits = new List<IUnit>
        {
            new AstralSovereign(),
            new Bulldog(),
            new ExoTempest(),
            new Flatfish(),
            new Goose(),
            new Hornet(),
            new Stump(),
            new Taurus(),
            new Twinblade(),
            new Viper(),
            new Wedge()
        };
        private readonly Random Random = new Random();
        private readonly WarpQueue WarpQueue;

        public AttackFirewall(WarpQueue warpQueue) : base("Attack")
        {
            WarpQueue = warpQueue;
        }

        public override double CalulateUtility(GameState gameState)
        {
            return (gameState.Resources * 0.3) - (gameState.HumanStrength * 0.7) + gameState.IdleCycles; //if the human player is stronger then the resources, then discourage an attack
        }

        public override void Execute(GameState gameState)
        {
            Logger.Log("The ai has choosen to attack the firewall", Logger.Severity.Info, Logger.LogType.Server);
            gameState.IdleCycles = 0;
            List<IUnit> units = GetAttackUnitList(gameState);
            WarpQueue.AddItem(new WarpAttack(units));
        }

        public override int GetCost(GameState gameState)
        {
            return 100 * (gameState.GameStage * 2);
        }

        private List<IUnit> GetAttackUnitList(GameState gameState)
        {
            IUnit[] pickableUnits = PossibleUnits.Where(x => x.GameStage <= gameState.GameStage).ToArray();
            int leftToPick = gameState.GameStage;
            List<IUnit> units = new List<IUnit>(leftToPick);

            for (int i = 0; i < pickableUnits.Length; i++)
            {
                IUnit[] options = pickableUnits.Where(x => (x.GameStage *100) <= gameState.Resources).ToArray();
                if (options.Length == 0)
                {
                    break;
                }
                else if (options.Length == 1)
                {
                    units.Add(options[0]);
                    continue;
                }
                else
                {
                    units.Add(options[Random.Next(0, options.Length - 1)]);
                }

                leftToPick--;
            }


            return units;
        }

        /// <summary>
        /// Class that will execute the attack after the warp in
        /// </summary>
        private sealed class WarpAttack : IWarpQueueItem
        {
            private int Duration;
            private readonly List<IUnit> UnitList;

            public WarpAttack(List<IUnit> unitList)
            {
                Duration = unitList.Sum(x => x.GameStage) * 3600; //1 minute per game stage to warp in
                UnitList = unitList;
            }

            public void Execute()
            {
                IMyCubeGrid target = CoreSession.Instance.ServerSession.Firewall.GetFirewallGrid();
                if (target == null)
                {
                    Logger.Log("Cancelling attack, no firewall found", Logger.Severity.Info, Logger.LogType.Server);
                    return;
                }

                Random r = new Random();
                Vector3 fleetPos;
                const int distance = 5000;
                switch (r.Next(0, 3))
                {
                    case 0:
                        fleetPos = target.GetPosition() + target.WorldMatrix.Forward * distance;
                        break;
                    case 1:
                        fleetPos = target.GetPosition() + target.WorldMatrix.Right * distance;
                        break;
                    case 2:
                        fleetPos = target.GetPosition() + target.WorldMatrix.Left * distance;
                        break;
                    case 3:
                        fleetPos = target.GetPosition() + target.WorldMatrix.Backward * distance;
                        break;
                    default:
                        fleetPos = target.GetPosition() + target.WorldMatrix.Forward * distance;
                        break;

                }
                int offset = 0;
                Vector3 direction = Vector3.Normalize(target.GetPosition() - fleetPos);
                foreach (IUnit unit in UnitList.OrderBy(x => x.GameStage))
                {
                    Vector3 pos = fleetPos + direction * offset;
                    offset += unit.GameStage * 100;
                    CoreSession.Instance.ServerSession.SpawnHostileShip(unit, UnitSpawned, pos);
                }
            }

            private static void UnitSpawned(WorkData data)
            {
                ServerSession.HostileCallback callbackData = (ServerSession.HostileCallback)data;
                if (callbackData.AllGrids != null)
                {
                    var serverSession = CoreSession.Instance.ServerSession;
                    Unit unit = new Unit(callbackData.AllGrids, serverSession.UnitCommander, "testspawn", serverSession.WeaponsCore, serverSession.BlockLibrary, serverSession.Firewall.GetFirewallGrid());
                } else
                {
                    Logger.Log("Failed to spawn unit for firewall attack", Logger.Severity.Warning, Logger.LogType.Server);
                }
            }

            public bool ShouldExecuteTick()
            {
                Duration--;
                return Duration == 0;
            }
        }
    }
}
