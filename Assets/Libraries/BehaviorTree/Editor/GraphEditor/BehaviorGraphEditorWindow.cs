using Assets.Libraries.BehaviorTree.Editor.GraphEditor;
using BehaviorTree.Factories.FactoryGraph;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Libraries.BehaviorTree.Editor
{
    public class BehaviorGraphEditorWindow : EditorWindow
    {
        private static IDictionary<int, BehaviorGraphEditorWindow> OpenWindows = new Dictionary<int, BehaviorGraphEditorWindow>();
        public static BehaviorGraphEditorWindow GetWindowForAsset(CompositeFactoryGraph asset)
        {
            var key = asset.GetInstanceID();
            if (OpenWindows.TryGetValue(key, out BehaviorGraphEditorWindow existingWindow))
            {
                if (existingWindow != null && existingWindow)
                {
                    return existingWindow;
                }
            }

            var name = asset.name;
            var newWindow = CreateInstance<BehaviorGraphEditorWindow>();
            newWindow.titleContent = new GUIContent(name);
            newWindow.factoryGraph = asset;
            newWindow.LoadFromAsset();
            OpenWindows[key] = newWindow;
            return newWindow;
        }


        private BehaviorGraphView _graphView;
        public CompositeFactoryGraph factoryGraph;


        private void OnEnable()
        {
            if (factoryGraph && factoryGraph != null)
            {
                LoadFromAsset();
            }
        }

        private void LoadFromAsset()
        {
            ConstructGraphView(factoryGraph);
            GenerateToolbar();
        }


        private void ConstructGraphView(CompositeFactoryGraph factoryGraph)
        {
            _graphView = new BehaviorGraphView(factoryGraph)
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

            var saveButton = new Button(() =>
            {
                _graphView.SaveToAsset();
            });
            saveButton.text = "Save";
            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}
