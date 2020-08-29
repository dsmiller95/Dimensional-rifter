using Assets.Scripts.Core;
using Assets.Scripts.ObjectVariables;
using TradeModeling.Inventories;
using UniRx;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    public class GatherableProducer : MonoBehaviour, IGatherable
    {
        public BooleanReference IsGatherable;
        public InventoryReference InventoryToProduceInto;

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
            var inventory = InventoryToProduceInto.CurrentValue;
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
