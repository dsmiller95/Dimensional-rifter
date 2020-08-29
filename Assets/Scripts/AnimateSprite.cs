using Assets.Scripts.Core;
using UniRx;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AnimateSprite : MonoBehaviour
    {
        public Sprite[] spriteSequence;
        public FloatReference animateValue;

        public float amountPerSprite;

        public void Awake()
        {
            SetSprite(animateValue.CurrentValue);
            animateValue.ValueChanges.Subscribe(next => SetSprite(next));
        }

        private void SetSprite(float spriteSelection)
        {
            var spriteIndex = (int)(spriteSelection / amountPerSprite) % spriteSequence.Length;

            GetComponent<SpriteRenderer>().sprite = spriteSequence[spriteIndex];
        }
    }
}
