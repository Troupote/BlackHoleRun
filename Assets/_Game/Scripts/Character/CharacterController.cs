using BHR;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private TimeControl m_timeController;
    private CharacterBehavior m_characterBehavior;
    private Vector2 m_moveValue = Vector2.zero;
    private bool m_isInitialized = false;

    void Start()
    {
        InitializeDependencies();
    }

    private void InitializeDependencies()
    {
        m_characterBehavior = GetComponent<CharacterBehavior>();
        m_timeController = GameManager.Instance.gameObject.GetComponent<TimeControl>();

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
        //PlayersInputManager.Instance.OnHSlide.RemoveListener();
    }

    #region Life Cycle

    private void FixedUpdate()
    {
        if (!m_isInitialized) return;

        m_characterBehavior.Move(m_moveValue);
    }

    #endregion

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
        m_moveValue = Vector2.zero;
        m_characterBehavior.OnThrowSingularity();
    }

    private void HandleAim()
    {

    }
}
