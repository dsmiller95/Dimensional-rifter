using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "TimeBasedSelector", menuName = "Behaviors/Control/TimeBasedSelector", order = 20)]
    [FactoryGraphNode("Composite/TimeBasedSelector", "TimeBasedSelector", 3)]
    public class TimeBasedSelectorFactory : NodeFactory
    {
        public GameTime gameTimeProvider;

        public NodeFactory daytimeNode;
        public NodeFactory eveningNode;
        public NodeFactory nightNode;

        public override void SetChildFactories(IEnumerable<NodeFactory> children)
        {
            var childrenArray = children.ToArray();
            if (childrenArray.Length != 3)
            {
                throw new NotImplementedException("must have exactly 3 children");
            }
            daytimeNode = childrenArray[0];
            eveningNode = childrenArray[1];
            nightNode = childrenArray[2];
        }

        protected override BehaviorNode OnCreateNode(GameObject target)
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
