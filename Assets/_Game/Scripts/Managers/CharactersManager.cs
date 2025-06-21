using BHR;
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using _Game.Scripts.Music;

public class CharactersManager : ManagerSingleton<CharactersManager>
{
    [SerializeField]
    private GameObject m_characterPrefab;

    [SerializeField]
    private GameObject m_singularityPrefab;

    [SerializeField]
    private CharacterGameplayData m_gameplayData;

    internal CharacterGameplayData GameplayData => m_gameplayData;

    [field: SerializeField]
    internal LimitPlayersMovementsController LimitPlayersMovements { get; private set; } = null;

    [field: SerializeField]
    internal NewSingularityPreviewController SingularityPreviewController { get; private set; } = null;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private CanvasGroup m_fadeCanvasGroup;

    private GameObject m_singularityObject;
    private GameObject m_characterObject;
    public GameObject CharacterObject => m_characterObject;

    private SingularityBehavior m_singularityBehavior;
    private CharacterBehavior m_characterBehavior;

    public Action ResetInputs;

    public bool isHumanoidAiming = false;
    public bool canCharacterJump => m_characterBehavior.CanJump();
    internal bool IsDashing => m_characterBehavior.IsDashing;

    internal bool CanThrow => m_singularityBehavior.IsAllowedToBeThrown;

    private bool _speedLinesInitDone = false;

    public override void Awake()
    {
        SetInstance(false);
    }

    private void Start()
    {
        SpawnCharacterAtPosition(CheckpointsManager.Instance.CurrentCheckpoint.position);
    }

    private bool m_isInstancied = false;
    private void InstanciatePrefabsOnScene()
    {
        m_characterObject = Instantiate(m_characterPrefab);
        m_singularityObject = Instantiate(m_singularityPrefab);

        m_singularityBehavior = m_singularityObject.GetComponent<SingularityBehavior>();
        m_characterBehavior = m_characterObject.GetComponent<CharacterBehavior>();

        CameraManager.Instance.SetDependencies(m_characterObject, m_singularityObject);
        m_characterBehavior.InitializeDependencies(m_gameplayData);
        m_singularityBehavior.InitializeDependencies(m_gameplayData);

        ListenToEvents();

        m_isInstancied = true;
    }

    private void ListenToEvents()
    {
        m_characterBehavior.OnThrowInput += ThrowSingularity;
        m_singularityBehavior.OnThrowPerformed += OnThrowPerformed;
        m_singularityBehavior.OnUnmorph += SwitchCharactersPositions;
        m_singularityBehavior.OnJump += OnSingularityJump;
        m_singularityBehavior.OnDash += SingularityDash;

        GameManager.Instance.OnRespawned.AddListener(HardReset);
        GameManager.Instance.OnPaused.AddListener(CancelAim);
    }

    private void UnlistenToEvents()
    {
        m_singularityBehavior.OnThrowPerformed -= OnThrowPerformed;
        m_characterBehavior.OnThrowInput -= ThrowSingularity;
        m_singularityBehavior.OnUnmorph -= SwitchCharactersPositions;
        m_singularityBehavior.OnJump -= OnSingularityJump;
        m_singularityBehavior.OnDash -= SingularityDash;

        GameManager.Instance.OnRespawned.RemoveListener(HardReset);
    }

    private bool AreObjectsInstancied() => m_isInstancied;

    public void SpawnCharacterAtPosition(Vector3 a_position)
    {
        if (!AreObjectsInstancied())
        {
            InstanciatePrefabsOnScene();
        }

        m_characterObject.transform.position = a_position;
        m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(true);
        StartAmbience();
        StartMusic();
    }

    public void HardReset()
    {
        Debug.Log("Hard Reset Characters Manager");

        ResetAmbience();
        ResetMusic();
        if (BringNewSingularityToNewCharacterCoroutine != null)
        {
            StopCoroutine(BringNewSingularityToNewCharacterCoroutine);
            BringNewSingularityToNewCharacterCoroutine = null;
            m_isSwitching = false;
            m_singularityBehavior.SetIgnoreCollision(false);
        }

        var rbCharacter = m_characterObject.GetComponent<Rigidbody>();
        rbCharacter.linearVelocity = Vector3.zero;
        rbCharacter.angularVelocity = Vector3.zero;

        var rbSingularity = m_singularityObject.GetComponent<Rigidbody>();
        rbSingularity.linearVelocity = Vector3.zero;
        rbSingularity.angularVelocity = Vector3.zero;

        m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(true);

        CameraManager.Instance.SwitchCameraToCharacter(m_characterObject.transform.position);
        GameManager.Instance.ChangeMainPlayerState(PlayerState.HUMANOID, false);

        if (!m_characterObject.activeInHierarchy)
            HideSingularityPreview();

        m_characterBehavior.ImobilizeCharacter(false);
        m_singularityBehavior.HideHorizon(false);
        GameManager.Instance.ResetSpeedLines();

        ResetInputs?.Invoke();

        if (m_gameplayData.ActivateMovementsLimit)
        {
            LimitPlayersMovements.ClearPerformedMovements();
        }
    }


