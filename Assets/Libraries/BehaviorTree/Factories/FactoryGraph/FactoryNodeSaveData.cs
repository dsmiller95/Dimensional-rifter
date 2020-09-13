using System;
using UnityEngine;

namespace BehaviorTree.Factories.FactoryGraph
{
    [Serializable]
    public class FactoryNodeSavedNode
    {
        public string Guid;
        public string Title;

        public NodeFactory factory;
        public Vector2 position;

        public string[] ConnectedChildrenGuids;
    }
}
