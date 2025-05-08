using Sirenix.OdinInspector;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "FolderDescription", menuName = "Editor/FolderDescription")]
public class FolderDescription : ScriptableObject
{
    [SerializeField, TextArea, MultiLineProperty(4)] private string _description;
    //private void Awake()
    //{
    //    string assetPath = AssetDatabase.GetAssetPath(this);
    //    string folderName = new DirectoryInfo(Path.GetDirectoryName(assetPath)).Name;
    //    AssetDatabase.RenameAsset(assetPath, $"_{folderName}Description ");
    //}
}