    private float DistanceBetweenPlayersInPercents()
    {
        return (Vector3.Distance(m_characterObject.transform.position, m_singularityObject.transform.position)
            / m_gameplayData.MaxDistanceBetweenPlayers);
    }

    #region Life Cycle

    private void FixedUpdate()
    {
        if (!AreObjectsInstancied()) return;

    }
    
    private void Update()
    {
        if (!AreObjectsInstancied()) return;

        // Gestion du son des pas (footsteps)
        Vector3 currentPosition = m_characterObject.transform.position;
        bool isMoving = (currentPosition - m_lastPosition).sqrMagnitude > 0.0001f && m_characterBehavior.IsGrounded();
        if (isMoving && !m_wasMovingLastFrame)
        {
            PlayFootsteps();
        }
        else if (!isMoving && m_wasMovingLastFrame)
        {
            StopFootsteps();
        }
        m_wasMovingLastFrame = isMoving;
        m_lastPosition = currentPosition;

        if (m_ambienceStarted)
        {
            AudioManager.Instance.Set3DAttributesFromGameObject(m_ambienceInstance, m_characterObject);
            if (!m_isFadingAmbience)
                m_ambienceInstance.setVolume(AudioManager.Instance.SFXVolume);
        }
        if (m_musicStarted)
        {
            m_musicInstance.setVolume(AudioManager.Instance.MusicVolume);
        }

        // Check if the distance between players is exceeded
        if (IsEndingCinematicStarted) return;
        if (DistanceBetweenPlayersInPercents() >= 1f && !m_singularityBehavior.SingularityCharacterFollowComponent.IsPickedUp)
            SwitchCharactersPositions();
        else if (GameManager.Instance.ActivePlayerState == PlayerState.SINGULARITY)
            GameManager.Instance.ApplySpeedLinesSingu(DistanceBetweenPlayersInPercents());
    }

    private void OnDestroy()
    {
        StopAmbience();
        StopMusic();
    }

    #endregion

    #region Switch Characters

    private Coroutine BringNewSingularityToNewCharacterCoroutine;
    float GetCharacterHeight()
    {
        CapsuleCollider collider = m_characterObject.GetComponent<CapsuleCollider>();
        return collider ? collider.height : 2.0f;
    }

    private bool m_isSwitching = false;
    public bool IsCurrentlySwitching => m_isSwitching;
    public void SwitchCharactersPositions()
    {
        if (m_isSwitching) return;

        GameManager.Instance.ResetSpeedLines();

        m_isSwitching = true;

        HideSingularityPreview();

        Vector3 singularityPosition = m_singularityObject.transform.position;
        var oldCharacterPosition = m_characterObject.transform.position;

        RaycastHit hit;
        // To make sure the character does not get stuck underground
        if (Physics.Raycast(singularityPosition + Vector3.up * 0.5f, Vector3.down, out hit, 2.0f, groundLayer))
        {
            singularityPosition.y = hit.point.y + GetCharacterHeight();
        }

        CorrectlySwitchPositionsOfPlayers(singularityPosition, oldCharacterPosition);

        CameraManager.Instance.SwitchCameraToCharacter(m_characterObject.transform.position);
        
        // Correct the filter when returning to character
        SetMusicLowFilterTo1();

        ResetInputs?.Invoke();

        GameManager.Instance.ChangeMainPlayerState(PlayerState.HUMANOID, false);

        m_singularityBehavior.ShaderColorController.SetPlayerColors();

        LimitPlayersMovements.ClearPerformedMovements();

        m_characterBehavior.ImobilizeCharacter(false);

        BringNewSingularityToNewCharacterCoroutine = StartCoroutine(MoveSlowlySingularityToNewCharacter(() =>
        {
            m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(true);
        }));
    }

