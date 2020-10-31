﻿using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.SaveObjects.SaveManager;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    public class GrowingThingEntityErrandSource : MonoBehaviour, IErrandSource<HarvestEntityErrand>, IErrandCompletionReciever<HarvestEntityErrand>
    {
        public HarvestErrandType harvestErrandType;
        public ErrandType ErrandType => harvestErrandType;
        public ErrandBoard errandBoard;


        private EntityQuery errandTargetQuery;
        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            errandTargetQuery = entityManager.CreateEntityQuery(
                typeof(ErrandClaimComponent),
                ComponentType.ReadOnly<GrowingThingComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePosition>());

            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            errandBoard.RegisterErrandSource(this);
        }

        public HarvestEntityErrand GetErrand(GameObject errandExecutor)
        {
            Entity targetEntity = Entity.Null;
            HarvestEntityErrand resultErrand = null;
            using (var targets = errandTargetQuery.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var claimed = errandTargetQuery.ToComponentDataArray<ErrandClaimComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    var claimedData = claimed[i];
                    if (claimedData.Claimed)
                    {
                        continue;
                    }
                    targetEntity = targets[i];
                    resultErrand = new HarvestEntityErrand(
                        World.DefaultGameObjectInjectionWorld,
                        harvestErrandType,
                        targetEntity,
                        errandExecutor,
                        this);
                    break;
                }
            }
            if (targetEntity != Entity.Null)
            {
                World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(targetEntity, new ErrandClaimComponent
                {
                    Claimed = true
                });
            }
            return resultErrand;
        }

        public void ErrandAborted(HarvestEntityErrand errand)
        {
            Debug.LogError("[ERRANDS] Entity harvest errand aborted");
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(errand.targetEntity, new ErrandClaimComponent
            {
                Claimed = false
            });
        }

        public void ErrandCompleted(HarvestEntityErrand errand)
        {
            Debug.Log("[ERRANDS] Entity harvest errand completed");
            World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<ErrandClaimComponent>(errand.targetEntity);
        }

    }
}
