using UnityEngine;

public class ShockwaveEffect : MonoBehaviour
{
    private float expandDuration = 1.5f;
    private float maxScale = 10f;                  // Scale factor (for ring's size relative to planet)
    private float fadeDuration = 5.0f;
    private float baseScaleMultiplier = 386f;     // Directly matches planet scale



    private Material m_material;
    private Color m_startColor;
    private float m_elapsed;
    private Vector3 m_finalScale;

    private void Start()
    {
        m_material = GetComponent<Renderer>().material;
        m_startColor = m_material.color;

        transform.localScale = Vector3.zero;

        // Final scale in X and Z (flat ring)
        m_finalScale = new Vector3(maxScale * baseScaleMultiplier, transform.localScale.y, maxScale * baseScaleMultiplier);
    }

    private void Update()
    {
        m_elapsed += Time.deltaTime;

        float expandT = Mathf.Clamp01(m_elapsed / expandDuration);
        float fadeT = Mathf.Clamp01(m_elapsed / fadeDuration);

        transform.localScale = Vector3.Lerp(Vector3.zero, m_finalScale, expandT);

        Color color = m_startColor;
        color.a = Mathf.Lerp(m_startColor.a, 0f, fadeT);
        m_material.color = color;

        if (fadeT >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
