using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class SingularityController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    private void Start()
    {
    }
    void Update()
    {
        if (!CharactersManager.Instance.isSingularityThrown) return;

        Move();
    }

    [SerializeField] private float curveForce = 0.001f;

    private void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");

        if (moveX != 0)
        {
            Vector3 curveDirection = transform.right * moveX;
            rb.AddForce(curveDirection * curveForce, ForceMode.Force);
        }
    }

}
