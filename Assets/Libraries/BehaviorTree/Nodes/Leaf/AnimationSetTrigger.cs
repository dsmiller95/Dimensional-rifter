using UnityEngine;

namespace BehaviorTree.Nodes
{
    public class AnimationSetTrigger : ComponentMemberLeaf<Animator>
    {
        private string animationTrigger;

        public AnimationSetTrigger(
            GameObject gameObject,
            string animationTrigger) : base(gameObject)
        {
            this.animationTrigger = animationTrigger;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            componentValue.SetTrigger(animationTrigger);
            return NodeStatus.SUCCESS;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
