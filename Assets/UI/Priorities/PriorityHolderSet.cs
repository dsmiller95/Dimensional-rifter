﻿using System.Linq;
using UnityEngine;

namespace Assets.UI.Priorities
{
    public class PriorityHolderSet : MonoBehaviour
    {
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

        private void OnDestroy()
        {
            priorities.OnItemSetChanged -= ReDrawPriorityGrid;
        }

        private PriorityRowEditController[] editControllers;

        private void ReDrawPriorityGrid()
        {
            if (!this)
            {
                priorities.OnItemSetChanged -= ReDrawPriorityGrid;
                return;
            }
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
