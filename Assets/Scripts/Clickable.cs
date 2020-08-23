using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class Clickable : MonoBehaviour
    {
        public UnityEvent whenClicked;

        private void OnMouseDown()
        {
            Debug.Log($"Clicked: {gameObject.name}");

            whenClicked.Invoke();
        }
    }
}
