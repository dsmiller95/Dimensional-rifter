using UnityEditor;
using UnityEngine;

namespace BehaviorTree.Nodes
{
    public enum NodeStatus
    {
        FAILURE,
        SUCCESS,
        RUNNING
    }

    public abstract class Node
    {
#if UNITY_EDITOR
        public string Label;
        public float LastExecuted;
        public NodeStatus LastStatus;
#endif
        public int UniqueID;

        public Node()
        {
            UniqueID = GUID.Generate().GetHashCode();
        }
        public NodeStatus Evaluate(Blackboard blackboard)
        {
            var result = OnEvaluate(blackboard);
#if UNITY_EDITOR
            LastExecuted = Time.time;
            LastStatus = result;
#endif
            return result;
        }

        protected abstract NodeStatus OnEvaluate(Blackboard blackboard);

        public abstract void Reset(Blackboard blackboard);
    }
}
