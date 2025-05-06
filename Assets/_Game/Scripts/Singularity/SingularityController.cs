using BHR;
using System.Runtime.CompilerServices;
using UnityEditor.ShaderKeywordFilter;
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
        PlayersInputManager.Instance.OnSJump.AddListener(Jump);
        // Bind toout le reste la

    }

    private void OnDisable()
    {
        // Debind inputs
        PlayersInputManager.Instance.OnSMove.RemoveListener(HandleSingulairtyMove);
        PlayersInputManager.Instance.OnSUnmorph.RemoveListener(HandleUnMorphEarly);
        PlayersInputManager.Instance.OnSJump.RemoveListener(Jump);
        // Debing le reste
    }

    void FixedUpdate()
    {
        if (!CharactersManager.Instance.isSingularityThrown) return;
        Move();
        CheckJumpApex();
    }

    private void Move()
    {
        float moveX = moveValue.x;

        if (moveValue.x == 0 && moveValue.y == 0) return;

        Vector3 curveDirection = transform.right * moveX;

        rb.AddForce(curveDirection.normalized * CharactersManager.Instance.GameplayData.MovingCurveForce, ForceMode.Force);
    }

    [SerializeField] private float jumpForce = 15f;
    private bool isWaitingForJumpApex = false;
    private bool hasTriggeredApex = false;

    public void Jump()
    {
        if (isPaused) return;

        Vector3 currentVelocity = rb.linearVelocity;

        currentVelocity.x *= 0.4f;
        currentVelocity.z *= 0.4f;

        rb.linearVelocity = currentVelocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        isWaitingForJumpApex = true;
        hasTriggeredApex = false;
    }

    private void CheckJumpApex()
    {
        if (!isWaitingForJumpApex || hasTriggeredApex) return;

        if (rb.linearVelocity.y <= 5f)
        {
            CharactersManager.Instance.ChangePlayersTurn();
            hasTriggeredApex = true;
            isWaitingForJumpApex = false;
        }
    }


    private void Update()
    {
        HandlePauseState(GameManager.Instance.GameTimeScale == 0);
    }

    private Vector3 cachedVelocity;
    private Vector3 cachedAngularVelocity;
    private bool isPaused;

    public void HandlePauseState(bool paused)
    {
        if (paused && !isPaused)
        {
            // Pause
            cachedVelocity = rb.linearVelocity;
            cachedAngularVelocity = rb.angularVelocity;
            rb.isKinematic = true;
            isPaused = true;
        }
        else if (!paused && isPaused)
        {
            // Resume
            rb.isKinematic = false;
            rb.linearVelocity = cachedVelocity;
            rb.angularVelocity = cachedAngularVelocity;
            isPaused = false;
        }
    }

    public void HandleSingulairtyMove(Vector2 value) => moveValue = value;

    private void HandleUnMorphEarly()
    {
        CharactersManager.Instance.TryThrowSingularity();
    }
}
