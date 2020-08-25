using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Core
{
    [Serializable]
    public struct BooleanInstanceConfig
    {
        public string name;
        public bool defaultValue;
    }

    public class VariableInstantiator: MonoBehaviour
    {
        public BooleanInstanceConfig[] variableInstancingConfig;

        public IDictionary<string, BooleanVariable> instancedVariables;

        private void Awake()
        {
            if(instancedVariables == null)
            {
                this.instantiateVariables();
            }
        }

        private void instantiateVariables()
        {
            instancedVariables = variableInstancingConfig.ToDictionary(x => x.name, x =>
            {
                var instanced = ScriptableObject.CreateInstance<BooleanVariable>();
                instanced.SetValue(x.defaultValue);
                return instanced;
            });
        }

        public BooleanVariable GetValue(string name)
        {
            if (instancedVariables == null)
            {
                this.instantiateVariables();
            }
            if(instancedVariables.TryGetValue(name, out var variable))
            {
                return variable;
            }
            return null;
        }
    }
}
