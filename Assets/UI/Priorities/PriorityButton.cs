using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.UI.Priorities
{
    public class PriorityButton : MonoBehaviour, IPointerClickHandler
    {
        public int indexInPriorities;
        public string[] priorityOptions;
        public TextMeshProUGUI buttonText;

        private SinglePriorityHolder currentHolder;
        public void SetBasedOnPriorityHolder(SinglePriorityHolder holder)
        {
            currentHolder = holder;
            SetTextBasedOnCurrentPriority();
        }

        private void SetTextBasedOnCurrentPriority()
        {
            buttonText.text = priorityOptions[currentHolder.priorities[indexInPriorities]];
        }

        private void TryIncrementPriority()
        {
            var nextPriority = Math.Min(priorityOptions.Length - 1, currentHolder.priorities[indexInPriorities] + 1);
            currentHolder.priorities[indexInPriorities] = nextPriority;
            SetTextBasedOnCurrentPriority();
        }
        private void TryDecrementPriority()
        {
            var nextPriority = Math.Max(0, currentHolder.priorities[indexInPriorities] - 1);
            currentHolder.priorities[indexInPriorities] = nextPriority;
            SetTextBasedOnCurrentPriority();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                TryIncrementPriority();
                Debug.Log("Left click");
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                TryDecrementPriority();
                Debug.Log("Right click");
            }
        }
    }
}
