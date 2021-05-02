using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Boids.Systems
{
    // https://www.red3d.com/cwr/boids/ - boids description
    // 1. Separation: steer to avoid crowding local flockmates
    // 2. Alignment: steer towards the average heading of local flockmates
    // 3. Cohesion: steer to move toward the average position of local flockmates
    public class BoidSystem : SystemBase
    {
        EntityQuery  boidQuery;

        protected override void OnCreate()
        {
            boidQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {ComponentType.ReadOnly<Boid>(), ComponentType.ReadWrite<LocalToWorld>()},
            });

            RequireForUpdate(boidQuery);
        }

        // Implementation options
        // 1. Try to loop over all boids within the loop and compare
        // 2. 
        protected override void OnUpdate()
        {
            var boidSettings = GameObject.FindObjectOfType<BoidSettings>();
            float separationWeight = boidSettings.SeparationWeight;
            float cohesionWeight = boidSettings.CohesionWeight;
            float alignmentWeight = boidSettings.AlignmentWeight;
            
            float deltaTime = Time.DeltaTime;

            var boidCount = boidQuery.CalculateEntityCount();

            // var cellAlignment = new NativeArray<float3>(boidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var cellSeparation = new NativeArray<float3>(boidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var initialCellSeparationJobHandle = Entities
                .WithAll<Boid>()
                .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                {
                    cellSeparation[entityInQueryIndex] = localToWorld.Position;
                })
                .ScheduleParallel(Dependency);


            Dependency = initialCellSeparationJobHandle;
            var boidJobHandle = Entities
                .WithName("BoidSystem")
                .ForEach((int entityInQueryIndex, ref Translation translation, ref Rotation rotation, in LocalToWorld localToWorld, in Boid boid) 
                    =>
                {
                    float3 summedPos = 0;
                    int count = 0;

                    foreach (var separation in cellSeparation)
                    {
                        summedPos += separation;
                        count++;
                    }

                    var separationResult = math.normalizesafe(translation.Value - (summedPos / count));
                    var cohesionResult = -separationResult;
                    var alignmentResult = float3.zero;  // TODO 
                    var targetHeading = math.normalizesafe(alignmentResult + separationResult * separationWeight + cohesionResult * cohesionWeight);

                    var newForward = localToWorld.Forward + (deltaTime * (targetHeading - localToWorld.Forward));
                    rotation.Value = quaternion.LookRotationSafe(newForward, math.up());
                    
                    translation.Value += math.normalizesafe(newForward) * boid.MaxSpeed * deltaTime;
                    // float3 forward = localToWorld.Forward;
                    // forward = math.normalizesafe(forward);
                    // translation.Value += forward * boid.MaxSpeed * deltaTime;
                })
                .WithDisposeOnCompletion(cellSeparation)
                .WithReadOnly(cellSeparation)
                .ScheduleParallel(Dependency);

            Dependency = boidJobHandle;
        }
    }
}