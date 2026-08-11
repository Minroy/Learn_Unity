#if UNITY_EDITOR
using InventoryModule.IDSystem;
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace InventoryModule.Diagnostics
{
    internal class ItemRegistryGitPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets,
                                                   string[] deletedAssets,
                                                   string[] movedAssets,
                                                   string[] movedFromAssetPaths)

        {
            bool hasItemChanged = false;

            for (int i = 0; i < importedAssets.Length; i++)
            {
                string path = importedAssets[i];

                // Load main asset and verify if it implements your IItem interface
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset is IItem)
                {
                    hasItemChanged = true;
                    break;
                }
            }

            if (hasItemChanged)
            {
                Debug.Log("[InventoryModule] IItem asset changes detected (Git/Import). Validating IDs...");
                ItemRegistryTool.Validate();
            }
        }
    }

    [InitializeOnLoad]
    internal static class ItemRegistryStartupValidator
    {
        private const string SESSION_KEY = "InventoryModule_StartupValidated";

        static ItemRegistryStartupValidator()
        {
            // Check if we've already validated during this Editor session
            if (SessionState.GetBool(SESSION_KEY, false))
                return;

            // Flag as validated so code recompiles won't trigger it again
            SessionState.SetBool(SESSION_KEY, true);

            EditorApplication.delayCall += () =>
            {
                Debug.Log("[InventoryModule] Running initial Editor startup ID check...");
                ItemRegistryTool.Validate();
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class BuildProfileChecker : IPreprocessBuildWithReport
    {
        public int callbackOrder => -10;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (ItemRegistryTool.Validate() == false)
            {
                EditorUtility.DisplayDialog(
                    "InventoryModule — Build Aborted",
                    "Items with missing or duplicate ItemIDs detected.\nCheck the console for details.",
                    "OK");

                throw new OperationCanceledException(
                    "[InventoryModule] Build aborted: Items exist with missing or duplicate ItemIDs!");
            }

            Debug.Log("[InventoryModule] Pre-build validation passed.");
        }
    }
}
#endif
