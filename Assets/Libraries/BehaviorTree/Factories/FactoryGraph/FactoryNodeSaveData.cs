using UnityEngine;

namespace BehaviorTree.Factories.FactoryGraph
{
    public class FactoryNodeSavedNode
    {
        public string Guid;
        public string Title;
        public bool isEntryNode;

        public NodeFactory factory;
        public Vector2 position;

        public string[] ConnectedChildrenGuids;
    }
}
