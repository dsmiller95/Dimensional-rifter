using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite;
using Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers;
using Assets.WorldObjects;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.NodeFactories
{
    [CreateAssetMenu(fileName = "TimeBasedSelector", menuName = "Behaviors/Control/TimeBasedSelector", order = 20)]
    public class TimeBasedSelectorFactory : NodeFactory
    {
        public GameTime gameTimeProvider;

        public NodeFactory daytimeNode;
        public NodeFactory eveningNode;
        public NodeFactory nightNode;

        public override Node CreateNode(GameObject target)
        {
            return new Sequence(
                    new GetTimeZoneClassificaitonLeaf(gameTimeProvider, "time"),
                    new SelectOne("time",
                        daytimeNode.CreateNode(target),
                        eveningNode.CreateNode(target),
                        nightNode.CreateNode(target))
                    );
        }
    }
}
