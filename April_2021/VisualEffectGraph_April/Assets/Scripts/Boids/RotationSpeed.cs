using Unity.Entities;

namespace Boids
{
    [GenerateAuthoringComponent]
    public struct RotationSpeed : IComponentData
    {
        public float RadiansPerSecond;
    }
}