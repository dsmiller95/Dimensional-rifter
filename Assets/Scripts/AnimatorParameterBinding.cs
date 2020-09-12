using Assets.Scripts.Core;
using System;
using UniRx;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorParameterBinding : MonoBehaviour
    {
        [Serializable]
        public struct BoolReferenceAnimatorBinding
        {
            public BooleanReference boolReference;
            public string booleanParameterName;
        }

        public BoolReferenceAnimatorBinding[] bindings;

        private Animator animator;

        public void Awake()
        {
            animator = GetComponent<Animator>();
            foreach (var binding in bindings)
            {
                binding.boolReference.ValueChanges
                    .TakeUntilDisable(this)
                    .Subscribe(newValue =>
                    {
                        animator.SetBool(binding.booleanParameterName, newValue);
                    }).AddTo(this);
            }
        }
    }
}
