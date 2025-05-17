using BHR;
using Sirenix.OdinInspector;
using UnityEngine;

public class SetCheckpoint : MonoBehaviour
{
    [SerializeField, Tooltip("Find checkpoint by its reference"), HideIf(nameof(_checkpointName),null), ValidateInput("ValidateCheckpoint", "Enter a correct checkpoint, child of the manager")]
    private Transform _checkpointTransform = null;
    [SerializeField, Tooltip("Find checkpoint by its name"), HideIf(nameof(_checkpointTransform), null), ValidateInput("ValidateCheckpoint", "Enter a correct checkpoint, child of the manager")]
    private string _checkpointName = null;

    private Transform _targetCheckpoint;

#if UNITY_EDITOR
    private bool ValidateCheckpoint()
    {
        string checkpointParentName = "";
        if (_checkpointTransform != null)
            checkpointParentName = _checkpointTransform.parent.name;

        else if (_checkpointName != null)
            checkpointParentName = GameObject.Find(_checkpointName).transform.parent.name;

        return checkpointParentName == "CheckpointsManager";
    }
#endif

    private void Start()
    {
        _targetCheckpoint = _checkpointTransform;
        if (_targetCheckpoint == null) _targetCheckpoint = CheckpointsManager.Instance.transform.Find(_checkpointName);
        if (_targetCheckpoint == null)
        {
            Debug.LogError("Checkpoint reference missing ! Are you sure " + _checkpointTransform == null ? "" : $"{_checkpointTransform.name}" + _checkpointName + " is a checkpoint and child of the manager ?");
            return;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(CheckpointsManager.Instance.CurrentCheckpoint != _targetCheckpoint && other.CompareTag("Player"))
            CheckpointsManager.Instance.SetCheckpoint(_targetCheckpoint);
    }
}
