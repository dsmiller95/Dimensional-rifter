using Assets.Scripts.Core;
using System;
using System.Linq;
using System.Text;
using TradeModeling.Inventories;
using UnityEngine;


namespace Assets.WorldObjects.Members.Hungry.HeldItems
{
    public class InventoryHoldingController : MonoBehaviour
    {
        public GenericSelector<IInventory<Resource>> inventoryTarget;
        public VariableInstantiator stateHolder;

        public bool GrabAllItemsIntoSelf(
            IInventory<Resource> inventoryToGrabFrom,
            GameObject originPoint,
            StringBuilder toastMessage)
        {
            var grabbedAny = false;
            foreach (var resource in inventoryToGrabFrom.GetAllResourceTypes().ToList())
            {
                grabbedAny |= GrabItemIntoSelf(
                    inventoryToGrabFrom,
                    resource,
                    originPoint,
                    toastMessage);
            }
            return grabbedAny;
        }

        public bool GrabItemIntoSelf(
            IInventory<Resource> inventoryToGrabFrom,
            Resource itemType,
            GameObject originPoint,
            StringBuilder toastMessage,
            float amount = -1)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            if (amount < 0)
            {
                amount = inventoryToGrabFrom.Get(itemType);
            }
            var added = inventoryToGrabFrom.TransferResourceInto(itemType, inventory, amount);
            if (added.info < 1e-5)
            {
                return false;
            }

            added.Execute();
            toastMessage.AppendLine($"{added.info} {Enum.GetName(typeof(Resource), itemType)}");

            return true;
        }

        public float GrabItemIntoSelf(
            Resource itemType,
            GameObject originPoint,
            StringBuilder toastMessage,
            float amount)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            var added = inventory.Add(itemType, amount);
            if (added.info < 1e-5)
            {
                return 0f;
            }

            added.Execute();
            toastMessage.AppendLine($"{added.info} {Enum.GetName(typeof(Resource), itemType)}");

            return added.info;
        }

        public bool PullAllItemsFromSelfIntoInv(
            IInventory<Resource> inventoryToDepositInto,
            GameObject destinationPoint,
            StringBuilder toastMessage)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            var suppliedAny = false;
            foreach (var resource in inventory.GetAllResourceTypes().ToList())
            {
                suppliedAny |= PullItemFromSelf(
                    inventoryToDepositInto,
                    resource,
                    destinationPoint,
                    toastMessage);
            }
            return suppliedAny;
        }

        public bool PullItemFromSelf(
            IInventory<Resource> inventoryToDepositInto,
            Resource itemType,
            GameObject destinationPoint,
            StringBuilder toastMessage,
            float amount = -1)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            if (amount < 0)
            {
                amount = inventory.Get(itemType);
            }
            var transferred = inventory.TransferResourceInto(itemType, inventoryToDepositInto, amount);
            if (transferred.info < 1e-5)
            {
                return false;
            }

            transferred.Execute();

            toastMessage.AppendLine($"{transferred.info} {Enum.GetName(typeof(Resource), itemType)}");
            return true;
        }

        public float PullItemFromSelf(
            Resource itemType,
            GameObject destinationPoint,
            StringBuilder toastMessage,
            float amount)
        {
            var inventory = inventoryTarget.GetCurrentValue(stateHolder);
            var transferred = inventory.Consume(itemType, amount);
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