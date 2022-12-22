using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using Sector9.Api;
using Sector9.Core;
using Sector9.Core.Logging;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using static Sector9.Api.WcApiDef;
using static Sector9.Api.WcApiDef.WeaponDefinition;

namespace Sector9.Server
{
    internal class Provider : ITickable
    {
        private readonly List<MyReactor> Reactors;
        private readonly List<MyBatteryBlock> Batteries;
        private readonly List<WeaponWrapper> Weapons;

        private readonly List<IMyGasTank> Tanks;
        private readonly MyObjectBuilder_Ingot FuelItem;

        //todo: hydrogen tanks

        public Provider(ListReader<MyCubeBlock> fatBlocks, Wc weaponsCore, DefinitionLibrary library)
        {
            Reactors = fatBlocks.OfType<MyReactor>().ToList();
            Batteries = fatBlocks.OfType<MyBatteryBlock>().ToList();
            var uraniumId = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "Uranium");
            FuelItem = MyObjectBuilderSerializer.CreateNewObject(uraniumId) as MyObjectBuilder_Ingot;
            Weapons = new List<WeaponWrapper>();
            Tanks = fatBlocks.OfType<IMyGasTank>().ToList();

            Weapons.AddRange(fatBlocks.Where(block => library.AllWeapons.Contains(block.BlockDefinition.Id) && !block.Closed).Select(block => new WeaponWrapper(block, weaponsCore)));

            Tick(); //tick once to ensure everything is set
        }

        public void Tick()
        {
            foreach (WeaponWrapper weapon in Weapons)
            {
                weapon.Tick();
            }

            foreach (MyBatteryBlock battery in Batteries)
            {
                if (battery.Closed) { continue; }
                battery.CurrentStoredPower = battery.MaxStoredPower;
            }

            foreach (MyReactor reactor in Reactors)
            {
                if (reactor.Closed) { continue; }
                var inventory = reactor.GetInventory();
                if (inventory.GetItemAmount(FuelItem.GetId()) <= 1)
                {
                    inventory.AddItems(5, FuelItem);
                }
            }

            foreach (IMyGasTank tank in Tanks)
            {
                if (tank.Closed) { continue; }
                var sinkcomp = tank.Components.Get<MyResourceSinkComponent>();
                sinkcomp.SetMaxRequiredInputByType(MyResourceDistributorComponent.HydrogenId, float.MaxValue);
                sinkcomp.SetInputFromDistributor(MyResourceDistributorComponent.HydrogenId, float.MaxValue, true);
            }
        }

        private sealed class WeaponWrapper : ITickable
        {
            private readonly IMyCubeBlock Block;
            private readonly List<SubWeapon> SubWeapons;

            public bool IsValid { get; private set; }

            public WeaponWrapper(MyCubeBlock block, Wc weaponsCore)
            {
                SubWeapons = new List<SubWeapon>();
                Block = block;
                IMyTerminalBlock termBlock = block as IMyTerminalBlock;
                Dictionary<string, int> weaponsInBlock = new Dictionary<string, int>();
                weaponsCore.GetBlockWeaponMap(termBlock, weaponsInBlock);
                weaponsCore.DisableRequiredPower(termBlock);
                foreach (var weaponName in weaponsInBlock.Keys)
                {
                    foreach (WeaponDefinition definition in weaponsCore.WeaponDefinitions)
                    {
                        Dictionary<string, AmmoDef> knownAmmoTypes = new Dictionary<string, AmmoDef>();
                        foreach (var ammoType in definition.Ammos)
                        {
                            if (!knownAmmoTypes.ContainsKey(ammoType.AmmoRound))
                            {
                                knownAmmoTypes.Add(ammoType.AmmoRound, ammoType);
                            }
                        }

                        if (definition.HardPoint.WeaponName == weaponName)
                        {
                            var weaponId = weaponsInBlock[weaponName];
                            SubWeapons.Add(new SubWeapon(definition, weaponsCore.GetActiveAmmo(termBlock, weaponId), block, knownAmmoTypes));
                            break;
                        }
                    }
                }
                IsValid = true;
            }

            public void Tick()
            {
                if (!IsValid || Block.Closed)
                {
                    IsValid = false;
                    return;
                }

                foreach (var sub in SubWeapons)
                {
                    sub.CheckAndSupply();
                }
            }
        }

        private sealed class SubWeapon
        {
            private readonly AmmoDef Ammo;
            private readonly bool NeedsAmmo;
            private readonly MyDefinitionId Magazine;
            private readonly MyInventory Inventory;
            private readonly MyFixedPoint MaxFit;

            public SubWeapon(WeaponDefinition definition, string ammo, MyCubeBlock block, Dictionary<string, AmmoDef> knownAmmoTypes)
            {
                if (string.IsNullOrEmpty(ammo))
                {
                    Ammo = definition.Ammos[0];
                }
                else
                {
                    Ammo = knownAmmoTypes[ammo];
                }
                Magazine = new MyDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), Ammo.AmmoMagazine);

                NeedsAmmo = Magazine.SubtypeName != "Energy";
                if (NeedsAmmo)
                {
                    Inventory = block.GetInventory();
                    if (Inventory == null)
                    {
                        Logger.Log($"Inventory is null of block {block.Name}!", Logger.Severity.Fatal, Logger.LogType.Server);
                        NeedsAmmo = false;
                        return;
                    }
                    float maxItemCount = Inventory.ComputeAmountThatFits(Magazine).ToIntSafe();
                    double halfSize = Math.Floor(maxItemCount / 2f);
                    MaxFit = Convert.ToInt32(Math.Max(1, halfSize));
                }
                CheckAndSupply();
            }

            public void CheckAndSupply()
            {
                if (!NeedsAmmo) { return; }
                var ammoCount = Inventory.GetItemAmount(Magazine);
                if (ammoCount < 1)
                {
                    Logger.Log($"Adding {MaxFit} of ammo {Magazine.SubtypeName} to weapon", Logger.Severity.Info, Logger.LogType.Server);
                    Inventory.AddItems(MaxFit, MyObjectBuilderSerializer.CreateNewObject(Magazine));
                }
            }
        }
    }
}