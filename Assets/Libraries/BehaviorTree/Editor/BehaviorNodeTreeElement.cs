using BehaviorTree.Nodes;
using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Assets.Libraries.BehaviorTree.Editor
{
    public class BehaviorNodeTreeElement : TreeViewItem
    {
        private Node node;

        public BehaviorNodeTreeElement(Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            this.node = node;
            displayName = GetLabel();
            id = node.UniqueID;
        }

        public string GetLabel()
        {
            if (node.Label == null || node.Label.Length <= 0)
            {
                return node.GetType().Name;
            }
            return node.Label;
        }


        public Color GetBackgroundColoring(float fadeoutTimeSpan)
        {
            var timeSinceExecuted = Time.time - node.LastExecuted;
            if (timeSinceExecuted > fadeoutTimeSpan)
            {
                return Color.clear;
            }
            var fadeFactor = timeSinceExecuted / fadeoutTimeSpan;

            return Color.Lerp(ColorBasedOnStatus(), Color.clear, fadeFactor);
        }

        private static readonly Color failColor = Color.red;
        private static readonly Color runningColor = Color.cyan;
        private static readonly Color successColor = Color.green;
        private Color ColorBasedOnStatus()
        {
            switch (node.LastStatus)
            {
                case NodeStatus.FAILURE:
                    return failColor;
                case NodeStatus.SUCCESS:
                    return successColor;
                case NodeStatus.RUNNING:
                    return runningColor;
                default:
                    return failColor;
            }
        }

        public void AddChildrenIfAnyRecursively()
        {
            if (node is Root rootNode)
            {
                CreateAndAddAllChildren(rootNode.Child);
            }
            else if (node is Decorator decoratorNode)
            {
                CreateAndAddAllChildren(decoratorNode.Child);
            }
            else if (node is CompositeNode compositeNode)
            {
                foreach (var childNode in compositeNode.children)
                {
                    CreateAndAddAllChildren(childNode);
                }
            }
        }

        private BehaviorNodeTreeElement CreateAndAddAllChildren(Node node)
        {
            var newChild = new BehaviorNodeTreeElement(node);
            AddChild(newChild);
            newChild.AddChildrenIfAnyRecursively();
            return newChild;
        }
    }
}
