using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree
{
    public class BehaviorTreeMachine : MonoBehaviour
    {
        public RootFactory rootTreeFactory;

        public Root instantiatedRootTreeNode;

        private void Awake()
        {
            instantiatedRootTreeNode = rootTreeFactory.CreateNode(gameObject) as Root;
        }

        private void Update()
        {
            instantiatedRootTreeNode.Tick();
        }
    }
}
