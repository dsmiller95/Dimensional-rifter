using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public abstract class GenericSelector<T>: ScriptableObject
    {
        public abstract T GetCurrentValue(VariableInstantiator instancer);
        public abstract IObservable<T> ValueChanges(VariableInstantiator instancer);
    }
}
