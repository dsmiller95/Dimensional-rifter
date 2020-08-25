using System;
using UniRx;

namespace Assets.Scripts.Core
{
    public enum BooleanReferenceDataSource
    {
        CONSTANT,
        SINGLETON_VARIABLE,
        INSTANCER
    }

    [Serializable]
    public class BooleanReference
    {
        /// <summary>
        /// Warning: only change this in the inspector. the ValueChanges subscribtion currently will break otherwise
        /// </summary>
        public BooleanReferenceDataSource DataSource = BooleanReferenceDataSource.CONSTANT;
        public bool ConstantValue;
        public BooleanVariable Variable;

        public VariableInstantiator Instancer;
        public string NamePath;


        public BooleanReference(bool value)
        {
            DataSource = BooleanReferenceDataSource.CONSTANT;
            ConstantValue = value;
        }

        public bool CurrentValue
        {
            get
            {
                switch (DataSource)
                {
                    case BooleanReferenceDataSource.CONSTANT:
                        return ConstantValue;
                    case BooleanReferenceDataSource.SINGLETON_VARIABLE:
                        return Variable.CurrentValue;
                    case BooleanReferenceDataSource.INSTANCER:
                        return Instancer.GetValue(NamePath).CurrentValue;
                    default:
                        return false;
                }
            }
        }

        public IObservable<bool> ValueChanges
        {
            get
            {
                switch (DataSource)
                {
                    case BooleanReferenceDataSource.CONSTANT:
                        return Observable.Return(ConstantValue);
                    case BooleanReferenceDataSource.SINGLETON_VARIABLE:
                        return Variable.Value;
                    case BooleanReferenceDataSource.INSTANCER:
                        return Instancer.GetValue(NamePath).Value;
                    default:
                        throw new Exception("Invalid data source");
                }
            }
        }

        public void SetValue(bool v)
        {
            switch (DataSource)
            {
                case BooleanReferenceDataSource.CONSTANT:
                    ConstantValue = v;
                    break;
                case BooleanReferenceDataSource.SINGLETON_VARIABLE:
                    Variable.SetValue(v);
                    break;
                case BooleanReferenceDataSource.INSTANCER:
                    Instancer.GetValue(NamePath).SetValue(v);
                    break;
                default:
                    throw new Exception("Invalid data source");
            }
        }
    }
}
