using Assets.WorldObjects;

namespace Assets.Behaviors.HungryStates
{
    public class HungryDecider : GenericStateHandler<Hungry>
    {
        public float hungerThreshold = 10f;

        public GenericStateHandler<Hungry> HandleState(Hungry data)
        {
            var timeZone = TimeController.instance.GetTimezone();
            switch (timeZone)
            {
                case Timezone.Day:
                    return HandleWorking(data);
                case Timezone.Evening:
                case Timezone.Night:
                    return HandleRecreation(data);
                default:
                    return HandleWorking(data);
            }
        }

        private GenericStateHandler<Hungry> HandleRecreation(Hungry data)
        {
            if (data.currentHunger >= hungerThreshold)
            {
                return new Eating();
            }
            return HandleWorking(data);
        }

        private GenericStateHandler<Hungry> HandleWorking(Hungry data)
        {
            return new Foraging();
        }

        public void TransitionIntoState(Hungry data)
        {
        }

        public void TransitionOutOfState(Hungry data)
        {
        }
    }
}
