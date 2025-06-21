using BHR;
using UnityEngine;

public class NewSingularityPreviewController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_singularityPreviewPrefab;

    private GameObject m_currentPreviewObject;

    private SingularityShaderColorController m_shaderColorController;

    private void Start()
    {
        if (m_singularityPreviewPrefab == null)
        {
            Debug.LogError("Singularity Preview Prefab is not assigned.");
            return;
        }
    }

    /// <summary>
    /// Inflates the singularity preview at the specified character position.
    /// </summary>
    /// <param name="a_characterPosition"></param>
    public void Inflate(Transform a_characterPosition)
    {
        if (m_currentPreviewObject != null)
        {
            Destroy(m_currentPreviewObject);
            m_shaderColorController = null;
        }
        m_currentPreviewObject = Instantiate(m_singularityPreviewPrefab, a_characterPosition.position, Quaternion.identity);

        m_shaderColorController = m_currentPreviewObject.GetComponent<SingularityShaderColorController>();

        m_shaderColorController.SetPlayerColors();
    }

    /// <summary>
    /// Destroys the current singularity preview object if it exists.
    /// </summary>
    public void Deflate()
    {
        if (m_currentPreviewObject != null)
        {
            Destroy(m_currentPreviewObject);
            m_currentPreviewObject = null;
        }
    }
}
