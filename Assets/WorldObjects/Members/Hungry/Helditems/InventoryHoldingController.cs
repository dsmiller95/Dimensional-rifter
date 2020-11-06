using Assets.Scripts.Core;
using Assets.Scripts.ResourceManagement;
using System;
using System.Text;
using TradeModeling.Inventories;
using UnityEngine;


namespace Assets.WorldObjects.Members.Hungry.HeldItems
{
    public class InventoryHoldingController : MonoBehaviour
    {
        public GenericSelector<IInventory<Resource>> inventoryTarget;
        public VariableInstantiator stateHolder;

        public float GrabUnclaimedItemIntoSelf(
            Resource itemType,
            float amount)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            var added = inventory.Add(itemType, amount);
            added.Execute();
            return added.info;
        }

        public float GrabItemIntoSelf(
            Resource itemType,
            GameObject originPoint,
            StringBuilder toastMessage,
            ResourceAllocation amount)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            var added = inventory.Add(itemType, amount.Amount);
            if (!amount.Execute(added.info))
            {
                Debug.LogError("Failed to execute allocated subtraction");
            }
            if (added.info < 1e-5)
            {
                return 0f;
            }

            added.Execute();
            toastMessage.AppendLine($"{added.info} {Enum.GetName(typeof(Resource), itemType)}");

            return added.info;
        }


        public float PullUnclaimedItemFromSelf(
            Resource itemType,
            float amount)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            var added = inventory.Consume(itemType, amount);
            added.Execute();
            return added.info;
        }

        public float PullItemFromSelf(
            Resource itemType,
            GameObject destinationPoint,
            StringBuilder toastMessage,
            ResourceAllocation amount)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            var transferred = inventory.Consume(itemType, amount.Amount);
            if (!amount.Execute(transferred.info))
            {
                Debug.LogError("Failed to execute allocated addition");
            }
            if (transferred.info < 1e-5)
            {
                return 0f;
            }

            transferred.Execute();

            toastMessage.AppendLine($"{transferred.info} {Enum.GetName(typeof(Resource), itemType)}");
            return transferred.info;
        }
    }
}