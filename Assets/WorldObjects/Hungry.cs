using Assets.Behaviors;
using Assets.Behaviors.HungryStates;
using UnityEngine;

namespace Assets.WorldObjects
{
    [RequireComponent(typeof(TileMapNavigationMember))]
    public class Hungry : MonoBehaviour
    {
        public float hungeringRate = .1f;
        public float currentHunger = 0;

        StateMachine<Hungry> stateMachine;
        private void Start()
        {
            stateMachine = new StateMachine<Hungry>(new Foraging());
        }

        private void Update()
        {
            stateMachine.update(this);
            currentHunger += Time.deltaTime * hungeringRate;
        }
    }
}
