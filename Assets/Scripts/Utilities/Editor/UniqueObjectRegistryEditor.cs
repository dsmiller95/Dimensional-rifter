using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.Members;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Core.Editor
{
    [CustomEditor(typeof(UniqueObjectRegistry), true)]
    public class UniqueObjectRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Reassign unique IDs"))
            {
                var registry = serializedObject.targetObject as UniqueObjectRegistry;
                registry.AssignAllIDs();
                Debug.Log("Successfully reset all object IDs. All save files may be invalid.");
            }
        }
    }
}