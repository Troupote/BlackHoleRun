using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace BHR
{
    public class PlayableZoneController : MonoBehaviour
    {
        private void Start()
        {
            Init(); 
        }

        private void Init()
        {
            bool preview = false;
#if UNITY_EDITOR
            preview = DebugManager.Instance.DeadzonesPreview;
#endif
            transform.GetChild(0).gameObject.SetActive(false);

            for(int i=1; i < transform.childCount-1; i++)
            {
                Transform zone = transform.GetChild(i);
                zone.name = "Deadzone" + i.ToString();
                zone.AddComponent<ForceRespawnOnTrigger>();
                zone.GetComponent<MeshRenderer>().enabled = preview;

            }
        }

#if UNITY_EDITOR
        [Button]
        private void ToggleDeadzonePreview(bool enable)
        {
            for(int i=1; i < transform.childCount-1; i++)
                transform.GetChild(i).GetComponent<MeshRenderer>().enabled = enable;
        }
#endif
    }
}

