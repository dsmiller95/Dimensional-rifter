using Assets.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeModeling.Inventories;
using UnityEngine;
using UniRx;

namespace Assets.WorldObjects.Members.Food
{
    [RequireComponent(typeof(ResourceInventory))]
    public class GatherableProducer: MonoBehaviour, IGatherable
    {
        public BooleanReference IsGatherable;

        private void Awake()
        {
            IsGatherable.ValueChanges.TakeUntilDisable(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if(!pair.Previous && pair.Current)
                    {
                        BecomeGatherable();
                    }
                }).AddTo(this);
        }

        private void BecomeGatherable()
        {
            var inventory = GetComponent<ResourceInventory>().inventory;
            inventory.Add(Resource.FOOD, 1f).Execute();
        }

        public bool CanGather()
        {
            return IsGatherable.CurrentValue;
        }

        public void OnGathered()
        {
            IsGatherable.SetValue(false);
        }
    }
}
