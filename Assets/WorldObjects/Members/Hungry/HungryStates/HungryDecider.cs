using Assets.Behaviors;

namespace Assets.WorldObjects.Members.Hungry.HungryStates
{
    public class HungryDecider : IGenericStateHandler<Hungry>
    {
        public float hungerThreshold = 10f;

        public IGenericStateHandler<Hungry> HandleState(Hungry data)
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

        private IGenericStateHandler<Hungry> HandleRecreation(Hungry data)
        {
            if (data.currentHunger >= hungerThreshold)
            {
                return new Eating();
            }
            return HandleWorking(data);
        }

        private IGenericStateHandler<Hungry> HandleWorking(Hungry data)
        {
            var currentFood = data.GetComponent<ResourceInventory>().inventory.Get(Resource.FOOD);
            if (currentFood > 0)
            {
                return new Storing();
            }
            else
            {
                return new Foraging();
            }
        }

        public void TransitionIntoState(Hungry data)
        {
        }

        public void TransitionOutOfState(Hungry data)
        {
        }
    }
}
