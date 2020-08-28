using Assets.Scripts.Core;
using UniRx;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SwapSprite : MonoBehaviour
    {
        public Sprite spriteIfTrue;
        public Sprite spriteIfFalse;
        public BooleanReference spriteSwitch;

        public void Awake()
        {
            SetSprite(spriteSwitch.CurrentValue);
            spriteSwitch.ValueChanges.Subscribe(next => SetSprite(next));
        }

        private void SetSprite(bool boolValue)
        {
            GetComponent<SpriteRenderer>().sprite = boolValue ? spriteIfTrue : spriteIfFalse;
        }
    }
}
