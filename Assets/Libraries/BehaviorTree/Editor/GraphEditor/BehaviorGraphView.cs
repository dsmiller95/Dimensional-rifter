using BehaviorTree.Factories.FactoryGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Libraries.BehaviorTree.Editor.GraphEditor
{
    public class BehaviorGraphView : GraphView
    {
        public CompositeFactoryGraph factoryGraph;
        public bool IsDirtyState = false;

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

            graphViewChanged = change =>
            {
                IsDirtyState = true;
                return change;
            };
        }


        private void LoadStateFromAsset()
        {
            var nodes = factoryGraph.SavedNodes;
            if (nodes == null || nodes.Length <= 0)
            {
                var rootNode = GenerateEntryNode();
                AddElement(rootNode);
                IsDirtyState = true;
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
                        .GenerateConnectionsFromSaveData(createdNode.savedNode, this);
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
            factoryGraph.DestroyUnlinkedNodeFactories();


            var rootNode = this.Query<BehaviorGraphViewRootNode>().First();
            try
            {
                var rootChild = rootNode
                    .outputContainer
                    .Query<Port>().First()
                    ?.connections.First()
                    ?.input.node as BehaviorGraphViewNode;

                factoryGraph.entryFactory = rootChild?.backingFactory;
            }
            catch (Exception)
            {
                Debug.LogError("Root not not connected");
            }
            IsDirtyState = false;
            EditorUtility.SetDirty(factoryGraph);
        }

        public BehaviorGraphViewNode GetBehaviorNodeByGuid(string guid)
        {
            return nodes.ToList()
                .Cast<BehaviorGraphViewNode>()
                .Where(node => node.GUID == guid)
                .FirstOrDefault();
        }

        public void CreateNode(
            FactoryGraphNodeAttribute attribute,
            Type nodeFactoryType)
        {
            AddElement(BehaviorGraphViewNode.CreateNewNodeFromFactory(
                factoryGraph,
                attribute,
                nodeFactoryType));
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
            return BehaviorGraphViewNode.CreateEntryNode();
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
