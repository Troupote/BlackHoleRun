using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ToggleGameObjectActiveState
{
    static ToggleGameObjectActiveState()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceId, Rect selectionRect)
    {
        Event e = Event.current;

        if (e != null && e.type == EventType.MouseDown && e.button == 2)
        {
            if (selectionRect.Contains(e.mousePosition))
            {
                GameObject clickedObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

                if (clickedObject != null)
                {
                    clickedObject.SetActive(!clickedObject.activeSelf);
                    e.Use();
                }
            }
        }
    }
}
