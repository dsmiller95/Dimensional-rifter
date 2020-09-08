using System;

namespace BehaviorTree.Nodes
{
    public class Root : Node
    {
        private Blackboard myBlackboard;
        public Node Child { get; private set; }

        public Root(Node child)
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
