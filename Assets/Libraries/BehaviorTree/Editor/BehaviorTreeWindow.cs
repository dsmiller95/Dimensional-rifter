﻿using BehaviorTree;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Assets.Libraries.BehaviorTree.Editor
{
    public class BehaviorTreeWindow : EditorWindow
    {
        // SerializeField is used to ensure the view state is written to the window 
        // layout file. This means that the state survives restarting Unity as long as the window
        // is not closed. If the attribute is omitted then the state is still serialized/deserialized.
        [SerializeField] TreeViewState m_TreeViewState;

        //The TreeView is not serializable, so it should be reconstructed from the tree data.
        BehaviorTreeView m_BehaviorTree;
        private void OnEnable()
        {
            // Check whether there is already a serialized view state (state 
            // that survived assembly reloading)
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            m_BehaviorTree = new BehaviorTreeView(m_TreeViewState, 15);
            UpdateBehaviorTreeViewTargetMachine();
        }

        private void OnSelectionChange()
        {
            UpdateBehaviorTreeViewTargetMachine();
        }
        private void OnFocus()
        {
            UpdateBehaviorTreeViewTargetMachine();
        }

        private void UpdateBehaviorTreeViewTargetMachine()
        {
            var nextInspectedMachine = GetSelectedMachineIfAny();
            if (nextInspectedMachine == null)
            {
                // the machine is sticky, only switches if you select a different object with a machine attached
                return;
            }
            m_BehaviorTree?.SetInspectedMachine(nextInspectedMachine);
        }

        private BehaviorTreeMachine GetSelectedMachineIfAny()
        {
            var selectedObj = Selection.activeGameObject;
            if (selectedObj == null)
            {
                return null;
            }
            var behaviorMachine = selectedObj.GetComponentInChildren<BehaviorTreeMachine>();
            return behaviorMachine;
        }

        private void OnGUI()
        {
            m_BehaviorTree.OnGUI(new Rect(0, 0, position.width, position.height));
        }

        [MenuItem("BehaviorTree/Inspector")]
        private static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<BehaviorTreeWindow>();
            window.titleContent = new GUIContent("Behavior Tree Inspector");
            window.Show();
        }
    }
}
