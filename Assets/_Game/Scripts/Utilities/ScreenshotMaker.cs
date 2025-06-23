using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class ScreenshotTaker : MonoBehaviour
{
    [Header("Résolution HD")]
    public int width = 3840;
    public int height = 2160;
    public string folder = "Screenshots";

    [Button]
    public void TakeScreenshot()
    {
        string directory = Path.Combine(Application.dataPath, "..", folder);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"screenshot_{timestamp}_{width}x{height}.png";
        string path = Path.Combine(directory, filename);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Mode Éditeur : capture de la vue de scène
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                RenderTexture rt = new RenderTexture(width, height, 24);
                view.camera.targetTexture = rt;
                Texture2D screen = new Texture2D(width, height, TextureFormat.RGB24, false);
                view.camera.Render();
                RenderTexture.active = rt;
                screen.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screen.Apply();

                view.camera.targetTexture = null;
                RenderTexture.active = null;
                DestroyImmediate(rt);

                File.WriteAllBytes(path, screen.EncodeToPNG());
                Debug.Log($"📷 Screenshot pris depuis la vue de scène : {path}");
            }
            else
            {
                Debug.LogWarning("Aucune vue de scène active pour la capture.");
            }
        }
        else
#endif
        {
            // Mode Jeu : capture de la caméra principale
            ScreenCapture.CaptureScreenshot(path, superSize: Mathf.RoundToInt((float)width / Screen.width));
            Debug.Log($"📷 Screenshot pris depuis la caméra de jeu : {path}");
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Backspace))
            TakeScreenshot();
    }
}
