using BehaviorTree.Factories.FactoryGraph;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Libraries.BehaviorTree.Editor.GraphEditor
{
    public class BehaviorGraphView : GraphView
    {
        public CompositeFactoryGraph factoryGraph;

        public BehaviorGraphView(CompositeFactoryGraph factoryGraph)
        {
            this.factoryGraph = factoryGraph;

            styleSheets.Add(Resources.Load<StyleSheet>("BehaviorGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            LoadStateFromAsset();
        }

        private void LoadStateFromAsset()
        {
            var nodes = factoryGraph.SavedNodes;
            if(nodes == null)
            {
                var rootNode = GenerateEntryNode();
                AddElement(rootNode);
            }
            else
            {
                var nodesWithSaveData = nodes.Select(savedNode =>
                    {
                        var instantiatedNode = BehaviorGraphViewNode.NodeFromSaveData(savedNode);
                        AddElement(instantiatedNode);
                        return new { savedNode, instantiatedNode };
                    })
                    .ToList();
                foreach (var createdNode in nodesWithSaveData)
                {
                    createdNode.instantiatedNode
                        .GenerateConnectionsForNode(createdNode.savedNode, this);
                }
            }
        }

        public void SaveToAsset()
        {
            var nodeSaveData = nodes.ForEach(node =>
            {
                var typedNode = node as BehaviorGraphViewNode;
                return typedNode.GetSaveData();
            });
            factoryGraph.SavedNodes = nodeSaveData.ToArray();
        }

        public BehaviorGraphViewNode GetBehaviorNodeByGuid(string guid)
        {;
            return nodes.ToList()
                .Cast<BehaviorGraphViewNode>()
                .Where(node => node.GUID == guid)
                .FirstOrDefault();
        }

        public void CreateNode(string nodeName)
        {
            AddElement(BehaviorGraphViewNode.CreateNewNode(nodeName, false));
        }

        public void LinkPorts(Port outputSocket, Port inputSocket)
        {
            var tempEdge = new Edge()
            {
                output = outputSocket,
                input = inputSocket
            };
            inputSocket.Connect(tempEdge);
            outputSocket.Connect(tempEdge);
            Add(tempEdge);
        }

        private BehaviorGraphViewNode GenerateEntryNode()
        {
            var newNode = BehaviorGraphViewNode.CreateNewNode("Root", true);
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
    }
}
