using BHR;
using System.Collections;
using UnityEngine;

public class AsteroidBreak : MonoBehaviour
{
    [SerializeField] private float m_countdown = 3f;
    [SerializeField] private float m_timerBeforeRespawn = 5f;
    [SerializeField] private float m_checkInterval = 0.1f;

    [SerializeField] private Vector3 m_boxSize = new Vector3(2f, 1f, 2f);
    [SerializeField] private float m_boxCenterHeight = 0.5f;

    [SerializeField] private LayerMask m_playerLayer;

    private Renderer m_renderer;
    private MeshCollider m_collider;
    private float m_initialCountdown;
    private Coroutine m_checkCoroutine;

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

        m_checkCoroutine = StartCoroutine(CheckForPlayerStanding());
    }

    private IEnumerator CheckForPlayerStanding()
    {
        while (true)
        {
            if (m_renderer.enabled)
            {
                Debug.Log("AsteroidBreak: Checking for player standing...");
                Vector3 boxCenter = transform.position + Vector3.up * m_boxCenterHeight;
                Vector3 halfExtents = m_boxSize * 0.5f;

                Collider[] hits = Physics.OverlapBox(boxCenter, halfExtents, Quaternion.identity, m_playerLayer);
                if (hits.Length > 0)
                {
                    Debug.Log("AsteroidBreak: Player detected standing on the asteroid.");
                    m_countdown -= m_checkInterval;
                    if (m_countdown <= 0f)
                    {
                        DisableObject(true);
                    }
                }
                else
                {
                    Debug.Log("AsteroidBreak: No player detected standing on the asteroid.");
                }
            }

            yield return new WaitForSeconds(m_checkInterval);
        }
    }

    private void DisableObject(bool shouldDisable)
    {
        if (shouldDisable)
        {
            m_renderer.enabled = false;
            m_collider.enabled = false;
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

    private void OnRespawned()
    {
        m_countdown = m_initialCountdown;
        DisableObject(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 boxCenter = transform.position + Vector3.up * m_boxCenterHeight;
        Gizmos.DrawWireCube(boxCenter, m_boxSize);
    }
#endif
}
