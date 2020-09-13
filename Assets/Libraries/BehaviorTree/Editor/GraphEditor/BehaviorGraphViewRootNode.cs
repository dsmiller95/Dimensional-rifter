﻿using BehaviorTree.Factories.FactoryGraph;
using UnityEditor.Experimental.GraphView;

namespace Assets.Libraries.BehaviorTree.Editor.GraphEditor
{
    public class BehaviorGraphViewRootNode : BehaviorGraphViewNode
    {
        protected override void SetupUIElements(FactoryNodeSavedNode saveData = null)
        {
            SetupRootNodeUIElements();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void SetupRootNodeUIElements()
        {
            var port = GeneratePort(Direction.Output);
            port.portName = "Child";
            outputContainer.Add(port);
        }

        public override FactoryNodeSavedNode GetSaveData()
        {
            return new FactoryNodeSavedNode
            {
                Guid = GUID,
                position = GetPosition().position,
                Title = title,
                isEntryNode = true,
                ConnectedChildrenGuids = GetChildrenIfConnected()
            };
        }
    }
}
