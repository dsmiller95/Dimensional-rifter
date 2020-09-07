using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
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
