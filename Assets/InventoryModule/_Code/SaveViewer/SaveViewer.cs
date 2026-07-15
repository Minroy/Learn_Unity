using InventoryModule.Windows;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryModule.SaveModule 
{

    [DisallowMultipleComponent]
    public class SaveViewer : MonoBehaviour
    {
        [SerializeField] private string settingsKey = "Default";


        /// <summary>
        /// Global registery of viewersSettings, same key will share the same veiwers
        /// </summary>
        public static Dictionary<string, ViewerSettings> SettingsRegistry = new Dictionary<string, ViewerSettings>();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => SettingsRegistry.Clear();


        //public static ViewerSettings Capture(Viewer currentviewer)
        //{

        //}

        public void CaptureComponent<TComponent, Tsettings>(TComponent Viewer, ref Tsettings settings)
            where TComponent : Component 
            where Tsettings : struct

        {





        }
    } 
}
