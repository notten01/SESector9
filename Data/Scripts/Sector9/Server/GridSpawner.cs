﻿using Sandbox.Definitions;
using Sandbox.ModAPI;
using Sector9.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Sector9.Server
{
    /// <summary>
    /// Class that handles the spawning of new grids. Pure utilitarian, does not know function of said grid
    /// </summary>
    internal class GridSpawner
    {
        private readonly DictionaryReader<string, MyPrefabDefinition> DefinitionsCache;

        public GridSpawner()
        {
            DefinitionsCache = MyDefinitionManager.Static.GetPrefabDefinitions();
            foreach (var definition in DefinitionsCache)
            {
                Logger.Log($"Founnd grid definition {definition.Key}", Logger.Severity.Info, Logger.LogType.Server);
            }
        }

        /// <summary>
        /// Spawn a grid (including subgrids) on a given location
        /// </summary>
        /// <param name="name">Name of prefab to the grid('s) from</param>
        /// <param name="Position">Positon in world matrix where to spawn the ship</param>
        /// <param name="spawnedGrids">List of grids that got spawned</param>
        /// <returns>Spawning was a success</returns>
        public bool TrySpawnGrid(string name, MatrixD Position, out List<IMyEntity> spawnedGrids)
        {
            MyPrefabDefinition definition;
            if (!DefinitionsCache.TryGetValue(name, out definition))
            {
                Logger.Log($"Could not find grid {name} to spawn!", Logger.Severity.Error, Logger.LogType.Server);
                spawnedGrids = null;
                return false;
            }

            var size = GetGridSize(definition);
            var distance = (Math.Sqrt(size.LengthSquared()) * ToGridLength(definition.CubeGrids[0].GridSizeEnum) / 2);
            var position = Position.Translation + Position.Forward * distance;
            var offset = position - definition.CubeGrids[0].PositionAndOrientation.Value.Position; //0 is the 'main' grid
            var tmpList = new List<MyObjectBuilder_EntityBase>();

            //grid can have subgrids, itterate and attach them
            foreach (var grid in definition.CubeGrids)
            {
                var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(grid.PositionAndOrientation.Value.Position + offset, grid.PositionAndOrientation.Value.Forward, grid.PositionAndOrientation.Value.Up);
                tmpList.Add(gridBuilder);
            }
            spawnedGrids = CreateAndSyncEntities(tmpList);
            Logger.Log($"Spawned in grid {name}", Logger.Severity.Info, Logger.LogType.Server);
            return true;
        }

        private static Vector3 GetGridSize(MyPrefabDefinition prefab)
        {
            Vector3I min = Vector3I.MaxValue;
            Vector3I max = Vector3I.MinValue;
            foreach (var gridMin in prefab.CubeGrids[0].CubeBlocks.Select(x => x.Min))
            {
                min = Vector3I.Min(gridMin, min);
                max = Vector3I.Max(gridMin, max);
            }
            Vector3D size = new Vector3(max - min);
            return size;
        }

        private static float ToGridLength(MyCubeSize cubeSize)
        {
            return MyDefinitionManager.Static.GetCubeSize(cubeSize);
        }

        private List<IMyEntity> CreateAndSyncEntities(List<MyObjectBuilder_EntityBase> entities)
        {
            MyAPIGateway.Entities.RemapObjectBuilderCollection(entities);
            List<IMyEntity> output = entities.Select(objBase => MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(objBase)).ToList();
            return output;
        }
    }
}