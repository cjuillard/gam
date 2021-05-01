using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class BoidSchoolAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject Prefab;
        public float InitialRadius;
        public int Count;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Prefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new BoidSchool
            {
                Prefab = conversionSystem.GetPrimaryEntity(Prefab),
                Count = Count,
                InitialRadius = InitialRadius
            });
        }
    }
}