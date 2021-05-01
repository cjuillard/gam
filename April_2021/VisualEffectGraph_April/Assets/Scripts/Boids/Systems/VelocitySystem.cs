using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Systems
{
    public class VelocitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            Entities
                .WithName("VelocitySystem")
                .ForEach((ref Translation translation, in Velocity velocity) 
                    =>
                {
                    float3 newPos = translation.Value + new float3(velocity.Value * deltaTime, 0);
                    translation.Value = newPos;
                })
                .ScheduleParallel();
        }
    }
}