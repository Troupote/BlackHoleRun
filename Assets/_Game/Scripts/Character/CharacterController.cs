using BHR;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterBehavior m_characterBehavior;
    private Vector2 m_moveValue = Vector2.zero;
    private bool m_isInitialized = false;
    private int aimCallCount = 0;

    void Start()
    {
        InitializeDependencies();
    }

    private void InitializeDependencies()
    {
        m_characterBehavior = GetComponent<CharacterBehavior>();

        m_isInitialized = true;
    }

private void OnEnable()
    {
        // Bind inputs 
        PlayersInputManager.Instance.OnHMove.AddListener(HandleMove);
        PlayersInputManager.Instance.OnHJump.AddListener(HandleJump);
        PlayersInputManager.Instance.OnHDash.AddListener(HandleDash);
        PlayersInputManager.Instance.OnHThrow.AddListener(HandleThrowSingularity);
        PlayersInputManager.Instance.OnHAim.AddListener(HandleAim);
        CharactersManager.Instance.ResetInputs += ResetInputs;
        GameManager.Instance.OnPaused.AddListener(ResetInputs);
        GameManager.Instance.OnRespawn.AddListener(ResetInputs);
        //PlayersInputManager.Instance.OnHSlide.AddListener();

    }

    private void OnDisable()
    {
        // Debind inputs
        PlayersInputManager.Instance.OnHMove.RemoveListener(HandleMove);
        PlayersInputManager.Instance.OnHJump.RemoveListener(HandleJump);
        PlayersInputManager.Instance.OnHDash.RemoveListener(HandleDash);
        PlayersInputManager.Instance.OnHThrow.RemoveListener(HandleThrowSingularity);
        PlayersInputManager.Instance.OnHAim.RemoveListener(HandleAim);
        CharactersManager.Instance.ResetInputs -= ResetInputs;
        GameManager.Instance.OnPaused.RemoveListener(ResetInputs);
        GameManager.Instance.OnRespawn.RemoveListener(ResetInputs);
        //PlayersInputManager.Instance.OnHSlide.RemoveListener();
    }

    #region Life Cycle

    private void FixedUpdate()
    {
        if (!m_isInitialized) return;

        m_characterBehavior.Move(m_moveValue);
    }

    #endregion

    private void ResetInputs()
    {
        m_moveValue = Vector2.zero;
    }
    private void HandleMove(Vector2 a_movementValue)
    {
        m_moveValue = a_movementValue;
    }

    private void HandleJump()
    {
        m_characterBehavior.OnJump();
    }

    private void HandleDash()
    {
        m_characterBehavior.OnDash();
    }

    private void HandleThrowSingularity()
    {
        if (!GameManager.Instance.isFinished)
        {
            GameManager.Instance.isSlowed = false;
        }

        GameManager.Instance.isStarted = false;
        GameManager.Instance.isSlowed = false;

        aimCallCount = 0;
        m_characterBehavior.OnThrowSingularity();
    }

    private void HandleAim()
    {
        if (aimCallCount == 0)
        {
            aimCallCount++;

            StartCoroutine(GameManager.Instance.SlowmotionSequence());
            GameManager.Instance.isStarted = true;
            CharactersManager.Instance.isHumanoidAiming = true;
        }
        else if (aimCallCount == 1)
        {
            GameManager.Instance.isStarted = false;
            GameManager.Instance.isSlowed = false;

            aimCallCount = 0;
            CharactersManager.Instance.isHumanoidAiming = false;
        }
    }
}
