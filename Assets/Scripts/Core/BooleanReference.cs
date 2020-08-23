using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Assets.Scripts.Core
{
    [Serializable]
    public class BooleanReference
    {
        /// <summary>
        /// Warning: only change this in the inspector. the ValueChanges subscribtion currently will break otherwise
        /// </summary>
        public bool UseConstant = true;
        public bool ConstantValue;
        public BooleanVariable Variable;
        

        public BooleanReference(bool value)
        {
            UseConstant = true;
            ConstantValue = value;
        }

        public bool CurrentValue
        {
            get => UseConstant ? ConstantValue : Variable.CurrentValue;
        }

        public IObservable<bool> ValueChanges
        {
            get => UseConstant ? Observable.Return(ConstantValue) : Variable.Value;
        }
    }
}
