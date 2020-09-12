using UnityEngine;

namespace BehaviorTree.Nodes
{
    public class AnimationResetTrigger : ComponentMemberLeaf<Animator>
    {
        private string animationTrigger;

        public AnimationResetTrigger(
            GameObject gameObject,
            string animationTrigger) : base(gameObject)
        {
            this.animationTrigger = animationTrigger;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            componentValue.ResetTrigger(animationTrigger);
            return NodeStatus.SUCCESS;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
