using BHR;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlanetSpawningController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_planetsObject;
    [SerializeField]
    private GameObject m_shockwavePrefab;
    [SerializeField]
    private ScreenFlash m_screenFlash;
    [SerializeField]
    private List<Renderer> m_planetsRenderer;

    [SerializeField]
    private Transform m_planetPlacement1;
    [SerializeField]
    private Transform m_planetPlacement2;

    private Vector3 m_startPos1;
    private Vector3 m_startPos2;
    private Coroutine m_movementCoroutine;

    private SphereCollider m_sphereCollider1;
    private SphereCollider m_sphereCollider2;

    private List<Material> m_planetMaterials = new List<Material>();

    public Transform PlanetPlacement1 => m_planetPlacement1;
    public Transform PlanetPlacement2 => m_planetPlacement2;

    private void Start()
    {
        PlanetsCollidingManager.Instance.SetSpawner(this);

        m_sphereCollider1 = m_planetPlacement1.GetComponentInChildren<SphereCollider>();
        m_sphereCollider2 = m_planetPlacement2.GetComponentInChildren<SphereCollider>();

        if (m_planetsRenderer != null)
        {
            foreach (var renderer in m_planetsRenderer)
            {
                if (renderer != null)
                {
                    Material mat = renderer.material;
                    if (mat != null)
                    {
                        mat.SetFloat("_DissolveIntensity", 0f);
                        m_planetMaterials.Add(mat);
                    }
                }
            }
        }

        m_startPos1 = m_planetPlacement1.position;
        m_startPos2 = m_planetPlacement2.position;
    }

    public void SpawnPlanets(float movementDuration)
    {
        if (GameManager.Instance.IsPracticeMode) return;

        m_planetsObject.SetActive(true);

        if (m_movementCoroutine != null)
            StopCoroutine(m_movementCoroutine);

        m_movementCoroutine = StartCoroutine(MovePlanets(m_startPos1, m_startPos2, movementDuration));
    }

    private IEnumerator MovePlanets(Vector3 from1, Vector3 from2, float duration)
    {
        float timer = 0f;

        float radius1 = m_sphereCollider1.radius * m_sphereCollider1.transform.lossyScale.x;
        float radius2 = m_sphereCollider2.radius * m_sphereCollider2.transform.lossyScale.x;

        float contactDistance = radius1 + radius2;
        Vector3 direction = (from2 - from1).normalized;

        Vector3 to1 = from1 + direction * (Vector3.Distance(from1, from2) - contactDistance) / 2f;
        Vector3 to2 = from2 - direction * (Vector3.Distance(from1, from2) - contactDistance) / 2f;

        while (timer < duration)
        {
            if (!GameManager.Instance.IsPaused)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);

                m_planetPlacement1.position = Vector3.Lerp(from1, to1, t);
                m_planetPlacement2.position = Vector3.Lerp(from2, to2, t);
            }
            yield return null;
        }

        m_planetPlacement1.position = to1;
        m_planetPlacement2.position = to2;
    }

    public Vector3 GetCenter()
    {
        Vector3 center = (m_planetPlacement1.position + m_planetPlacement2.position) / 2f;
        center.y += m_sphereCollider1.radius * m_sphereCollider1.transform.lossyScale.x;
        return center;
    }

    private GameObject m_shockwave;
    public void HandleCollision(Vector3 contactPoint)
    {
        if (m_movementCoroutine != null)
        {
            StopCoroutine(m_movementCoroutine);
            m_movementCoroutine = null;
        }

        Debug.Log("Planet collision detected!");

        Invoke(nameof(StopChrono), 0.01f);

        CameraManager.Instance.ShakeCamera(8f, 5f);
        m_shockwave = Instantiate(m_shockwavePrefab, contactPoint, Quaternion.identity);
        m_screenFlash.Flash();
        StartCoroutine(LerpMaterialFloat("_DissolveIntensity", 0f, 1f, 3f));
        GameManager.Instance.OnRespawned.AddListener(DesactivateFlash);
    }

    private void StopChrono()
    {
        GameManager.Instance.StopChrono();
    }

    private void DesactivateFlash()
    {
        if (m_screenFlash != null)
        {
            m_shockwave.gameObject.SetActive(false);
        }

        GameManager.Instance.OnRespawned.RemoveListener(DesactivateFlash);
    }

    private IEnumerator LerpMaterialFloat(string propertyName, float from, float to, float duration)
    {
        if (m_planetMaterials.Count == 0)
            yield break;

        float timer = 0f;
        while (timer < duration)
        {
            if (!GameManager.Instance.IsPaused)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                float value = Mathf.Lerp(from, to, t);

                foreach (var mat in m_planetMaterials)
                {
                    if (mat.HasProperty(propertyName))
                        mat.SetFloat(propertyName, value);
                }
            }
            yield return null;
        }

        foreach (var mat in m_planetMaterials)
        {
            if (mat.HasProperty(propertyName))
                mat.SetFloat(propertyName, to);
        }
    }

    public void DisableSphereColliders()
    {
        if (m_sphereCollider1 != null)
            m_sphereCollider1.enabled = false;
        if (m_sphereCollider2 != null)
            m_sphereCollider2.enabled = false;
    }
}
