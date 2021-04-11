using Assets.WorldObjects.SaveObjects.SaveManager;
using BehaviorTree.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    [CreateAssetMenu(fileName = "ErrandBoard", menuName = "Behaviors/Errands/ErrandBoard", order = 1)]
    public class ErrandBoard : ScriptableObject
    {
        public ISet<IErrandSource<IErrand>>[] ErrandSourcesByErrandTypeID;
        public ErrandTypeRegistry ErrandRegistry;


        public void Init()
        {
            SaveSystemHooks.Instance.PreLoad += ClearErrandSources;
        }

        private void ClearErrandSources()
        {
            if (ErrandSourcesByErrandTypeID == null) return;
            Debug.Log("Clearing all errands");
            foreach (var sourceSet in ErrandSourcesByErrandTypeID)
            {
                sourceSet.Clear();
            }
        }

        /// <summary>
        /// Attempt to claim any errand matching <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="claimer"></param>
        /// <returns>Errand if any found, otherwise null</returns>
        public ErrandClaimingNode AttemptClaimAnyErrandOfType(ErrandType type, GameObject claimer)
        {
            var errandIndex = type.myId;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);
            return new ErrandClaimingNode(
                claimer,
                ErrandSourcesByErrandTypeID[errandIndex].ToArray());
        }

        public class ErrandClaimingNode : BehaviorNode
        {
            public IErrand resultingErrand;
            private IErrandSource<IErrand>[] sources;
            private int currentSourceIndex = 0;
            private IErrandSourceNode<IErrand> currentNode = null;

            private GameObject targetObj;
            public ErrandClaimingNode(
                GameObject targetObj,
                IErrandSource<IErrand>[] sources)
            {
                this.targetObj = targetObj;
                this.sources = sources;
            }

            protected override NodeStatus OnEvaluate(Blackboard blackboard)
            {
                if (resultingErrand != null)
                {
                    return NodeStatus.SUCCESS;
                }
                if (sources.Length <= 0)
                {
                    return NodeStatus.FAILURE;
                }
                if (currentNode == null && currentSourceIndex == 0)
                {
                    currentNode = sources[0].GetErrand(targetObj);
                }
                while (currentSourceIndex < sources.Length)
                {
                    var result = currentNode?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
                    switch (result)
                    {
                        case NodeStatus.SUCCESS:
                            resultingErrand = currentNode.ErrandResult;
                            return NodeStatus.SUCCESS;
                        case NodeStatus.FAILURE:
                            currentSourceIndex++;
                            if (currentSourceIndex < sources.Length)
                            {
                                currentNode = sources[currentSourceIndex].GetErrand(targetObj);
                            }
                            else
                            {
                                return NodeStatus.FAILURE;
                            }
                            break;
                        case NodeStatus.RUNNING:
                            return NodeStatus.RUNNING;
                        default:
                            break;
                    }
                }
                return NodeStatus.FAILURE;
            }

            public override void Reset(Blackboard blackboard)
            {
                throw new System.NotImplementedException();
            }

        }

        public bool DeRegisterErrandSource(IErrandSource<IErrand> errandSource)
        {
            var errandType = errandSource.ErrandType;
            var errandIndex = errandType.myId;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);
            Debug.Log($"[ERRANDS]Deregistered errand source of type: {errandType.name}");
            return ErrandSourcesByErrandTypeID[errandIndex].Remove(errandSource);
        }

        public void RegisterErrandSource(IErrandSource<IErrand> source)
        {
            var errandIndex = source.ErrandType.myId;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);

            ErrandSourcesByErrandTypeID[errandIndex].Add(source);
            Debug.Log($"[ERRANDS]Registered errand source of type: {source.ErrandType.name}");
        }

        private void ExtendErrandMappingToLengthIfNeeded(int errandIndex)
        {
            if (ErrandSourcesByErrandTypeID != null && errandIndex < ErrandSourcesByErrandTypeID.Length)
            {
                return;
            }
            var newArray = new ISet<IErrandSource<IErrand>>[errandIndex + 1];
            int i = 0;
            if (ErrandSourcesByErrandTypeID != null)
            {
                for (; i < ErrandSourcesByErrandTypeID.Length; i++)
                {
                    newArray[i] = ErrandSourcesByErrandTypeID[i];
                }
            }
            for (; i < newArray.Length; i++)
            {
                newArray[i] = new HashSet<IErrandSource<IErrand>>();
            }

            ErrandSourcesByErrandTypeID = newArray;
        }
    }
}
