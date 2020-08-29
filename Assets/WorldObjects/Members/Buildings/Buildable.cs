using Assets.Scripts.Core;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Members.Building
{
    [RequireComponent(typeof(ResourceInventory))]
    [RequireComponent(typeof(TileMapMember))]
    public class Buildable : MonoBehaviour
    {
        public BooleanReference isBuilt;

        private ResourceInventory inventory;

        private void Awake()
        {
            inventory = GetComponent<ResourceInventory>();
        }

        private ResourceRequirement? _resourceRequirementCached = null;
        public ResourceRequirement? ResourceRequirement
        {
            get
            {
                if (_resourceRequirementCached.HasValue)
                {
                    return _resourceRequirementCached;
                }
                var tileMember = GetComponent<TileMapMember>();
                if (tileMember.memberType is BuildingMemberType buildable)
                {
                    _resourceRequirementCached = buildable.resourceCost;
                }
                return _resourceRequirementCached;
            }
        }

        private bool IsSelfSetup()
        {
            return !isBuilt.CurrentValue && ResourceRequirement.HasValue;
        }

        /// <summary>
        /// If the buildable can transition into the built state
        /// </summary>
        /// <returns>True if the buildable is set up and is not already built</returns>
        public bool CanBeBuilt()
        {
            return IsSelfSetup();
        }

        /// <summary>
        /// If the buildable is ready to transition into the build state
        /// </summary>
        /// <returns>True if the buildable CanBeBuilt, and also has the resources required to be built</returns>
        public bool CanBuild()
        {
            if (!IsSelfSetup())
            {
                return false;
            }

            var currentInventory = inventory.inventory;
            var consumeOption = currentInventory.Consume(ResourceRequirement.Value.type, ResourceRequirement.Value.amount);
            return consumeOption.info == ResourceRequirement.Value.amount;
        }
        public bool BuildIfPossible()
        {
            if (!IsSelfSetup())
            {
                return false;
            }

            var currentInventory = inventory.inventory;
            var consumeOption = currentInventory.Consume(ResourceRequirement.Value.type, ResourceRequirement.Value.amount);
            if (consumeOption.info == ResourceRequirement.Value.amount)
            {
                isBuilt.SetValue(true);
                return true;
            }
            return false;
        }
    }
}
