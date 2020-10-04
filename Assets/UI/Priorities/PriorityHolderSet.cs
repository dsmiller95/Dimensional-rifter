using Assets.Behaviors.Errands.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.UI.Priorities
{
    public class PriorityHolderSet : MonoBehaviour
    {
        public ErrandType[] errandTypesToSetPrioritiesFor;
        public PriorityHolderObjectSet priorities;

        public GameObject editPrefab;

        private void Awake()
        {
        }

        private void Start()
        {
            ReDrawPriorityGrid();
            priorities.OnItemSetChanged += ReDrawPriorityGrid;
        }

        private PriorityRowEditController[] editControllers;

        private void ReDrawPriorityGrid()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            editControllers = priorities.GetAll()
                .Select(priority =>
                    PriorityRowEditController.CreateAsChildOf(
                        transform,
                        editPrefab,
                        priority,
                        this
                        )
                ).ToArray();
        }
    }
}
