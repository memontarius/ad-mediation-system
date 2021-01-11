using UnityEngine;

namespace Virterix.Common
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T m_instance;

        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = (T)FindObjectOfType(typeof(T));
                    if (m_instance == null)
                    {
                        Debug.LogWarning("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
                    }
                }
                return m_instance;
            }
        }
    }
} // namespace Virterix.Common