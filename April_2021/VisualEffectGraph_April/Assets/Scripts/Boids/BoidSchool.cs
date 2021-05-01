using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Profiling;

namespace Boids
{
    public struct BoidSchool : IComponentData
    {
        public Entity Prefab;
        public float InitialRadius;
        public int Count;
    }
    
    public class BoidSchoolSpawnSystem : SystemBase
    {
        [BurstCompile]
        struct SetBoidLocalToWorld : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> TranslationFromEntity;
        
            public NativeArray<Entity> Entities;
            public float3 Center;
            public float Radius;
        
            public void Execute(int i)
            {
                var entity = Entities[i];
                var random = new Random(((uint)(entity.Index + i + 1) * 0x9F6ABC1));
                var dir = math.normalizesafe(random.NextFloat2() - new float2(0.5f, 0.5f));
                var pos = Center.xy + (dir * Radius * random.NextFloat());
                TranslationFromEntity[entity] = new Translation {Value = new float3(pos, Center.z)};
                // var localToWorld = new LocalToWorld
                // {
                //     Value = float4x4.TRS(pos, quaternion.LookRotationSafe(dir, math.up()), new float3(1.0f, 1.0f, 1.0f))
                // };
                // TranslationFromEntity[entity] = localToWorld;
            }
        }

        protected override void OnUpdate()
        {
            Entities.WithStructuralChanges().ForEach((Entity entity, int entityInQueryIndex, in BoidSchool boidSchool, in LocalToWorld boidSchoolLocalToWorld) =>
            {
                var boidEntities = new NativeArray<Entity>(boidSchool.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                Profiler.BeginSample("Instantiate");
                EntityManager.Instantiate(boidSchool.Prefab, boidEntities);
                Profiler.EndSample();

                var translationBoids = GetComponentDataFromEntity<Translation>();
                var setBoidLocalToWorldJob = new SetBoidLocalToWorld
                {
                    TranslationFromEntity = translationBoids,
                    Entities = boidEntities,
                    Center = boidSchoolLocalToWorld.Position,
                    Radius = boidSchool.InitialRadius
                };
                Dependency = setBoidLocalToWorldJob.Schedule(boidSchool.Count, 64, Dependency);
                Dependency = boidEntities.Dispose(Dependency);

                EntityManager.DestroyEntity(entity);
            }).Run();
        }
    }
}