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
            if (GUILayout.Button("Edit Graph"))
            {
                var window = EditorWindow.GetWindow<BehaviorGraphEditorWindow>(name);
                window.titleContent = new GUIContent(name);
                window.factoryGraph = serializedObject.targetObject as CompositeFactoryGraph;
                window.Show();
            }
        }
    }
}
