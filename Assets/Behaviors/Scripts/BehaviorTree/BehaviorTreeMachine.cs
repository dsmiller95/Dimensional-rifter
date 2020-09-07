using Assets.Behaviors.Scripts.BehaviorTree.NodeFactories;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.FunctionalLeafs;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree
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
