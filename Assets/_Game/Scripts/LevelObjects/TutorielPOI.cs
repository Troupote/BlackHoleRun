using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR
{
    public class TutorielPOI : MonoBehaviour
    {
        [SerializeField, Required, ValidateInput(nameof(_hasATutoriel), "Your action ref doesn't have a tutoriel associated with it")]
        private InputActionReference _actionRef;

#if UNITY_EDITOR
        [SerializeField, Tooltip("Enter tutorials data to quickly check if your action ref has one"), ShowIf(nameof(_actionRef), null), LabelText("Quick check")]
        private TutorielDatasSO _tutorielDatas;
#endif

        #region Odin stuff
        private bool _hasATutoriel => _tutorielDatas == null || _tutorielDatas.ActionRefTutoriels.ContainsKey(_actionRef);

        #endregion

        private void OnTriggerEnter(Collider other)
        {
            // @todo remove tuto if level completed ? -> make a settings option ?
            GameManager.Instance.LoadTutorielData(_actionRef);
        }
    }

}
