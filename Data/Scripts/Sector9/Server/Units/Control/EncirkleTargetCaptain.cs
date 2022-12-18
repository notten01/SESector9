using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Core.Logging;
using Sector9.Server.Targets;
using Sector9.Server.Units.Behaviours;
using VRageMath;

namespace Sector9.Server.Units.Control
{
    /// <summary>
    /// captain class that will attempt to fly to a target and then encirkle it at a given distance. Will give chase if the target goes out of the encirkle area
    /// </summary>
    public class EncirkleTargetCaptain : BaseCaptain
    {
        private bool AtCirkleLocation = false;
        private const int CirkleDistance = 750;
        private const int TargetHeight = 500;
        private Vector3D EncirkleLocation = Vector3D.Zero;
        private Vector3D[] Waypoints;
        private bool CalculatingLocation = false;
        private bool IsCirckling = false;
        private MoveTo MoveToOrder;

        /// <summary>
        /// ctor for prefered target
        /// </summary>
        /// <param name="unit">Unit instance the captain has control of</param>
        /// <param name="target">Specific target to hunt for</param>
        public EncirkleTargetCaptain(ITarget target, Planets planets) : base(TargetPreference.Player, planets)
        {
            Target = target;
        }

        /// <summary>
        /// ctor for free target, will select one that is closest
        /// </summary>
        /// <param name="unit">Unit the captain has control of</param>
        /// <param name="preference">Prefered target type</param>
        public EncirkleTargetCaptain(TargetPreference preference, Planets planets) : base(preference, planets)
        {
        }

        public override void UnitTick()
        {
            //check if there is a target, if not get a target
            if (Target == null)
            {
                Targeting = true;
                MyAPIGateway.Parallel.StartBackground(LocateTarget);
                return;
            }

            //check if we have a intercept point with the target, if not calcuate it and fly towards it
            if (!AtCirkleLocation && MoveToOrder == null)
            {
                if (EncirkleLocation == Vector3D.Zero)
                {
                    if (!CalculatingLocation)
                    {
                        MyAPIGateway.Parallel.StartBackground(CalculateMovetoPositionn);
                        CalculatingLocation = true;
                    }
                    return;
                }
                Logger.Log("Setting new moveto order", Logger.Severity.Info, Logger.LogType.Server);
                MoveToOrder = new MoveTo(EncirkleLocation);
                Unit.SetBehaviour(MoveToOrder);
                return;
            }

            //The ship is moving to the intercept point, but has not arrived yet
            if (MoveToOrder != null && !MoveToOrder.IsComplete)
            {
                return;
            }
            else if (MoveToOrder != null && MoveToOrder.IsComplete)
            {
                AtCirkleLocation = true;
                MoveToOrder = null;
            }
            //the ship is on the intercept point, but has not started to cirkle around the target yet
            if (!IsCirckling && AtCirkleLocation)
            {
                Unit.SetBehaviour(new Encirkle(Waypoints));
                IsCirckling = true;
                return;
            }

            //the ship is cirkling around the target, check if the target is still slive
            if (!Target.IsValid())
            {
                //target is dead, reset behaviour and begin again
                Target = null;
                AtCirkleLocation = false;
                IsCirckling = false;
                CalculatingLocation = false;
                return;
            }

            //target still alive, ship is cirkling, check if target still within area
            var targetPosition = Target.GetPosition();
            if (Vector3D.Distance(targetPosition, LastPingedTargetLocation) > CirkleDistance / 2)
            {
                //target has moved out of range, move to target
                AtCirkleLocation = false;
                CalculatingLocation = false;
                IsCirckling = false;
                LastPingedTargetLocation = targetPosition;
            }
        }

        private void CalculateMovetoPositionn()
        {
            Vector3D targetPosition = Target.GetPosition();
            MyPlanet planet = Planets.GetPlanetOfPoint(targetPosition);
            if (planet != null)
            {
                Vector3D forward;
                Vector3D up = Planets.Reverse(Planets.GetGravityDirection(planet, targetPosition));
                up.CalculatePerpendicularVector(out forward);
                Vector3D targetVector = planet.GetClosestSurfacePointGlobal(ref targetPosition);
                up.Normalize();
                forward.Normalize();
                Vector3D backward = -forward;
                Vector3D left = Vector3D.Cross(forward, up);
                Vector3D right = -left;
                Vector3D forwardPoint = targetVector;
                forwardPoint += up * TargetHeight;
                forwardPoint += forward * CirkleDistance;
                Vector3D backPoint = targetVector;
                backPoint += up * TargetHeight;
                backPoint += backward * CirkleDistance;
                Vector3D leftPoint = targetVector;
                leftPoint += up * TargetHeight;
                leftPoint += left * CirkleDistance;
                Vector3D rightPoint = targetVector;
                rightPoint += up * TargetHeight;
                rightPoint += right * CirkleDistance;
                Waypoints = new Vector3D[4] { forwardPoint, rightPoint, backPoint, leftPoint };
                EncirkleLocation = forwardPoint;
                Logger.Log($"Encirkle captain calculated planetary target {targetPosition}", Logger.Severity.Info, Logger.LogType.Server);
            }
            else
            {
                Vector3D forwardLocation = targetPosition;
                forwardLocation += Vector3D.Up * TargetHeight;
                forwardLocation += Vector3D.Forward * CirkleDistance;
                Vector3D backposition = targetPosition;
                backposition += Vector3D.Backward * CirkleDistance;
                backposition += Vector3D.Down * TargetHeight;
                Vector3D leftPosition = targetPosition;
                leftPosition += Vector3D.Left * CirkleDistance;
                Vector3D rightPosition = targetPosition;
                rightPosition += Vector3D.Right * CirkleDistance;

                Waypoints = new Vector3D[4] { forwardLocation, rightPosition, backposition, leftPosition };
                EncirkleLocation = forwardLocation;
                Logger.Log($"Encirkle captain calcualted space target {targetPosition}", Logger.Severity.Info, Logger.LogType.Server);
            }
        }
    }
}