using System;

namespace BehaviorTree.Nodes
{
    public class Root : BehaviorNode
    {
        private Blackboard myBlackboard;
        public BehaviorNode Child { get; private set; }

        public Root(BehaviorNode child)
        {
            myBlackboard = new Blackboard();
            Child = child;
        }

        public void Tick()
        {
            var status = Child.Evaluate(myBlackboard);
            if (status != NodeStatus.RUNNING)
            {
                Reset(myBlackboard);
            }
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            throw new NotImplementedException();
        }

        public override void Reset(Blackboard blackboard)
        {
            Child.Reset(myBlackboard);
        }
    }
}
