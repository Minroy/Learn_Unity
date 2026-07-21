#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace InventoryModule.IDSystem
{
    [CreateAssetMenu(order = 3, fileName = "ItemRegisteryTool", menuName = "Inventory Items")]
    public class ItemRegisteryTool : ScriptableObject
    {
        [SerializeField] private List<ScriptableObject> InventoryItems = new List<ScriptableObject>();

        ////To-Do Low : Store keys to prevent dupication, and handle removal. SO
        ///Save_System does not curropt.
        //[SerializeField, HideInInspector]
        //private Dictionary<uint, ScriptableObject> _GlobalItemLookUp = new();


        /// <summary>
        /// Generates unique ID for each item. 
        /// </summary>
        [ContextMenu(nameof(GenerateID))]
        private void GenerateID()
        {
            InventoryItems.Clear();
            var assets = AssetDatabase.FindAssets("t:ScriptableObject");
            uint CurrentId = 0;

            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                ScriptableObject asset =
                    AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset is not IItemData itemData) continue; //first we check if the IItemHas a ID. if it does then we skip. it it dont. then we assign.
               

                itemData.SetID(CurrentId);
                EditorUtility.SetDirty(asset);
                Debug.Log($"{asset.name} assigned ID {CurrentId}");
                InventoryItems.Add(asset);

                CurrentId++;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
#endif