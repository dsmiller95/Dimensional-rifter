using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree
{
    public class BehaviorTreeMachine : MonoBehaviour
    {
        public RootFactory rootTreeFactory;

        private Root rootTreeNode;

        private void Awake()
        {
            rootTreeNode = rootTreeFactory.CreateNode(gameObject) as Root;
        }

        private void Update()
        {
            rootTreeNode.Tick();
        }
    }
}
