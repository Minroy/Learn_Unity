#if UNITY_EDITOR

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace InventoryModule.IDSystem
{
    [InitializeOnLoad]
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
            InventoryItems.Clear();

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

                if(currentCount >= countToPuase)
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
    

        static ItemRegistryTool()
        {
            Validate();
        }

         public static bool Validate()
        {
            string[] assets =
                AssetDatabase.FindAssets("t:ScriptableObject");


            Dictionary<uint, string> ids = new();


            bool valid = true;


            foreach(string guid in assets)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(guid);


                ScriptableObject asset =
                    AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);


                if(asset is not IItem item)
                    continue;


                if(!item.ItemID.HasValue)
                {
                    Debug.LogWarning(
                        $"[InventoryModule] {asset.name} has no ItemID."
                    );

                    continue;
                }


                uint id = item.ItemID.Value;


                if(ids.TryGetValue(id, out string existing))
                {
                    Debug.LogError(
                        $"[InventoryModule] Duplicate ItemID detected!\n\n" +
                        $"ID: {id}\n" +
                        $"Item 1: {existing}\n" +
                        $"Item 2: {path}"
                    );

                    valid = false;
                }
                else
                {
                    ids.Add(id, path);
                }
            }


            if(valid)
            {
                Debug.Log(
                    $"[InventoryModule] Validation passed. " +
                    $"{ids.Count} IDs checked."
                );
            }


            return valid;
        }
    
        
    }
}



#endif