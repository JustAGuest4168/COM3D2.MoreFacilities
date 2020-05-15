using UnityEngine;

namespace COM3D2.MoreFacilities.Plugin.Core
{
    public class MoreFacilitiesManager : MonoBehaviour
    {
        public bool Initialized { get; private set; }
        public void Initialize()
        {
            //Copied from examples
            if (this.Initialized)
                return;
            MoreFacilitiesHooks.Initialize();
            this.Initialized = true;
            UnityEngine.Debug.Log("MoreFacilities: Manager Initialize");
        }

        public void Awake()
        {
            //Copied from examples
            UnityEngine.Debug.Log("MoreFacilities: Manager Awake");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);
        }
    }
}
