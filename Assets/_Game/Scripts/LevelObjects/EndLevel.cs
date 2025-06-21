using BHR;
using UnityEngine;

public class EndLevel : MonoBehaviour
{
    private bool _hasEnded;
    private void OnTriggerEnter(Collider other)
    {
        if(!_hasEnded && other.CompareTag("Player"))
        {
            _hasEnded = true;
            GameManager.Instance.EndLevel();
        }
    }

    private void OnEnable()
    {
        _hasEnded = false;
    }
}
