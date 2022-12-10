using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Core.Logging;
using System.Collections.Generic;
using System.Linq;
using VRage.ModAPI;
using VRageMath;

namespace Sector9.Server
{
    /// <summary>
    /// Utility class to handle planets within the game world
    /// </summary>
    public class Planets
    {
        private readonly HashSet<IMyEntity> SessionPlanets;

        public Planets()
        {
            SessionPlanets = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(SessionPlanets, x => x is MyPlanet);
            Logger.Log($"Loaded {SessionPlanets.Count} planets", Logger.Severity.Info, Logger.LogType.Server);
        }

        /// <summary>
        /// Get the planet for a given point
        /// </summary>
        /// <param name="position">Point that you want to know is within a planet</param>
        /// <returns>The <see cref="MyPlanet"/> or null if in space</returns>
        public MyPlanet GetPlanetOfPoint(Vector3 position)
        {
            var planet = SessionPlanets.FirstOrDefault(x => x.Components.Get<MyGravityProviderComponent>().IsPositionInRange(position));
            if (planet != null)
            {
                Logger.Log($"Found planet {planet.Name} for position {position}", Logger.Severity.Info, Logger.LogType.Server);
                return planet as MyPlanet;
            }
            Logger.Log($"Did not find planet for {position}", Logger.Severity.Info, Logger.LogType.Server);
            return null;
        }

        public static Vector3D GetGravityDirection(MyPlanet planet, Vector3D location)
        {
            var ent = planet as IMyEntity;
            var grafity = ent.GetPosition();
            return new Vector3D(grafity.X - location.X, grafity.Y - location.Y, grafity.Z - location.Z);
        }
    }
}