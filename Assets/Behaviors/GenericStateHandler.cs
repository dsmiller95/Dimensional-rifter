namespace Assets.Behaviors
{
    public interface GenericStateHandler<ParamType>
    {
        GenericStateHandler<ParamType> HandleState(ParamType data);
        void TransitionIntoState(ParamType data);
        void TransitionOutOfState(ParamType data);
    }
}