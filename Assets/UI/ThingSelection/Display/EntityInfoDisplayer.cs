using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Buildings.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.UI.ThingSelection.Display
{
    public class EntityInfoDisplayer : MonoBehaviour
    {
        private Entity selectedEntity = Entity.Null;

        private EntityQuery selectedEntityQuery;
        private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        private ConnectivityEntitySystem ConnectivitySystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConnectivityEntitySystem>();

        public TextMeshProUGUI text;

        // Start is called before the first frame update
        void Start()
        {
            var entityManager = EntityManager;
            selectedEntityQuery = entityManager.CreateEntityQuery(typeof(SelectedComponentFlag));
        }

        // Update is called once per frame
        void Update()
        {
            if (selectedEntityQuery.IsEmpty)
            {
                SelectionClear();
                selectedEntity = Entity.Null;
                return;
            }
            var selectedEntities = selectedEntityQuery.ToEntityArray(Allocator.Temp);
            var nextSelectedEntity = selectedEntities[selectedEntities.Length - 1];
            if (nextSelectedEntity == selectedEntity)
            {
                //TODO: this won't show an update if the entity changes while it is selected.
                return;
            }
            selectedEntity = nextSelectedEntity;
            EntityDataUpdated();
        }

        private void SelectionClear()
        {
            text.text = "Nothing Selected";
        }

        private void EntityDataUpdated()
        {
            var resultText = new StringBuilder();
            var manager = EntityManager;
            if (manager.HasComponent<SelectedTitleSharedComponent>(selectedEntity))
            {
                var titleData = manager.GetSharedComponentData<SelectedTitleSharedComponent>(selectedEntity);
                resultText.AppendLine(titleData.Title);
                resultText.AppendLine("----------");
            }
            if (manager.HasComponent<UniversalCoordinatePositionComponent>(selectedEntity))
            {
                var coordinateData = manager.GetComponentData<UniversalCoordinatePositionComponent>(selectedEntity);
                resultText.AppendLine($"Position: {coordinateData.Value}");
                var connector = ConnectivitySystem;
                if (connector.HasRegionMaps && connector.Regions.ContainsKey(coordinateData.Value))
                {
                    resultText.AppendLine($"Region: {System.Convert.ToString(connector.Regions[coordinateData.Value], 2).PadLeft(16, '0')}");
                }
            }
            var isBuildingData = manager.HasComponent<BuildingChildComponent>(selectedEntity);
            if (isBuildingData)
            {
                ShowBuildingDataFromEntity(resultText, selectedEntity, manager);
            }
            else if (manager.HasComponent<BuildingParentComponent>(selectedEntity))
            {
                var buildingParentData = manager.GetComponentData<BuildingParentComponent>(selectedEntity);
                if (manager.Exists(buildingParentData.buildingEntity))
                {
                    ShowBuildingDataFromEntity(resultText, buildingParentData.buildingEntity, manager);
                }
            }
            if (!isBuildingData && manager.HasComponent<ItemAmountClaimBufferData>(selectedEntity))
            {
                ShowInventoryDataFromEntity(resultText, selectedEntity, manager);
            }

            text.text = resultText.ToString();
        }

        private void ShowInventoryDataFromEntity(StringBuilder resultText, Entity e, EntityManager manager)
        {
            var inventoryData = manager.GetBuffer<ItemAmountClaimBufferData>(e);
            resultText.AppendLine($"Inventory:");
            inventoryData.SerializeCurrentAmount(resultText);
        }

        private void ShowBuildingDataFromEntity(StringBuilder resultText, Entity e, EntityManager manager)
        {
            var inventoryData = manager.GetBuffer<ItemAmountClaimBufferData>(e);
            resultText.AppendLine($"Building Materials:");
            inventoryData.SerializeCurrentAmount(resultText);
        }
    }
}