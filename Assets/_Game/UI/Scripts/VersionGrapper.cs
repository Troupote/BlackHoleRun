using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class VersionDisplay : MonoBehaviour
{
    [Header("Affiche la version dans le champ texte")]
    [Tooltip("Format : {0} = Application.version")]
    public string format = "Version {0}";

    private TMP_Text tmpText;
    private Text uiText;

    void Awake()
    {
        UpdateVersionText();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Mettre à jour automatiquement en éditeur
        UpdateVersionText();
    }
#endif

    [Button]
    public void UpdateVersionText()
    {
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
        if (uiText == null) uiText = GetComponent<Text>();

        string version = string.Format(format, Application.version);

        if (tmpText != null)
            tmpText.text = version;
        else if (uiText != null)
            uiText.text = version;
        else
            Debug.LogWarning($"[{nameof(VersionDisplay)}] Aucun composant Text ou TMP_Text trouvé sur {gameObject.name}");
    }
}
