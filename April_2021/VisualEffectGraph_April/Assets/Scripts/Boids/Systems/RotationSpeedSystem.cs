using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Systems
{
    public class RotationSpeedSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            Entities
                .WithName("RotationSpeedSystem_ForEach")
                .ForEach((ref Rotation rotation, in RotationSpeed rotationSpeed) 
                    => DoRotation(ref rotation, rotationSpeed.RadiansPerSecond * deltaTime))
                .ScheduleParallel();
        }

        static void DoRotation(ref Rotation rotation, float amount)
        {
            rotation.Value = math.mul(
                math.normalize(rotation.Value), 
                quaternion.AxisAngle(-math.forward(), amount));
        }
    }
}