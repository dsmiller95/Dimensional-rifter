using Assets.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    [RequireComponent(typeof(ResourceInventory))]
    public class GatherableProducer: MonoBehaviour, IGatherable
    {
        public BooleanReference IsGatherable;

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
