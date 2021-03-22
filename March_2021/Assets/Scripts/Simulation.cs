using UnityEngine;

namespace UnityTemplateProjects
{
    public class Simulation : MonoBehaviour
    {
        public Character Player;

        private void Awake()
        {
            Player = FindObjectOfType<Character>();
        }
    }
}