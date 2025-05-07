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
        PlayersInputManager.Instance.OnSUnmorph.AddListener(HandleUnMorphEarly);
        // Bind toout le reste la

    }

    private void OnDisable()
    {
        // Debind inputs
        PlayersInputManager.Instance.OnSMove.RemoveListener(HandleSingulairtyMove);
        PlayersInputManager.Instance.OnSUnmorph.RemoveListener(HandleUnMorphEarly);
        // Debing le reste
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying || !CharactersManager.Instance.isSingularityThrown) return;
        Move();
    }

    private void Move()
    {
        float moveX = moveValue.x;

        if (moveValue.x == 0 && moveValue.y == 0) return;

        Vector3 curveDirection = transform.right * moveX;

        rb.AddForce(curveDirection.normalized * CharactersManager.Instance.GameplayData.MovingCurveForce, ForceMode.Force);
    }

    public void HandleSingulairtyMove(Vector2 value) => moveValue = value;

    private void HandleUnMorphEarly()
    {
        CharactersManager.Instance.TryThrowSingularity();
    }
}
