using System;

namespace Assets.Behaviors.Scripts.BehaviorTree.Nodes
{
    public class Root : Node
    {
        private Blackboard myBlackboard;
        private Node child;

        public Root(Node child)
        {
            myBlackboard = new Blackboard();
            this.child = child;
        }

        public void Tick()
        {
            var status = child.Evaluate(myBlackboard);
            if (status != NodeStatus.RUNNING)
            {
                Reset(myBlackboard);
            }
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            throw new NotImplementedException();
        }

        public override void Reset(Blackboard blackboard)
        {
            child.Reset(myBlackboard);
        }
    }
}