    private void CorrectlySwitchPositionsOfPlayers(Vector3 a_singularityPosition, Vector3 a_oldCharacterPosition)
    {
        var rbCharacter = m_characterObject.GetComponent<Rigidbody>();
        var rbSingularity = m_singularityObject.GetComponent<Rigidbody>();

        var interpolationCharacter = rbCharacter.interpolation;
        var interpolationSingularity = rbSingularity.interpolation;

        rbCharacter.interpolation = RigidbodyInterpolation.None;
        rbSingularity.interpolation = RigidbodyInterpolation.None;

        rbCharacter.position = a_singularityPosition;
        rbSingularity.position = a_oldCharacterPosition;

        m_characterObject.transform.position = a_singularityPosition;
        m_singularityObject.transform.position = a_oldCharacterPosition;

        rbCharacter.interpolation = interpolationCharacter;
        rbSingularity.interpolation = interpolationSingularity;

        rbSingularity.isKinematic = true;

        m_singularityBehavior.HideHorizon(false);

        SingularityPreviewController.Deflate();
    }
    private IEnumerator MoveSlowlySingularityToNewCharacter(Action onComplete = null)
    {
        m_singularityBehavior.SetIgnoreCollision(true);

        Transform targetTransform = CameraManager.Instance.SingularityPlacementRefTransform;

#if UNITY_EDITOR
        if (DebugManager.Instance.SingularityInstantComeback)
            m_singularityBehavior.transform.position = targetTransform.position;
#endif

        //yield return WaitForCustomSeconds(m_gameplayData.CooldownBeforeThrowAllowed);

        float timer = 0f;

        while (Vector3.Distance(m_singularityObject.transform.position, targetTransform.position) > 1f)
        {
            while (GameManager.Instance.GameTimeScale == 0)
                yield return null;

            float speed = m_gameplayData.ComingBackCurve.Evaluate(timer);

            m_singularityObject.transform.position = Vector3.MoveTowards(
                m_singularityObject.transform.position,
                targetTransform.position,
                speed * Time.deltaTime * GameManager.Instance.GameTimeScale);

            timer += Time.deltaTime * GameManager.Instance.GameTimeScale;
            if (timer > m_gameplayData.ComingBackCurve.keys[m_gameplayData.ComingBackCurve.length - 1].time)
                timer = m_gameplayData.ComingBackCurve.keys[m_gameplayData.ComingBackCurve.length - 1].time;
            yield return null;
        }


        m_singularityObject.transform.position = targetTransform.position;

        onComplete?.Invoke();

        m_singularityBehavior.SetIgnoreCollision(false);

        m_isSwitching = false;
    }

    /// <summary>
    /// Waits for a specified duration in seconds, while checking for game pause state. (To avoid WaitForSeconds which uses Time.timeScale)
    /// </summary>
    /// <param name="a_duration"></param>
    /// <returns></returns>
    private IEnumerator WaitForCustomSeconds(float a_duration)
    {
        float timer = 0f;

        while (timer < a_duration)
        {
            while (GameManager.Instance.GameTimeScale == 0)
                yield return null;

            timer += Time.deltaTime;
            yield return null;
        }
    }



    #endregion

    #region Throw
    public void ThrowSingularity()
    {
        if (!m_singularityBehavior.IsAllowedToBeThrown) return;

        if (m_singularityBehavior.IsOverlapping()) return;

        m_singularityBehavior.HideHorizon(true);
        m_singularityBehavior.OnThrow();
        m_characterBehavior.ImobilizeCharacter(true);
        GameManager.Instance.ChangeMainPlayerState(PlayerState.SINGULARITY, true);
        GameManager.Instance.ChangeSpeedLines(SpeedLinesState.BLACK);
        ShowSingularityPreview();
    }

    private void OnThrowPerformed()
    {
        CameraManager.Instance.SwitchCameraToSingularity();
        SetMusicLowFilterTo0();
    }
    #endregion

    #region Character Aim
    private bool m_hasAlreadyCallAim = false;
    internal bool HasAlreadyCalledAim => m_hasAlreadyCallAim;

    public void HandleAim(bool withThrow)
    {
        m_hasAlreadyCallAim = !m_hasAlreadyCallAim;

        if (m_characterBehavior.IsJumping || m_characterBehavior.IsDashing) return;

        if (m_hasAlreadyCallAim && CanThrow)
        {
            StartAim(withThrow);
        }
        else if (!m_hasAlreadyCallAim)
        {
            CancelAim();
        }

    }

