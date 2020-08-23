namespace Assets.Behaviors
{
    public interface IGenericStateHandler<ParamType>
    {
        IGenericStateHandler<ParamType> HandleState(ParamType data);
        void TransitionIntoState(ParamType data);
        void TransitionOutOfState(ParamType data);
    }
}