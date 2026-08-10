#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace InventoryModule.IDSystem
{
    [CreateAssetMenu(order = 3, fileName = "ItemRegistryTool", menuName = "ItemDataBase")]
    public class ItemRegistryTool : ScriptableObject
    {
        [SerializeField]
        private List<ScriptableObject> InventoryItems = new();



        /// <summary>
        /// Generates missing Item IDs using Asset GUID hashing.
        /// Existing IDs are ignored.
        /// </summary>
        [ContextMenu(nameof(GenerateID))]
        private async Task GenerateID()
        {
            string[] assets = AssetDatabase.FindAssets("t:ScriptableObject");
            await Task.Yield();
            short countToPuase = 500;

            short currentCount = 0;
            foreach (var guid in assets)
            {
                currentCount++;
                string path = AssetDatabase.GUIDToAssetPath(guid);

                ScriptableObject asset =
                    AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset is not IItem itemData)
                    continue;



                if (itemData.ItemID.HasValue)
                {
                    InventoryItems.Add(asset);
                    continue;
                }

                if (currentCount >= countToPuase)
                {
                    currentCount = 0;
                    await Task.Yield();
                }


                uint generatedID = Generate(guid);

                itemData.SetID(generatedID);

                EditorUtility.SetDirty(asset);

                Debug.Log(
                    $"{asset.name} assigned ItemID {generatedID}"
                );


                InventoryItems.Add(asset);
            }

            AssetDatabase.SaveAssets();

            Debug.Log(
                $"Item Registry Generated. Found {InventoryItems.Count} items."
            );
        }

        public static uint Generate(string guid)
        {
            unchecked
            {
                uint hash = 2166136261;

                foreach (char c in guid)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return hash;
            }
        }


        //Check If, 2 items have the Same ID. (Typically after Git commints it is useful)
        [ContextMenu("Check For Dups")]
        public void CheckValidate()
        {
            Validate();
        }

        public static bool Validate()
        {
            string[] assets = AssetDatabase.FindAssets("t:ScriptableObject");
            Dictionary<uint, string> ids = new();
            bool valid = true;
            int itemCount = 0;

            foreach (string guid in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset is not IItem item) continue;

                itemCount++;

                if (!item.ItemID.HasValue)
                {
                    Debug.LogWarning($"[InventoryModule] {asset.name} has no ItemID.");
                    valid = false;
                    continue;
                }

                uint id = item.ItemID.Value;

                if (ids.TryGetValue(id, out string existing))
                {
                    Debug.LogError($"[InventoryModule] Duplicate ItemID!\nID: {id}\nItem 1: {existing}\nItem 2: {path}");
                    valid = false;
                }
                else
                {
                    ids.Add(id, path);
                }
            }

            // This tells you if IItem assets are being found at all
            Debug.Log($"[InventoryModule] Validated {itemCount} IItem assets. Valid: {valid}");
            return valid;
        }
    }
}



#endif