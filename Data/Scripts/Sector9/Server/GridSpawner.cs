using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units;
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
        private readonly Planets Planets;

        public GridSpawner(Planets planets)
        {
            DefinitionsCache = MyDefinitionManager.Static.GetPrefabDefinitions();
            Planets = planets;
            Logger.Log("Started grid spawner", Logger.Severity.Info, Logger.LogType.Server);
        }

        /// <summary>
        /// Spawn a grid (including subgrids) on a given location
        /// </summary>
        /// <param name="name">Name of prefab to the grid('s) from</param>
        /// <param name="location">Positon in world matrix where to spawn the ship</param>
        /// <param name="height">Height above the ground (if applicable) to spawn</param>
        /// <param name="spawnedGrids">List of grids that got spawned</param>
        /// <param name="callBack">Callback function called once entity list is ready</param>
        /// <returns>Total number of grids spawning</returns>
        public int TrySpawnGrid(string name, MatrixD location, int height, out List<IMyEntity> spawnedGrids, Action<IMyEntity> callBack)
        {
            bool onPlanet;
            MatrixD position = PlanetSafety(location, height, out onPlanet).GetMatrix();
            return SpawnGridEntities(name, out spawnedGrids, callBack, position);
        }

        /// <summary>
        /// Spawn a grid (including subgrids) on a given location
        /// </summary>
        /// <param name="unit">Unit to spawn</param>
        /// <param name="location">Positon in world matrix where to spawn the ship</param>
        /// <param name="height">Height above the ground (if applicable) to spawn</param>
        /// <param name="spawnedGrids">List of grids that got spawned</param>
        /// <param name="callBack">Callback function called once entity list is ready</param>
        /// <returns>Total number of grids spawning</returns>
        public int TrySpawnGrid(IUnit unit, MatrixD location, int height, out List<IMyEntity> spawnedGrids, Action<IMyEntity> callBack)
        {
            bool onPlanet;
            MatrixD position = PlanetSafety(location, height, out onPlanet).GetMatrix();
            string unitName;
            if (onPlanet)
            {
                unitName =unit.GetGridName(UnitType.Atmosphere);
            } else
            {
                unitName = unit.GetGridName(UnitType.Space);
            }
            return SpawnGridEntities(unitName, out spawnedGrids, callBack, position);
        }

        private int SpawnGridEntities(string name, out List<IMyEntity> spawnedGrids, Action<IMyEntity> callBack, MatrixD position)
        {
            MyPrefabDefinition definition;
            if (!DefinitionsCache.TryGetValue(name, out definition))
            {
                Logger.Log($"Could not find grid {name} to spawn!", Logger.Severity.Error, Logger.LogType.Server);
                spawnedGrids = null;
                return 0;
            }
            var size = GetGridSize(definition);
            var distance = (Math.Sqrt(size.LengthSquared()) * ToGridLength(definition.CubeGrids[0].GridSizeEnum) / 2);
            var spawnPoint = position.Translation + position.Forward * distance;

            var offset = spawnPoint - definition.CubeGrids[0].PositionAndOrientation.Value.Position; //0 is the 'main' grid
            var tmpList = new List<MyObjectBuilder_EntityBase>();

            //grid can have subgrids, itterate and attach them
            foreach (var grid in definition.CubeGrids)
            {
                var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(grid.PositionAndOrientation.Value.Position + offset, position.Forward, position.Up);
                tmpList.Add(gridBuilder);
            }
            spawnedGrids = CreateAndSyncEntities(tmpList, callBack);
            Logger.Log($"Spawned in grid {name}", Logger.Severity.Info, Logger.LogType.Server);
            return tmpList.Count;
        }

        private MyPositionAndOrientation PlanetSafety(MatrixD location, int height, out bool onPlanet)
        {
            Vector3D position = location.Translation;
            MyPlanet planet = Planets.GetPlanetOfPoint(position);

            if (planet == null)
            {
                onPlanet = false;
                return new MyPositionAndOrientation(location);
            }
            onPlanet = true;
            Vector3D gravity = Planets.GetGravityDirection(planet, position);
            Vector3D reversedGravityVector = Planets.Reverse(gravity);
            Vector3D forwardVector;
            reversedGravityVector.CalculatePerpendicularVector(out forwardVector);
            Vector3D targetVector = planet.GetClosestSurfacePointGlobal(ref position);
            Vector3D normalizedUp = reversedGravityVector;
            normalizedUp.Normalize();
            if (height != 0)
            {
                targetVector += reversedGravityVector * height; //hack: I don't know
            }
            return new MyPositionAndOrientation(targetVector, forwardVector, reversedGravityVector);
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

        private static List<IMyEntity> CreateAndSyncEntities(List<MyObjectBuilder_EntityBase> entities, Action<IMyEntity> callback)
        {
            MyAPIGateway.Entities.RemapObjectBuilderCollection(entities);
            List<IMyEntity> output = entities.Select(objBase => MyAPIGateway.Entities.CreateFromObjectBuilderParallel(objBase, true, callback)).ToList();
            return output;
        }
    }
}