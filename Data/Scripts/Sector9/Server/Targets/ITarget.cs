using VRageMath;

namespace Sector9.Server.Targets
{
    public interface ITarget
    {
        bool IsValid();

        Vector3D GetPosition();
    }
}