using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR
{
    public class CheckpointsManager : ManagerSingleton<CheckpointsManager>
    {
        [SerializeField, LabelText("Start Checkpoint")]
        Transform _currentCheckpoint;
        public Transform CurrentCheckpoint => _currentCheckpoint;
        public int CurrentPhaseId => int.Parse(_currentCheckpoint.name.Split('-')[1]);
        public int CurrentPhaseCheckpointId => int.Parse(_currentCheckpoint.name.Split('-')[2]);

#if UNITY_EDITOR
        [Button]
        private void SetCheckpointAndRespawn(Transform checkpoint)
        {
            SetCheckpoint(checkpoint);
            RespawnPlayer();
        }
#endif

        public override void Awake()
        {
            SetInstance(false);
        }

        private void Start()
        {
            SetCheckpoint(_currentCheckpoint);
        }

        public void SetCheckpoint(Transform checkpoint, bool force = false)
        {
            int phaseId = int.Parse(checkpoint.name.Split('-')[1]);
            int phaseCheckpointId = int.Parse(checkpoint.name.Split('-')[2]);

            if ((phaseId < CurrentPhaseId || phaseCheckpointId < CurrentPhaseCheckpointId) && !force)
                return; // older checkpoint, bypass (?)

            _currentCheckpoint = checkpoint;

#if UNITY_EDITOR
            UpdateDebugText();
#endif
        }

        public void RespawnPlayer()
        {
            CharactersManager.Instance.CharacterObject.transform.position = _currentCheckpoint.position;
            CharactersManager.Instance.CharacterObject.transform.rotation = _currentCheckpoint.rotation;

            // @todo transition, cooldown, "pause" state etc... many things
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.C) && DebugManager.Instance.DebugKeyboardShortcutsEnabled)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    RespawnPlayer();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                    DebugSwitchCheckpoint(1);
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    DebugSwitchCheckpoint(-1);

            }
        }

        private void UpdateDebugText()
        {
            if (DebugManager.Instance.CheckpointInfosTextEnabled)
            {
                DebugManager.Instance.CheckpointHUDInfosText.text = $"Phase: {CheckpointsManager.Instance?.CurrentPhaseId.ToString("D2")} - Checkpoint: {CheckpointsManager.Instance?.CurrentPhaseCheckpointId.ToString("D2")}";
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
            RespawnPlayer();
        }
#endif
    }
}
