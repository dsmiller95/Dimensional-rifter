using Assets.Scripts.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings
{
    /// <summary>
    /// Component which must be present on any item which will recieve deliveries
    ///     this implementation does not save the types of items it can recieve
    /// </summary>
    [RequireComponent(typeof(ResourceInventory))]
    public class ConstantTypeStorable : MonoBehaviour
    {
        [Tooltip("Whether the storage can recieve deliveries")]
        public BooleanReference IsOpen;

        public Resource[] storableTypes;

        private ISet<Resource> _storables;
        public ISet<Resource> Storables
        {
            get
            {
                if (_storables == null)
                {
                    _storables = new HashSet<Resource>(storableTypes);
                }
                return _storables;
            }
        }

        private ResourceInventory inventory;
        private void Awake()
        {
            inventory = GetComponent<ResourceInventory>();
        }

        public bool CanStore(Resource type)
        {
            return Storables.Contains(type) && inventory.inventory.CanFitMoreOf(type);
        }
    }
}
