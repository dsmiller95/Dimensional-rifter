using BehaviorTree.Factories.FactoryGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Assets.Libraries.BehaviorTree.Editor.GraphEditor
{
    public class BehaviorGraphViewNode : Node
    {
        public static readonly Vector2 DefaultNodeSize = new Vector2(300, 150);
        public string GUID;

        protected BehaviorGraphViewNode()
        {
        }

        public static BehaviorGraphViewNode CreateNewNode(string title, bool isEntryNode)
        {
            BehaviorGraphViewNode newNode;
            if (isEntryNode)
            {
                newNode = new BehaviorGraphViewRootNode();
            }
            else
            {
                newNode = new BehaviorGraphViewNode();
            }
            newNode.GUID = Guid.NewGuid().ToString();
            newNode.title = title;
            newNode.SetupUIElements();
            newNode.SetPosition(new Rect(Vector2.zero, DefaultNodeSize));
            return newNode;
        }

        public static BehaviorGraphViewNode NodeFromSaveData(
            FactoryNodeSavedNode savedNodeData)
        {
            BehaviorGraphViewNode newNode;
            if (savedNodeData.isEntryNode)
            {
                newNode = new BehaviorGraphViewRootNode();
            }
            else
            {
                newNode = new BehaviorGraphViewNode();
            }
            newNode.GUID = savedNodeData.Guid;
            newNode.title = savedNodeData.Title;

            newNode.SetupUIElements(savedNodeData);
            newNode.SetPosition(new Rect(savedNodeData.position, DefaultNodeSize));
            return newNode;
        }

        public virtual void GenerateConnectionsForNode(
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
                    if(otherNode == null)
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
            return new FactoryNodeSavedNode
            {
                Guid = GUID,
                position = GetPosition().position,
                Title = title,
                isEntryNode = false,
                ConnectedChildrenGuids = GetChildrenIfConnected()
            };
        }

        protected virtual void SetupUIElements(FactoryNodeSavedNode saveData = null)
        {
            SetupDefaultUIElements();
            if (saveData != null)
            {
                foreach (var index in saveData.ConnectedChildrenGuids.Select((x, index) => index))
                {
                    AddNumberedChildPort(index + 1);
                }
            }
            RefreshExpandedState();
            RefreshPorts();
        }

        protected string[] GetChildrenIfConnected()
        {
            return outputContainer.Query<Port>()
                       .ForEach(port => port.connections.FirstOrDefault()?.input.node as BehaviorGraphViewNode)
                       .Select(x => x?.GUID ?? null)
                       .ToArray();
        }

        private void SetupDefaultUIElements()
        {
            var input = GeneratePort(Direction.Input);

            input.portName = "Parent";
            inputContainer.Add(input);

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
