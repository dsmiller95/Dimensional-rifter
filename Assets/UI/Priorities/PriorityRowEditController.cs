using TMPro;
using UnityEngine;

namespace Assets.UI.Priorities
{

    public class PriorityRowEditController : MonoBehaviour
    {
        public SinglePriorityHolder priorityHolderToEdit;
        public PriorityHolderSet prioritySet;

        public TextMeshProUGUI nameText;


        private void Awake()
        {
        }

        public void ConnectButtons()
        {
            var buttons = GetComponentsInChildren<PriorityButton>();
            for (int i = 0; i < buttons.Length; i++)
            {
                var butt = buttons[i];
                butt.indexInPriorities = i;
                butt.SetBasedOnPriorityHolder(priorityHolderToEdit);
            }
        }

        public static PriorityRowEditController CreateAsChildOf(
            Transform parent,
            GameObject prefab,
            SinglePriorityHolder myPriorityHolder,
            PriorityHolderSet set)
        {
            var newObj = Instantiate(prefab, parent);
            var editController = newObj.GetComponentInChildren<PriorityRowEditController>();
            editController.priorityHolderToEdit = myPriorityHolder;
            editController.prioritySet = set;
            editController.ConnectButtons();

            editController.nameText.text = myPriorityHolder.priorityHolderName;

            return editController;
        }
    }
}
