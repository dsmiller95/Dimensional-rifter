using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Libraries.BehaviorTree.Editor.GraphEditor
{
    public class BehaviorGraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private readonly Vector2 defaultNodeSize = new Vector2(300, 150);


        public BehaviorGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var rootNode = GenerateEntryNode();
            AddElement(rootNode);
        }

        public void CreateNode(string nodeName)
        {
            AddElement(GenerateNewNode(nodeName));
        }

        public BehaviorGraphViewNode GenerateNewNode(string nodeName)
        {
            var newNode = new BehaviorGraphViewNode
            {
                title = nodeName,
                GUID = Guid.NewGuid().ToString()
            };

            var input = GeneratePort(newNode, Direction.Input);
            input.portName = "Parent";
            newNode.inputContainer.Add(input);

            var newOutputButton = new Button(() =>
            {
                AddChoicePort(newNode);
            });
            newOutputButton.text = "New child";
            newNode.titleButtonContainer.Add(newOutputButton);

            newNode.RefreshExpandedState();
            newNode.RefreshPorts();

            newNode.SetPosition(new UnityEngine.Rect(Vector2.zero, defaultNodeSize));

            return newNode;
        }

        private void AddChoicePort(BehaviorGraphViewNode node)
        {
            var generatedPort = GeneratePort(node, Direction.Output);
            var outputPortCount = node.outputContainer.Query("connector").ToList().Count;
            generatedPort.portName = $"Child {outputPortCount}";

            node.outputContainer.Add(generatedPort);

            node.RefreshExpandedState();
            node.RefreshPorts();
        }

        private BehaviorGraphViewNode GenerateEntryNode()
        {
            var newNode = new BehaviorGraphViewNode
            {
                title = "Root",
                GUID = Guid.NewGuid().ToString(),
                EntryPoint = true
            };

            var port = GeneratePort(newNode, Direction.Output);
            port.portName = "Child";
            newNode.outputContainer.Add(port);

            newNode.RefreshExpandedState();
            newNode.RefreshPorts();

            newNode.SetPosition(new UnityEngine.Rect(Vector2.zero, defaultNodeSize));

            return newNode;
        }


        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }


        private Port GeneratePort(BehaviorGraphViewNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }
    }
}
