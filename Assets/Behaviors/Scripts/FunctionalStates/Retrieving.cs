using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects;
using System;
using System.Runtime.InteropServices;
using TradeModeling.Inventories;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    [Serializable]
    public struct RetrievalAmount
    {
        public Resource type;
        public float amount;
    }

    public class Retrieving : ContinueableState<TileMapMember>
    {
        public RetrievalAmount retrival;
        public Retrieving(RetrievalAmount amount)
        {
            retrival = amount;
        }

        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var seekResult = tileMember.SeekClosestOfType(ResourceSourceFilter);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var storageInv = seekResult.reached.GetComponent<ResourceInventory>();
                var selfInv = data.GetComponent<ResourceInventory>();

                var transfer = storageInv.inventory.TransferResourceInto(retrival.type, selfInv.inventory, retrival.amount);
                transfer.Execute();

                return next;
            }
            return this;
        }

        public bool ResourceSourceFilter(TileMapMember member)
        {
            if (!member.memberType.recieveStorage)
            {
                return false;
            }
            var storage = member.gameObject.GetComponent<ResourceInventory>();
            return storage.inventory.Get(retrival.type) > 0;
        }

        public override void TransitionIntoState(TileMapMember data)
        {
            base.TransitionIntoState(data);
        }
        public override void TransitionOutOfState(TileMapMember data)
        {
        }

        public override string ToString()
        {
            return $"Retrieving {retrival.amount:F1} of {Enum.GetName(typeof(Resource), retrival.type)}";
        }
    }
}
