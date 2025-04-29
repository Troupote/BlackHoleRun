using BHR;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class SingularityController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    private Vector2 moveValue;

    private void OnEnable()
    {
        // Bind inputs
        PlayersInputManager.Instance.OnSMove.AddListener(HandleSingulairtyMove);
        // Bind toout le reste la

    }

    private void OnDisable()
    {
        // Debind inputs
        PlayersInputManager.Instance.OnSMove.RemoveListener(HandleSingulairtyMove);
        // Debing le reste
    }

    void Update()
    {
        if (!CharactersManager.Instance.isSingularityThrown) return;

        Move();
    }

    [SerializeField] private float curveForce = 0.001f;

    private void Move()
    {
        float moveX = moveValue.x;

        if (moveX != 0)
        {
            Vector3 curveDirection = transform.right * moveX;
            rb.AddForce(curveDirection * curveForce, ForceMode.Force);
        }
    }

    public void HandleSingulairtyMove(Vector2 value) => moveValue = value;
}
