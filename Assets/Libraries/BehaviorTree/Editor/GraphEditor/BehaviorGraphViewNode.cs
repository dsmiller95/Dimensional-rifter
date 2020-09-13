using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Libraries.BehaviorTree.Editor.GraphEditor
{
    public class BehaviorGraphViewNode : Node
    {
        public static readonly Vector2 DefaultNodeSize = new Vector2(300, 150);
        public string GUID;
        public NodeFactory backingFactory;
        public int childCountClassification;

        protected BehaviorGraphViewNode()
        {
        }

        public static BehaviorGraphViewNode CreateNewNodeFromFactory(
            CompositeFactoryGraph objectToStoreNewNode,
            FactoryGraphNodeAttribute attribute,
            Type nodeFactoryType)
        {
            BehaviorGraphViewNode newNode = new BehaviorGraphViewNode();

            newNode.GUID = Guid.NewGuid().ToString();
            newNode.title = attribute.name;
            newNode.childCountClassification = attribute.childCountClassification;

            var newFactory = ScriptableObject.CreateInstance(nodeFactoryType) as NodeFactory;

            newNode.backingFactory = objectToStoreNewNode.RegisterNewFactoryInsideAsset(newFactory);

            newNode.SetupUIElements();
            newNode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));
            return newNode;
        }

        public static BehaviorGraphViewNode CreateEntryNode()
        {
            var newNode = new BehaviorGraphViewRootNode();
            newNode.GUID = Guid.NewGuid().ToString();
            newNode.title = "Root";
            newNode.childCountClassification = 1;
            newNode.backingFactory = null;

            newNode.SetupUIElements();
            newNode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));
            return newNode;
        }

        public static BehaviorGraphViewNode NodeFromSaveData(
            FactoryNodeSavedNode savedNodeData)
        {
            BehaviorGraphViewNode newNode;
            if (savedNodeData.factory == null)
            {
                newNode = new BehaviorGraphViewRootNode();
            }
            else
            {
                var factoryType = savedNodeData.factory.GetType();
                var attr = factoryType.GetCustomAttribute<FactoryGraphNodeAttribute>();
                if (attr == null)
                {
                    throw new NotImplementedException("Cannot create node without factory attribute");
                }

                newNode = new BehaviorGraphViewNode();
                newNode.childCountClassification = attr.childCountClassification;
                newNode.backingFactory = savedNodeData.factory;
            }
            newNode.GUID = savedNodeData.Guid;
            newNode.title = savedNodeData.Title;

            newNode.SetupUIElements(savedNodeData);
            newNode.SetPosition(new Rect(savedNodeData.position, DefaultNodeSize));
            return newNode;
        }

        public virtual void GenerateConnectionsFromSaveData(
            FactoryNodeSavedNode saveData,
            BehaviorGraphView graphView)
        {
            var fullSaveData = graphView.factoryGraph;
            var connectedChildPorts = saveData.ConnectedChildrenGuids.Select(
                    guid =>
                    {
                        if (guid != null && fullSaveData.SavedNodesByGuid.TryGetValue(guid, out FactoryNodeSavedNode node))
                        {
                            return node;
                        }
                        return null;
                    }
                )
                .Select((otherNode, childIndex) =>
                {
                    if (otherNode == null)
                    {
                        return 0;
                    }
                    var childPort = graphView
                        .GetBehaviorNodeByGuid(otherNode.Guid)
                        ?.inputContainer[0] as Port;
                    var outputPort = outputContainer[childIndex] as Port;

                    graphView.LinkPorts(outputPort, childPort);

                    return 0;
                }).ToList();
        }

        public virtual FactoryNodeSavedNode GetSaveData()
        {
            var children = GetChildrenIfConnected();
            backingFactory?.SetChildFactories(children.Select(x => x?.backingFactory));
            return new FactoryNodeSavedNode
            {
                Guid = GUID,
                position = GetPosition().position,
                Title = title,
                factory = backingFactory,
                ConnectedChildrenGuids = children
                       .Select(x => x?.GUID ?? null)
                       .ToArray()
            };
        }

        protected virtual void SetupUIElements(FactoryNodeSavedNode saveData = null)
        {
            SetupInputPort();
            if (saveData != null)
            {
                foreach (var index in saveData.ConnectedChildrenGuids.Select((x, index) => index))
                {
                    AddNumberedChildPort(index + 1);
                }
            }
            if (childCountClassification > 0)
            {
                var protector = 0;
                while (outputContainer.childCount < childCountClassification)
                {
                    AddNumberedChildPort();
                    protector++;
                    if (protector > 20)
                    {
                        throw new Exception("uh oh");
                    }
                }
            }
            if (childCountClassification == -1)
            {
                SetupNewChildButton();
            }
            RefreshExpandedState();
            RefreshPorts();
        }

        protected IList<BehaviorGraphViewNode> GetChildrenIfConnected()
        {
            return outputContainer.Query<Port>()
                       .ForEach(port => port.connections.FirstOrDefault()?.input.node as BehaviorGraphViewNode)
                       .ToList();
        }

        protected void SetupInputPort()
        {
            var input = GeneratePort(Direction.Input);
            input.portName = "Parent";
            inputContainer.Add(input);
        }
        private void SetupNewChildButton()
        {
            var newOutputButton = new Button(() =>
            {
                AddNumberedChildPort();

                RefreshExpandedState();
                RefreshPorts();
            });
            newOutputButton.text = "New child";
            titleButtonContainer.Add(newOutputButton);
        }

        private void AddNumberedChildPort(int number = -1)
        {
            var generatedPort = GeneratePort(Direction.Output);
            if (number == -1)
            {
                number = outputContainer.Query("connector").ToList().Count;
            }
            generatedPort.portName = $"Child {number}";

            outputContainer.Add(generatedPort);
        }

        public Port GeneratePort(Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }
    }
}
