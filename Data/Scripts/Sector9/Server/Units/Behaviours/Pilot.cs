using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;
using VRageMath;

namespace Sector9.Server.Units.Behaviours
{
    /// <summary>
    /// the actual 'pilot' that control the remote control.
    /// Basically a utility function to get things to move to palces
    /// </summary>
    public class Pilot : ITickable
    {
        private readonly IMyRemoteControl RemoteControl;
        private float Altitude;
        private bool CompletedMove;
        private MyPlanet CurrentPlanet;
        private Vector3D CurrentVelocity;
        private Vector3D DesiredPosition;
        private Planets Planets;
        private Vector3D ReversedGrafity;
        private float Roll;
        private Vector2D Rotation;
        private bool RunningInBasicMode = true;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="remoteControl">The remote control the pilot has control of</param>
        public Pilot(IMyRemoteControl remoteControl, Planets planets)
        {
            RemoteControl = remoteControl;
            CurrentPosition = Vector3D.Zero;
            DesiredPosition = Vector3D.Zero;
            Rotation = Vector2D.Zero;
            ReversedGrafity = Vector3D.Zero;
            CurrentVelocity = Vector3D.Zero;
            Roll = 0;
            Altitude = 0;
            Planets = planets;
            CompletedMove = false;
        }

        public Vector3D CurrentPosition { get; private set; }

        /// <summary>
        /// Initialize the auto pilot
        /// </summary>
        public void Init()
        {
            CurrentPosition = RemoteControl.GetPosition();
            if (RemoteControl?.SlimBlock?.CubeGrid?.Physics != null)
            {
                CurrentVelocity = RemoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity;
            }
            else
            {
                CurrentVelocity = Vector3D.Zero;
            }

            var planet = Planets.GetPlanetOfPoint(CurrentPosition);
            if (planet != null)
            {
                CurrentPlanet = planet;
                ReversedGrafity = Planets.Reverse(Planets.GetGravityDirection(planet, CurrentPosition));
            }
            else
            {
                CurrentPlanet = null;
                ReversedGrafity = Vector3D.Zero;
            }
        }

        public void SetAutoPilotBasicEncirkle(Vector3D[] waypoints)
        {
            if (!CompletedMove)
            {
                Stop();
            }

            Logger.Log($"Pilot now cirkling around {waypoints.Length} points", Logger.Severity.Info, Logger.LogType.Server);
            RunningInBasicMode = true;
            DesiredPosition = waypoints[0];
            for (int i = 0; i < waypoints.Length; i++)
            {
                RemoteControl.AddWaypoint(waypoints[i], $"exec encirkle {i}");
            }
            RemoteControl.FlightMode = Sandbox.ModAPI.Ingame.FlightMode.Circle;
            RemoteControl.SetAutoPilotEnabled(true);
            CompletedMove = false;
        }

        public void SetAutoPilotBasicTarget(Vector3D target)
        {
            if (!CompletedMove)
            {
                Stop();
            }

            Logger.Log($"Setting pilot moveto to {target}", Logger.Severity.Info, Logger.LogType.Server);
            DesiredPosition = target;
            RunningInBasicMode = true;
            RemoteControl.AddWaypoint(DesiredPosition, "Exec move");
            RemoteControl.FlightMode = Sandbox.ModAPI.Ingame.FlightMode.OneWay;
            RemoteControl.SetAutoPilotEnabled(true);
            CompletedMove = false;
        }

        public void Stop()
        {
            StandDownBasicAutoPilot();
            CompletedMove = true;
        }

        public void Tick()
        {
            CurrentPosition = RemoteControl.GetPosition();

            if (!RunningInBasicMode)
            {
                UpdateManualAutoPilot();
            }
        }

        private void StandDownBasicAutoPilot()
        {
            if (RunningInBasicMode)
            {
                Logger.Log("Shutting down autopilot basic move", Logger.Severity.Info, Logger.LogType.Server);
                RemoteControl.SetAutoPilotEnabled(false);
                RemoteControl.ClearWaypoints();
            }
        }

        private void UpdateManualAutoPilot()
        {
        }
    }
}