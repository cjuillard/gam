using Unity.Entities;

namespace Boids
{
    [GenerateAuthoringComponent]
    public struct Boid : IComponentData
    {
        public float MaxSpeed;
    }
}
