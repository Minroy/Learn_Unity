
#if UNITY_EDITOR
using InventoryModule.Generics.Interfaces;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

[CreateAssetMenu(order = 3, fileName = "ItemRegisteryTool", menuName = "Inventory Items")]
public class ItemRegisteryTool : ScriptableObject
{
    [SerializeField] private List<ScriptableObject> _BakeditemRegistery = new List<ScriptableObject>();


    [ContextMenu(nameof(GenerateID))]
    private void GenerateID()
    {
        _BakeditemRegistery.Clear();
        var assets = AssetDatabase.FindAssets("t:ScriptableObject");
        int CurrentId = 0;

        foreach (var guid in assets)
        {

            var path = AssetDatabase.GUIDToAssetPath(guid); // why load a asset part first is the Item isnt even a IItemData

            ScriptableObject asset =
                AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (asset is not IItemData itemData) continue; //first we check if the IItemHas a ID. if it does then we skip. it it dont. then we assign.
            itemData.SetID(CurrentId);
            EditorUtility.SetDirty(asset);
            Debug.Log($"{asset.name} assigned ID {CurrentId}");

            CurrentId++;
            _BakeditemRegistery.Add(asset);
        }
        AssetDatabase.SaveAssets();
    }
}
#endif