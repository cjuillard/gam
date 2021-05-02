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
        EntityQuery  targetQuery;
        
        protected override void OnCreate()
        {
            boidQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {ComponentType.ReadOnly<Boid>(), ComponentType.ReadWrite<LocalToWorld>()},
            });

            RequireForUpdate(boidQuery);
            RequireForUpdate(targetQuery);
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
            float targetWeight = boidSettings.TargetWeight;
            
            float deltaTime = Time.DeltaTime;

            var boidCount = boidQuery.CalculateEntityCount();
            if (boidCount == 0)
                return;
            var targetCount = targetQuery.CalculateEntityCount();
            
            var cellAlignment = new NativeArray<float3>(boidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var cellSeparation = new NativeArray<float3>(boidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var copyTargetPositions = new NativeArray<float3>(targetCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var initialCellSeparationJobHandle = Entities
                .WithAll<Boid>()
                .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                {
                    cellSeparation[entityInQueryIndex] = localToWorld.Position;
                })
                .ScheduleParallel(Dependency);
            Dependency = initialCellSeparationJobHandle;
            
            var initialCellAlignmentJobHandle = Entities
                .WithAll<Boid>()
                .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                {
                    cellAlignment[entityInQueryIndex] = localToWorld.Forward;
                })
                .ScheduleParallel(Dependency);
            Dependency = initialCellAlignmentJobHandle;

            var copyTargetPositionsJobHandle = Entities
                .WithName("CopyTargetPositionsJob")
                .WithAll<BoidTarget>()
                .WithStoreEntityQueryInField(ref targetQuery)
                .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                {
                    copyTargetPositions[entityInQueryIndex] = localToWorld.Position;
                })
                .ScheduleParallel(Dependency);
            Dependency = copyTargetPositionsJobHandle;
            
            var boidJobHandle = Entities
                .WithName("BoidSystem")
                .ForEach((int entityInQueryIndex, ref Translation translation, ref Rotation rotation, in LocalToWorld localToWorld, in Boid boid) 
                    =>
                {
                    float3 summedPos = 0;

                    foreach (var separation in cellSeparation)
                    {
                        summedPos += separation;
                    }

                    float3 summedAlignment = 0;

                    foreach (var alignment in cellAlignment)
                    {
                        summedAlignment += alignment;
                    }

                    float3 nearestPos = 0;
                    float nearestDistSqrd = float.MaxValue;
                    foreach (var target in copyTargetPositions)
                    {
                        float currDistSq = math.distancesq(nearestPos, translation.Value);
                        if (currDistSq < nearestDistSqrd)
                        {
                            nearestDistSqrd = currDistSq;
                            nearestPos = target;
                        }
                    }

                    var separationResult = math.normalizesafe(translation.Value - (summedPos / cellSeparation.Length));
                    var cohesionResult = -separationResult;
                    var alignmentResult = math.normalizesafe((summedAlignment / cellAlignment.Length));
                    var targetHeading = math.normalizesafe(nearestPos - translation.Value);
                    
                    var targetForward = math.normalizesafe(alignmentWeight * alignmentResult 
                                                           + separationResult * separationWeight 
                                                           + cohesionResult * cohesionWeight
                                                           + targetHeading * targetWeight);

                    var newForward = localToWorld.Forward + (deltaTime * (targetForward - localToWorld.Forward));
                    rotation.Value = quaternion.LookRotationSafe(newForward, math.up());
                    
                    translation.Value += math.normalizesafe(newForward) * boid.MaxSpeed * deltaTime;
                    // float3 forward = localToWorld.Forward;
                    // forward = math.normalizesafe(forward);
                    // translation.Value += forward * boid.MaxSpeed * deltaTime;
                })
                .WithDisposeOnCompletion(cellSeparation)
                .WithDisposeOnCompletion(cellAlignment)
                .WithDisposeOnCompletion(copyTargetPositions)
                .WithReadOnly(cellSeparation)
                .WithReadOnly(cellAlignment)
                .WithReadOnly(copyTargetPositions)
                .ScheduleParallel(Dependency);

            Dependency = boidJobHandle;
        }
    }
}