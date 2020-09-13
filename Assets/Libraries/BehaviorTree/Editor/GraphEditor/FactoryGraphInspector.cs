using BehaviorTree.Factories.FactoryGraph;
using UnityEditor;
using UnityEngine;

namespace Assets.Libraries.BehaviorTree.Editor.GraphEditor
{
    [CustomEditor(typeof(CompositeFactoryGraph))]
    public class FactoryGraphInspector : UnityEditor.Editor
    {
        SerializedProperty savedNodes;
        SerializedProperty entryFactory;

        void OnEnable()
        {
            savedNodes = serializedObject.FindProperty("savedNodes");
            entryFactory = serializedObject.FindProperty("entryFactory");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Edit Graph"))
            {
                var targetObject = serializedObject.targetObject as CompositeFactoryGraph;
                var window = BehaviorGraphEditorWindow.GetWindowForAsset(targetObject);
                window.Show();
            }
        }
    }
}
