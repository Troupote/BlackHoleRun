using BHR;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SingularityShaderColorController : MonoBehaviour
{
    [SerializeField]
    private Renderer m_targetRenderer;

    [SerializeField]
    private Renderer m_horizonRenderer;

    private Material m_singularityMaterial;
    private Material m_horizonMaterial;

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
        m_horizonMaterial = m_horizonRenderer.material;

        if (m_singularityMaterial == null || m_horizonMaterial == null)
        {
            Debug.LogError("Material is missing on the Renderer!", this);
            return;
        }

        m_gameplayData = CharactersManager.Instance.GameplayData;

        m_isInitialized = true;
    }
    public void SetPlayerColors()
    {
        if (!m_isInitialized)
        {
            Initialize();
        }
        bool a_player = GameManager.Instance._mainPlayerIsPlayerOne;

        m_singularityMaterial.SetColor("_ExteriorColor", a_player ? m_gameplayData.PlayerOneColors.Exterior*12 : m_gameplayData.PlayerTwoColors.Exterior*12);
        m_singularityMaterial.SetColor("_InteriorColor", a_player ? m_gameplayData.PlayerOneColors.Interior : m_gameplayData.PlayerTwoColors.Interior);

        m_horizonMaterial.SetColor("_Color2", a_player ? m_gameplayData.PlayerOneColors.Exterior*6 : m_gameplayData.PlayerTwoColors.Exterior*6);
        m_horizonMaterial.SetColor("_Color1", a_player ? m_gameplayData.PlayerOneColors.Interior : m_gameplayData.PlayerTwoColors.Interior);

        a_player = !a_player;
    }

}
