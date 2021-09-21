using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "BeanProfile", menuName = "Bean/Stats", order = 0)]
    public class BeanStatScript : ScriptableObject
    {
        public float score;
    }
}