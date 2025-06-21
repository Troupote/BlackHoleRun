using BHR;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class AsteroidBreak : MonoBehaviour
{
    [SerializeField] private float m_countdown = 3f;
    [SerializeField] private bool m_respawnAfterTime;
    [SerializeField, ShowIf(nameof(m_respawnAfterTime))] private float m_timerBeforeRespawn = 5f;
    [SerializeField] private GameObject _particleEffects;

    //[SerializeField] private Vector3 m_boxSize = new Vector3(2f, 1f, 2f);
    //[SerializeField] private float m_boxCenterHeight = 0.5f;

    [SerializeField] private string m_tag = "Player";

    private Renderer m_renderer;
    private MeshCollider m_collider;
    private float m_initialCountdown;
    private bool m_startTimer = false;

    private void Start()
    {
        m_initialCountdown = m_countdown;

        m_renderer = GetComponent<Renderer>();
        if (m_renderer == null)
        {
            Debug.LogError("AsteroidBreak: Renderer component not found.");
        }

        m_collider = GetComponent<MeshCollider>();
        if (m_collider == null)
        {
            Debug.LogError("AsteroidBreak: MeshCollider component not found.");
        }

        GameManager.Instance.OnRespawned.AddListener(OnRespawned);
    }

    //private IEnumerator CheckForPlayerStanding()
    //{
    //    while (true)
    //    {
    //        if (m_renderer.enabled)
    //        {
    //            Debug.Log("AsteroidBreak: Checking for player standing...");
    //            Vector3 boxCenter = transform.position + Vector3.up * m_boxCenterHeight;
    //            Vector3 halfExtents = m_boxSize * 0.5f;

    //            Collider[] hits = Physics.OverlapBox(boxCenter, halfExtents, Quaternion.identity, m_playerLayer);
    //            if (hits.Length > 0)
    //            {
    //                Debug.Log("AsteroidBreak: Player detected standing on the asteroid.");
    //                m_countdown -= m_checkInterval;
    //                if (m_countdown <= 0f)
    //                {
    //                    DisableObject(true);
    //                }
    //            }
    //            else
    //            {
    //                Debug.Log("AsteroidBreak: No player detected standing on the asteroid.");
    //            }
    //        }

    //        yield return new WaitForSeconds(m_checkInterval);
    //    }
    //}

    public void DisableObject(bool shouldDisable, bool withParticles = false)
    {
        if (shouldDisable)
        {
            if(withParticles && _particleEffects != null)
            {
                var particles = Instantiate(_particleEffects, transform);
                StartCoroutine(DestroyObject(particles.gameObject, particles.GetComponent<ParticleSystem>().main.duration));
            }    
            m_renderer.enabled = false;
            m_collider.enabled = false;
            if(m_respawnAfterTime)
                StartCoroutine(RespawnAfterHavingBeenDisabled());
        }
        else
        {
            m_renderer.enabled = true;
            m_collider.enabled = true;
        }
    }

    private IEnumerator RespawnAfterHavingBeenDisabled()
    {
        yield return new WaitForSeconds(m_timerBeforeRespawn);
        OnRespawned();
    }

    private IEnumerator DestroyObject(GameObject go, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(go);
    }

    private void OnRespawned()
    {
        m_countdown = m_initialCountdown;
        m_startTimer = false;
        DisableObject(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(m_tag) && CharactersManager.Instance.canCharacterJump)
            m_startTimer = true;
    }

    private void Update()
    {
        if(m_startTimer)
        {
            m_countdown -= Time.deltaTime * GameManager.Instance.GameTimeScale;
            if(m_countdown <= 0)
            {
                DisableObject(true);
                m_startTimer= false;
            }
        }
    }
}
