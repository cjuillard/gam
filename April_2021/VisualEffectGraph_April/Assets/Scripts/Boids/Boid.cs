using Unity.Entities;
using Unity.Mathematics;

// https://www.red3d.com/cwr/boids/ - boids description
// 1. Separation: steer to avoid crowding local flockmates
// 2. Alignment: steer towards the average heading of local flockmates
// 3. Cohesion: steer to move toward the average position of local flockmates
namespace Boids
{
    [GenerateAuthoringComponent]
    public struct Boid : IComponentData
    {
        public float2 Velocity;
    }
}
