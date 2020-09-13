using Assets.Libraries.BehaviorTree.Editor.GraphEditor;
using BehaviorTree.Factories.FactoryGraph;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Libraries.BehaviorTree.Editor
{
    public class BehaviorGraphEditorWindow : EditorWindow
    {
        private BehaviorGraphView _graphView;
        public CompositeFactoryGraph factoryGraph;


        [MenuItem("BehaviorTree/GraphEditor")]
        private static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<BehaviorGraphEditorWindow>();
            window.titleContent = new GUIContent("Behavior Tree Graph");
            window.Show();
        }


        private void OnEnable()
        {
            this.ConstructGraphView();
            GenerateToolbar();
        }


        private void ConstructGraphView()
        {
            _graphView = new BehaviorGraphView()
            {
                name = "Behavior Graph"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var nodeCreateButton = new Button(() =>
            {
                _graphView.CreateNode("Fresh");

            });

            nodeCreateButton.text = "New Node";
            toolbar.Add(nodeCreateButton);

            rootVisualElement.Add(toolbar);

        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}
