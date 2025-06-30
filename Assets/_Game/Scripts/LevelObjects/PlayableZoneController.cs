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
            GameManager.Instance.OnDeadzonePreviewToggled.AddListener(TogglePlayableZonePreview);
        }

        private void Init()
        {
            bool preview = false;
#if UNITY_EDITOR
            preview = DebugManager.Instance.DeadzonesPreview;
#endif
            for(int i=2; i < transform.childCount-1; i++)
            {
                Transform zone = transform.GetChild(i);
                zone.name = "Deadzone" + i.ToString();
                zone.AddComponent<ForceRespawnOnTrigger>();
                zone.GetComponent<MeshRenderer>().enabled = preview;

            }

            TogglePlayableZonePreview(SettingsSave.LoadDeadzonePreview()==1);
        }

        [Button]
        private void TogglePlayableZonePreview(bool enable)
        {
            transform.GetChild(0).gameObject.SetActive(enable);
            transform.GetChild(1).gameObject.SetActive(enable);
        }

#if UNITY_EDITOR
        [Button]
        private void ToggleDeadzonePreview(bool enable)
        {
            for(int i=2; i < transform.childCount-1; i++)
                transform.GetChild(i).GetComponent<MeshRenderer>().enabled = enable;
        }
#endif
    }
}

