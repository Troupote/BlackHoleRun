using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBehavior : MonoBehaviour
{
    [SerializeField]
    private SingularityBehavior _singularity;

    private float _timeElasped;
    // Update is called once per frame
    void Update()
    {
        _singularity.FollowPlayer(transform.position);

        _timeElasped += Time.deltaTime;
        Debug.Log($"{_timeElasped}");
        if (_timeElasped >= 3f)
        {
            _singularity.Throw();
            _timeElasped = 0;
        }
    }
}
