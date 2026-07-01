// Editor/ContainerUnityEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ContainerUnityEditor : Editor
{
    public void GenerateSlots(int amount, GameObject prefab, Transform parent)
    {
        GameObject slot;
        if (!Application.isPlaying)
        {
            slot = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
        }
        else
        {
            slot = Instantiate(prefab, parent);
        }
    }

    public void AddSlot(GameObject prefab, Transform parent, int index)
    {
        GameObject slot = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
        slot.name = $"Slot_{index}";
        Undo.RegisterCreatedObjectUndo(slot, "Add Slot");
    }

    public void RemoveSlots(Transform parent)
    {
        if (!Application.isPlaying)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        }
        
    }
}
#endif