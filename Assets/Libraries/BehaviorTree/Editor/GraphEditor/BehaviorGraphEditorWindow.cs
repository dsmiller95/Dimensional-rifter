using Assets.Libraries.BehaviorTree.Editor.GraphEditor;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            var sceneWindow = typeof(SceneView);
            var newWindow = CreateWindow<BehaviorGraphEditorWindow>(sceneWindow);

            var name = asset.name;
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

            var saveButton = new Button(() =>
            {
                _graphView.SaveToAsset();
            });
            saveButton.text = "Save";
            toolbar.Add(saveButton);

            toolbar.Add(SetupCreateNodeMenu());

            rootVisualElement.Add(toolbar);
        }

        private ToolbarMenu SetupCreateNodeMenu()
        {
            var createMenu = new ToolbarMenu();
            createMenu.text = "Create Node";
            createMenu.menu.AppendAction("test/testAction", (actionThing) =>
            {
                Debug.Log("test action actioned");
                Debug.Log(actionThing.name);
            });

            var domain = System.AppDomain.CurrentDomain;
            var allAssemblies = domain.GetAssemblies();

            foreach (var assembly in allAssemblies)
            {
                var types = assembly.GetTypes()
                    .Where(type =>
                        type.IsClass && !type.IsAbstract &&
                        type.IsSubclassOf(typeof(NodeFactory)));

                foreach (var type in types)
                {
                    var theAttr = type.GetCustomAttribute<FactoryGraphNodeAttribute>();
                    if (theAttr != null)
                    {
                        createMenu.menu.AppendAction(theAttr.menuName, actionThing =>
                        {
                            _graphView.CreateNode(theAttr, type);
                            Debug.Log($"Create new node with children: {theAttr.childCountClassification}");
                        });
                    }
                }
            }

            return createMenu;
        }

        private void OnDisable()
        {
            var isDirty = _graphView.IsDirtyState;
            if (isDirty)
            {
                var shouldSave = EditorUtility.DisplayDialog("Save changes?", "There are unsaved changes to the graph. Save changes before exit?", "Save", "Discard");
                if (shouldSave)
                {
                    _graphView.SaveToAsset();
                }
            }
            rootVisualElement.Remove(_graphView);
        }
    }
}
