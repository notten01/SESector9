using Sandbox.Game.Entities;
using Sector9.Core.Logging;
using Sector9.Server.Targets;
using VRageMath;

namespace Sector9.Server.Units.Control
{
    public abstract class BaseCaptain : ICaptain
    {
        protected readonly TargetPreference PreferedTarget;
        protected Unit Unit;
        protected bool ReadyToInit = false;
        protected MyRemoteControl RemoteControl;
        protected ITarget Target;
        protected bool Targeting = false;
        protected Planets Planets;
        protected Vector3D LastPingedTargetLocation = Vector3D.Zero;

        protected BaseCaptain(TargetPreference preferedTarget, Planets planets)
        {
            PreferedTarget = preferedTarget;
            Planets = planets;
        }

        public enum TargetPreference
        {
            Player,
            Grid,
            Ship,
            Station
        }

        public void SetCaptainData(MyRemoteControl remoteControl, Unit unit)
        {
            Unit = unit;
            RemoteControl = remoteControl;
            ReadyToInit = true;
        }

        public void Tick()
        {
            if (!ReadyToInit) return;
            if (Targeting) return;
            UnitTick();
        }

        public abstract void UnitTick();

        protected void LocateTarget()
        {
            switch (PreferedTarget)
            {
                case TargetPreference.Player:
                    Target = TargetUtilities.SelectClosestPlayer(RemoteControl);
                    break;

                case TargetPreference.Grid:
                    Target = TargetUtilities.SelectClosestTarget();
                    break;

                case TargetPreference.Ship:
                    Target = TargetUtilities.SelectClosestShip();
                    break;

                case TargetPreference.Station:
                    Target = TargetUtilities.SelectClosestStation();
                    break;

                default:
                    Logger.Log($"Unkown target preference {PreferedTarget}", Logger.Severity.Error, Logger.LogType.Server);
                    Target = TargetUtilities.SelectClosestPlayer(RemoteControl);
                    break;
            }
            Logger.Log($"Unit {RemoteControl.CubeGrid.EntityId} targeted a {PreferedTarget}", Logger.Severity.Info, Logger.LogType.Server);
            LastPingedTargetLocation = Target.GetPosition();
            Targeting = false;
        }
    }
}