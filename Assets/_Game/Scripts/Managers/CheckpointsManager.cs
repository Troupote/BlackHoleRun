using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR
{
    public class CheckpointsManager : ManagerSingleton<CheckpointsManager>
    {
        [SerializeField, LabelText("Start Checkpoint")]
        Transform _currentCheckpoint;
        public Transform CurrentCheckpoint => _currentCheckpoint;

#if UNITY_EDITOR
        [Button]
        private void SetCheckpointAndRespawn(Transform checkpoint)
        {
            SetCheckpoint(checkpoint);
            ReplacePlayer();
        }
#endif

        public override void Awake()
        {
            SetInstance(false);
        }

        private void Start()
        {
            GameManager.Instance.OnRespawned.AddListener(ReplacePlayer);
            SetCheckpoint(_currentCheckpoint);
        }

        public void SetCheckpoint(Transform checkpoint, bool force = false)
        {
            if (checkpoint.GetSiblingIndex() <= _currentCheckpoint.GetSiblingIndex() && !force)
                return;

            _currentCheckpoint = checkpoint;
        }

        public void ReplacePlayer()
        {
            CharactersManager.Instance.CharacterObject.transform.position = _currentCheckpoint.position;
            CameraManager.Instance.ForceCameraLookAt(_currentCheckpoint.forward);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.C) && DebugManager.Instance.DebugKeyboardShortcutsEnabled)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    DebugSwitchCheckpoint(1);
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    DebugSwitchCheckpoint(-1);

            }
        }

        private void DebugSwitchCheckpoint(int move)
        {
            int checkpointId = -1;
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name == CurrentCheckpoint.name)
                    checkpointId = i;

            int newCheckpointId = checkpointId + move;
            if (newCheckpointId >= transform.childCount)
                newCheckpointId = 0;
            else if(newCheckpointId < 0)
                newCheckpointId = transform.childCount - 1;

            SetCheckpoint(transform.GetChild(newCheckpointId), true);
            ReplacePlayer();
        }
#endif
    }
}
