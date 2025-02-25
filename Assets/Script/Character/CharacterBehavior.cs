using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBehavior : MonoBehaviour
{
    [SerializeField]
    private SingularityBehavior _singularity;

    private float _timeElasped;

    public void SetDependencies(SingularityBehavior a_singularity)
    {
        _singularity = a_singularity;
    }

    // Update is called once per frame
    void Update()
    {
        _timeElasped += Time.deltaTime;
        
        if (_timeElasped >= 3f)
        {
            _singularity.Throw();
            _timeElasped = 0;
        }
        
    }
}
