////// Editor/ContainerBehaviourEditor.cs
////using UnityEditor;
////using UnityEngine;

////[CustomEditor(typeof(ContainerBehaviour))]
////public class ContainerBehaviourEditor : Editor
////{
////    public override void OnInspectorGUI()
////    {
////        DrawDefaultInspector();

////        ContainerBehaviour container = (ContainerBehaviour)target;

////        if (GUILayout.Button("Generate Slots"))
////        {
////            container.GenerateSlots(container.StartSize);
////        }

////        if (GUILayout.Button("Remove Slots"))
////        {
////            container.RemoveSlots();
////        }
////    }
////}