using UnityEngine;

namespace BehaviorTree.Nodes
{
    public class Wait : Leaf
    {
        private float waitTime;
        private float remainingWait;
        public Wait(float waitTime)
        {
            this.waitTime = waitTime;
            remainingWait = waitTime;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (remainingWait < 0)
            {
                return NodeStatus.SUCCESS;
            }

            remainingWait -= Time.deltaTime;
            return NodeStatus.RUNNING;
        }
        public override void Reset(Blackboard blackboard)
        {
            remainingWait = waitTime;
        }
    }
}
