using UnityEngine;

namespace Boids
{
    public class BoidSettings : MonoBehaviour
    {
        public float SeparationWeight = .1f;
        public float CohesionWeight = .75f;
        public float AlignmentWeight = .75f;
        public float TargetWeight = .5f;
    }
}