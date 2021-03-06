﻿using UnityEngine;

namespace Assets.Scripts.Core
{
    [CreateAssetMenu(fileName = "FloatState", menuName = "State/FloatState", order = 2)]
    public class FloatState : GenericState<float>
    {
        public float defaultState;
        public override GenericVariable<float> GenerateNewVariable()
        {
            var instanced = CreateInstance<FloatVariable>();
            instanced.SetValue(defaultState);
            return instanced;
        }

        public override object GetSaveObjectFromVariable(GenericVariable<float> variable)
        {
            return variable.CurrentValue;
        }

        public override void SetSaveObjectIntoVariable(GenericVariable<float> variable, object savedValue)
        {
            var saveValue = (float)savedValue;
            variable.SetValue(saveValue);
        }
    }
}
