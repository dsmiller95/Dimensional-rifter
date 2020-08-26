using Assets.Scripts.Core;
using UniRx;
using UnityEngine;


namespace Assets.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SwapSpriteColor : MonoBehaviour
    {
        public Color colorIfTrue;
        public Color colorIfFalse;
        public BooleanReference colorSwitch;

        public void Awake()
        {
            SetSpriteColor(colorSwitch.CurrentValue);
            colorSwitch.ValueChanges.Subscribe(next => SetSpriteColor(next));
        }

        private void SetSpriteColor(bool boolValue)
        {
            GetComponent<SpriteRenderer>().color = boolValue ? colorIfTrue : colorIfFalse;
        }
    }
}
