using System;
using Unity.Entities;
using UnityEngine;

namespace Assets.UI.ThingSelection
{
    public class SelectableEntityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public string titleWhenSelected;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddSharedComponentData(entity, new SelectedTitleSharedComponent
            {
                Title = titleWhenSelected
            });
            dstManager.AddComponent<SelectableFlagComponent>(entity);
        }
    }
    public struct SelectedTitleSharedComponent : ISharedComponentData, IEquatable<SelectedTitleSharedComponent>
    {
        public string Title;
        public bool Equals(SelectedTitleSharedComponent other)
        {
            return Title == other.Title;
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode();
        }
    }

    /// <summary>
    /// A flag indicating that this entity can be selected, and can have the SelectedComponent added to it when selected
    /// </summary>
    public struct SelectableFlagComponent : IComponentData
    {
    }
}
