using Assets.Scripts.Core;
using TradeModeling.Inventories;
using UniRx;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    [RequireComponent(typeof(ResourceInventory))]
    public class GatherableProducer : MonoBehaviour, IGatherable
    {
        public BooleanReference IsGatherable;

        public Resource resourceToSpawn = Resource.FOOD;
        public float resourceAmount = 1f;
        private void Awake()
        {
            IsGatherable.ValueChanges.TakeUntilDisable(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (!pair.Previous && pair.Current)
                    {
                        BecomeGatherable();
                    }
                }).AddTo(this);
        }

        private void BecomeGatherable()
        {
            var inventory = GetComponent<ResourceInventory>().inventory;
            inventory.Add(resourceToSpawn, resourceAmount).Execute();
        }

        public bool CanGather()
        {
            return IsGatherable.CurrentValue;
        }

        public void OnGathered()
        {
            IsGatherable.SetValue(false);
        }

        public Resource GatherableType => resourceToSpawn;
    }
}
