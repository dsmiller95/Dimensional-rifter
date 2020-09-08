using BehaviorTree;
using System;
using UniRx;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Assets.Libraries.BehaviorTree.Editor
{
    public class BehaviorTreeView : TreeView
    {
        private int framesPerUpdate;
        public BehaviorTreeView(TreeViewState treeViewState, int framesPerUpdate)
            : base(treeViewState)
        {
            Reload();
            this.framesPerUpdate = framesPerUpdate;
        }

        private BehaviorTreeMachine inspectedMachine;
        public void SetInspectedMachine(BehaviorTreeMachine newMachine)
        {
            if (newMachine != inspectedMachine)
            {
                inspectedMachine = newMachine;
                OnInspectedMachineChange();
            }
        }

        IDisposable updateHandler;
        private void OnInspectedMachineChange()
        {
            updateHandler?.Dispose();
            Reload();
            if (inspectedMachine != null)
            {
                updateHandler = Observable.IntervalFrame(framesPerUpdate)
                    .TakeUntilDisable(inspectedMachine)
                    .ObserveOnMainThread()
                    .Subscribe(f =>
                    {
                        Repaint();
                    }).AddTo(inspectedMachine);

            }
        }

        protected override TreeViewItem BuildRoot()
        {
            Debug.Log("root build");
            if (inspectedMachine == null)
            {
                return EmptyTree("Nothing selected ya dingus");
            }
            if (inspectedMachine.instantiatedRootTreeNode == null)
            {
                return EmptyTree("Can only inspect when running");
            }


            var rootNode = new BehaviorNodeTreeElement(inspectedMachine.instantiatedRootTreeNode);
            rootNode.AddChildrenIfAnyRecursively();
            rootNode.depth = -1;

            // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
            // are created from data. Here we create a fixed set of items. In a real world example,
            // a data model should be passed into the TreeView and the items created from the model.

            // This section illustrates that IDs should be unique. The root item is required to 
            // have a depth of -1, and the rest of the items increment from that.
            //var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            //var allItems = new List<TreeViewItem>
            //{
            //    new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
            //    new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
            //    new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
            //    new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
            //    new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
            //    new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
            //    new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
            //    new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
            //    new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
            //};

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            //SetupParentsAndChildrenFromDepths(root, allItems);
            SetupDepthsFromParentsAndChildren(rootNode);

            // Return root of the tree
            return rootNode;
        }

        private TreeViewItem EmptyTree(string message)
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            root.AddChild(new TreeViewItem { id = 1, depth = 0, displayName = message });
            return root;
        }

        private static readonly float ExecutionTimeFadeoutSeconds = 3;

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is BehaviorNodeTreeElement behaviorNode)
            {
                var backgroundColoring = behaviorNode.GetBackgroundColoring(ExecutionTimeFadeoutSeconds);
                var rect = args.rowRect;
                EditorGUI.DrawRect(rect, backgroundColoring);
            }
            base.RowGUI(args);
        }
    }
}
