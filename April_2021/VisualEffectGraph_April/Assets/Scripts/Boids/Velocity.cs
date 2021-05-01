using Unity.Entities;
using Unity.Mathematics;

namespace Boids
{
    [GenerateAuthoringComponent]
    public struct Velocity : IComponentData
    {
        public float2 Value;
    }
}