    public void StartAim(bool a_withThrow)
    {
        float duration = m_gameplayData.TriggerAimDuration;
        StartCoroutine(GameManager.Instance.SlowmotionSequence(duration, duration * (a_withThrow ? 1f : 1f)));
        GameManager.Instance.isSlowMotionSequenceStarted = true;
        isHumanoidAiming = true;
    }

    public void CancelAim()
    {
        m_hasAlreadyCallAim = false;
        GameManager.Instance.isSlowMotionSequenceStarted = false;
        GameManager.Instance.isTimeSlowed = false;

        isHumanoidAiming = false;
    }

    #endregion

    #region Singularity Jump

    private void OnSingularityJump(Vector3 a_linearVelocityToApply)
    {
        SwitchCharactersPositions();
        m_characterBehavior.OnSingularityJump(a_linearVelocityToApply);
    }

    #endregion

    #region Singularity Dash

    public void SingularityDash(Vector3 a_linearVelocityToApply, Vector3 a_direction)
    {
        // Jouer le son du dash avec notre méthode dédiée
        PlayDashSound();
        GameManager.Instance.ChangeSpeedLines(SpeedLinesState.DASH);

        SwitchCharactersPositions();
        m_characterBehavior.OnSingularityDash(a_linearVelocityToApply, a_direction);
    }

    #endregion

    #region Manage Singularity Preview

    private void ShowSingularityPreview()
    {
        SingularityPreviewController.Inflate(m_characterObject.transform);
        //Invoke("DisableCharacterObject", 0.05f);
        m_characterObject.SetActive(false);
    }

    private void DisableCharacterObject() => m_characterObject.SetActive(false);

    private void HideSingularityPreview()
    {
        SingularityPreviewController.Deflate();
        m_characterObject.SetActive(true);
    }

    #endregion

    #region Audio
    #region Ambiant
    private EventInstance m_ambienceInstance;
    private bool m_ambienceStarted = false;
    private bool m_isFadingAmbience = false;

    public void StartAmbience()
    {
        Debug.Log("Starting ambience");
        if (m_ambienceStarted) return;
        m_ambienceInstance = AudioManager.Instance.CreateEventInstance(FmodEventsCreator.instance.windAmbient);
        m_ambienceInstance.start();
        m_ambienceStarted = true;
    }

    public void StopAmbience()
    {
        if (!m_ambienceStarted) return;
        m_ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        m_ambienceInstance.release();
        m_ambienceStarted = false;
    }

    public void ResetAmbience()
    {
        if (!m_ambienceStarted) return;
        float strtVolume;
        m_ambienceInstance.getVolume(out strtVolume);
        float fadeDuration = 0.2f;
        float minVolume = strtVolume * 0.2f;

        m_isFadingAmbience = true;
        DOTween.To(
            () => strtVolume,
            v => {
                m_ambienceInstance.setVolume(v);
            },
            minVolume,
            fadeDuration
        ).OnComplete(() => {
            DOTween.To(
                () => minVolume,
                v => {
                    m_ambienceInstance.setVolume(v);
                },
                strtVolume,
                fadeDuration
            ).OnComplete(() => {
                m_isFadingAmbience = false;
            });
        });
    }
    #endregion

    #region SFX
    private EventInstance m_footstepsInstance;
    private bool m_isFootstepsPlaying = false;
    private Vector3 m_lastPosition;
    private bool m_wasMovingLastFrame = false;

    public void PlayFootsteps()
    {
        if (m_isFootstepsPlaying) return;
        m_footstepsInstance = AudioManager.Instance.CreateEventInstance(FmodEventsCreator.instance.playerFootsetps);
        m_footstepsInstance.setVolume(AudioManager.Instance.SFXVolume);
        m_footstepsInstance.start();
        m_isFootstepsPlaying = true;
    }

    public void PlayDashSound()
    {
        EventInstance dashInstance = AudioManager.Instance.CreateEventInstance(FmodEventsCreator.instance.dash);
        dashInstance.setVolume(AudioManager.Instance.SFXVolume);
        dashInstance.start();
        dashInstance.release();
    }

    public void StopFootsteps()
    {
        if (!m_isFootstepsPlaying) return;
        m_footstepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        m_footstepsInstance.release();
        m_isFootstepsPlaying = false;
    }
    #endregion

    #region Music
    private EventInstance m_musicInstance;
    private bool m_musicStarted = false;

