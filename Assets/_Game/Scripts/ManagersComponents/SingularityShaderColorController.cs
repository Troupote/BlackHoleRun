using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SingularityShaderColorController : MonoBehaviour
{
    [SerializeField]
    private Renderer m_targetRenderer;

    private Material m_singularityMaterial;

    private CharacterGameplayData m_gameplayData;

    private bool m_isInitialized = false;
    private void Initialize()
    {
        if (m_targetRenderer == null)
        {
            Debug.LogError("Target Renderer is not assigned!", this);
            return;
        }

        m_singularityMaterial = m_targetRenderer.material;

        if (m_singularityMaterial == null)
        {
            Debug.LogError("Material is missing on the Renderer!", this);
            return;
        }

        m_gameplayData = CharactersManager.Instance.GameplayData;

        m_isInitialized = true;
    }
    public void SetPlayerColors(bool a_isPlayerOne)
    {
        if (!m_isInitialized)
        {
            Initialize();
        }

        Debug.Log($"Setting colors for {(a_isPlayerOne ? "Player Two" : "Player One")}", this);

        m_singularityMaterial.SetColor("_ExteriorColor", a_isPlayerOne ? m_gameplayData.PlayerOneColors.Exterior : m_gameplayData.PlayerTwoColors.Exterior);
        m_singularityMaterial.SetColor("_InteriorColor", a_isPlayerOne ? m_gameplayData.PlayerOneColors.Interior : m_gameplayData.PlayerTwoColors.Interior);
    }

}
