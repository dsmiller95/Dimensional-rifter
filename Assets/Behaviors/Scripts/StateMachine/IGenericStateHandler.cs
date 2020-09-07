namespace Assets.Behaviors.Scripts
{
    public interface IGenericStateHandler<in ParamType>
    {
        IGenericStateHandler<ParamType> HandleState(ParamType data);
        void TransitionIntoState(ParamType data);
        void TransitionOutOfState(ParamType data);
    }
}