    public void StartMusic()
    {
        if (m_musicStarted) return;
        m_musicInstance = AudioManager.Instance.CreateEventInstance(FmodEventsCreator.instance.musicBeatRigolo);
        m_musicInstance.setVolume(AudioManager.Instance.MusicVolume);
        m_musicInstance.start();
        m_musicStarted = true;
    }

    public void StopMusic()
    {
        if (!m_musicStarted) return;
        m_musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        m_musicInstance.release();
        m_musicStarted = false;
    }

    public void ResetMusic()
    {
        if (!m_musicStarted) return;
        float fadeDuration = 0.5f;
        float minValue = 0f;
        float maxValue = 1f;

        DOTween.To(
            () => maxValue,
            v => m_musicInstance.setParameterByName("MusicLowFilter", v),
            minValue,
            fadeDuration +0.2f
        ).OnComplete(() => {
            DOTween.To(
                () => minValue,
                v => m_musicInstance.setParameterByName("MusicLowFilter", v),
                maxValue,
                fadeDuration
            );
        });
    }

    public void SetMusicLowFilterTo0(float duration = 0.2f)
    {
        if (m_musicStarted)
        {
            float currentValue;
            m_musicInstance.getParameterByName("MusicLowFilter", out currentValue);
            DG.Tweening.DOTween.To(
                () => currentValue,
                v => m_musicInstance.setParameterByName("MusicLowFilter", v),
                0f,
                duration
            );
        }
        if (m_ambienceStarted)
        {
            float currentValue;
            m_ambienceInstance.getParameterByName("MusicLowFilter", out currentValue);
            DG.Tweening.DOTween.To(
                () => currentValue,
                v => m_ambienceInstance.setParameterByName("MusicLowFilter", v),
                0f,
                duration
            );
        }
    }

    public void SlowMusic(float duration = 0.2f)
    {
        float currentValue2;
        m_musicInstance.getParameterByName("MusicSpeed", out currentValue2);
        DG.Tweening.DOTween.To(
            () => currentValue2,
            v => m_musicInstance.setParameterByName("MusicSpeed", v),
            0f,
            duration
        );
    }

    public void SetMusicLowFilterTo1(float duration = 0.2f)
    {
        if (m_musicStarted)
        {
            float currentValue;
            m_musicInstance.getParameterByName("MusicLowFilter", out currentValue);
            DG.Tweening.DOTween.To(
                () => currentValue,
                v => m_musicInstance.setParameterByName("MusicLowFilter", v),
                1f,
                duration
            );
        }
        if (m_ambienceStarted)
        {
            float currentValue;
            m_ambienceInstance.getParameterByName("MusicLowFilter", out currentValue);
            DG.Tweening.DOTween.To(
                () => currentValue,
                v => m_ambienceInstance.setParameterByName("MusicLowFilter", v),
                1f,
                duration
            );
        }
    }

    public void SpeedUpMusic(float duration = 0.2f)
    {
        float currentValue2;
        m_musicInstance.getParameterByName("MusicSpeed", out currentValue2);
        DG.Tweening.DOTween.To(
            () => currentValue2,
            v => m_musicInstance.setParameterByName("MusicSpeed", v),
            1f,
            duration
        );
    }

    #endregion
    #endregion

    #region End Cinematic

    internal bool IsEndingCinematicStarted { get; set; } = false;

    public void ManageEnding(Vector3 a_targetLook)
    {
        IsEndingCinematicStarted = true;
        DisableCharacterAndSingularityControllerScripts();
        MakeSureSingularityIsPickedUp();
        MoveSingularityToDestination(a_targetLook);
    }

    public void DisableCharacterAndSingularityControllerScripts()
    {
        m_singularityObject.GetComponent<SingularityController>().enabled = false;
        m_characterObject.GetComponent<PlayerController>().enabled = false;
    }

    public void DisableCharacterAndSingularityBehaviorScripts()
    {
        m_singularityBehavior.enabled = false;
        m_characterBehavior.enabled = false;
    }

