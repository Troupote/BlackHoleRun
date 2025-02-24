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
        
        if (_timeElasped >= 3f)
        {
            Debug.Log("Throwing the ball");
            _singularity.Throw();
            _timeElasped = 0;
        }
        
    }
}
