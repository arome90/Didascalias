using UnityEngine;

namespace Didascalia.StateMachine
{
    public class State : ScriptableObject
    {
        protected StateMachine _machine;

        [SerializeField]
        private StateBehaviour[] _behaviours;

        [SerializeField]
        private Transition[] _transitions;

        [HideInInspector]
        public Transition[] Transitions;

        public virtual void Initialize(StateMachine machine)
        {
            int i = 0;
            foreach(StateBehaviour behaviour in _behaviours)
            {
                _behaviours[i] = Instantiate(behaviour);
                _behaviours[i++].Initialize(machine);
            }

            Transitions = new Transition[_transitions.Length];

            i = 0;
            foreach (Transition transition in _transitions) {
                Transitions[i] = Instantiate(transition);
                Transitions[i++].Initialize(machine);
            }
        }

        public virtual void OnEnter() {
            // ...
        }
        public virtual void Update() {
            foreach (StateBehaviour behaviour in _behaviours) {
                behaviour.Update();
            }
        }
        public virtual void OnExit() { }
    }
}