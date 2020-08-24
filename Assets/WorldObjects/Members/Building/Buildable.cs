using TradeModeling.Inventories;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.WorldObjects.Members.Building
{
    [RequireComponent(typeof(ResourceInventory))]
    [RequireComponent(typeof(TileMapMember))]
    public class Buildable : MonoBehaviour
    {
        public UnityEvent whenBuilt;

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
                var tileMember = GetComponent<TileMapMember>();
                if (tileMember.memberType is BuildingMemberType buildable)
                {
                    _resourceRequirementCached = buildable.resourceCost;
                }
                return _resourceRequirementCached;
            }
        }

        public bool CanBuild()
        {
            if (!ResourceRequirement.HasValue)
            {
                return false;
            }

            var currentInventory = inventory.inventory;
            var consumeOption = currentInventory.Consume(ResourceRequirement.Value.type, ResourceRequirement.Value.amount);
            return consumeOption.info == ResourceRequirement.Value.amount;
        }
        public bool BuildIfPossible()
        {
            if (!ResourceRequirement.HasValue)
            {
                return false;
            }

            var currentInventory = inventory.inventory;
            var consumeOption = currentInventory.Consume(ResourceRequirement.Value.type, ResourceRequirement.Value.amount);
            if (consumeOption.info == ResourceRequirement.Value.amount)
            {
                whenBuilt.Invoke();
                Destroy(this);
                return true;
            }
            return false;
        }
    }
}