    private IEnumerator RotateCharacterCoroutine(Vector3 a_targetLook, Action onComplete = null)
    {
        Vector3 direction = (a_targetLook - CameraManager.Instance.PlayerCam.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        float rotationSpeed = 5f;
        float angleThreshold = 1f;

        while (Quaternion.Angle(CameraManager.Instance.PlayerCam.transform.rotation, targetRotation) > angleThreshold)
        {
            while (GameManager.Instance.GameTimeScale == 0)
                yield return null;

            CameraManager.Instance.PlayerCam.transform.rotation = Quaternion.Slerp(
                CameraManager.Instance.PlayerCam.transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed * GameManager.Instance.GameTimeScale
            );

            yield return null;
        }

        CameraManager.Instance.PlayerCam.transform.rotation = targetRotation;
        onComplete?.Invoke();
    }

    public void MakeSureSingularityIsPickedUp()
    {
        if (m_singularityBehavior.SingularityCharacterFollowComponent.IsPickedUp) return;

        m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(true);
    }

    public void MoveSingularityToDestination(Vector3 a_destination, Action onComplete = null)
    {
        DisableCharacterAndSingularityBehaviorScripts();
        StartCoroutine(RotateCharacterCoroutine(a_destination, () =>
        {
            Debug.Log("Character rotated to target look direction.");

            m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(false);
            m_singularityBehavior.SingularityCharacterFollowComponent.SetKinematicState(true);

            StartCoroutine(MoveSingularityToDestinationCoroutine(a_destination, () =>
            {
                StartCoroutine(RescaleSingularity(() =>
                {
                    StartCoroutine(FadeToBlack(4f));
                }));

            }));
        }));
    }
    private IEnumerator MoveSingularityToDestinationCoroutine(Vector3 a_destination, Action onComplete = null)
    {
        float timer = 0f;
        Vector3 startPosition = m_singularityObject.transform.position;
        while (Vector3.Distance(m_singularityObject.transform.position, a_destination) > 0.1f)
        {
            while (GameManager.Instance.GameTimeScale == 0)
                yield return null;
            float speed = 100f;
            m_singularityObject.transform.position = Vector3.MoveTowards(
                m_singularityObject.transform.position,
                a_destination,
                speed * Time.deltaTime * GameManager.Instance.GameTimeScale);
            timer += Time.deltaTime * GameManager.Instance.GameTimeScale;
            yield return null;
        }
        m_singularityObject.transform.position = a_destination;
        onComplete?.Invoke();
    }

    private Vector3 m_singularityDefaultScale;
    private IEnumerator RescaleSingularity(Action onComplete = null)
    {
        float timer = 0f;
        float duration = 5f;
        Vector3 startScale = m_singularityObject.transform.localScale;
        Vector3 targetScale = Vector3.one * 400f;

        Vector3 singularityPos = m_singularityObject.transform.position;

        Transform planet1 = PlanetsCollidingManager.Instance.Spawner.PlanetPlacement1;
        Transform planet2 = PlanetsCollidingManager.Instance.Spawner.PlanetPlacement2;

        Vector3 planet1StartPos = planet1.position;
        Vector3 planet2StartPos = planet2.position;

        Vector3 planet1StartScale = planet1.localScale;
        Vector3 planet2StartScale = planet2.localScale;

        Vector3 planetTargetScale = Vector3.zero;

        CameraManager.Instance.ShakeCamera(3f, 5f);

        m_singularityDefaultScale = startScale;

        PlanetsCollidingManager.Instance.Spawner.DisableSphereColliders();

        while (timer < duration)
        {
            while (GameManager.Instance.GameTimeScale == 0)
                yield return null;

            float t = Mathf.Clamp01(timer / duration);

            m_singularityObject.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            planet1.position = Vector3.Lerp(planet1StartPos, singularityPos, t);
            planet2.position = Vector3.Lerp(planet2StartPos, singularityPos, t);

            planet1.localScale = Vector3.Lerp(planet1StartScale, planetTargetScale, t);
            planet2.localScale = Vector3.Lerp(planet2StartScale, planetTargetScale, t);

            timer += Time.deltaTime * GameManager.Instance.GameTimeScale;
            yield return null;
        }

        m_singularityObject.transform.localScale = targetScale;
        planet1.position = singularityPos;
        planet2.position = singularityPos;

        planet1.localScale = planetTargetScale;
        planet2.localScale = planetTargetScale;

        onComplete?.Invoke();
    }

    public IEnumerator FadeToBlack(float duration)
    {
        float timer = 0f;
        while (timer < duration / 2)
        {
            m_fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }
        m_fadeCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(2f);

        GameManager.Instance.EndLevel();

        while (timer < duration)
        {
            m_fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, (timer - duration / 2) / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }
        m_fadeCanvasGroup.alpha = 0f;
    }

    #endregion
}


