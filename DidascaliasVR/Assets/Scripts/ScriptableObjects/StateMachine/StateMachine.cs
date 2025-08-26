using TMPro;
using UnityEngine;

namespace Didascalia.StateMachine
{
    public class StateMachine : MonoBehaviour
    {
        [SerializeField]
        State _initialState = null;

        State _currentState = null;

        [SerializeField]
        TextMeshProUGUI _debugText = null;

        private void Start()
        {
            if(!_initialState) { Debug.LogError("StateMachine could not be started due to initial state being null!");  }

            _currentState = Instantiate(_initialState);

            _currentState.Initialize(this);
            _currentState.OnEnter();

            _debugText.text = _currentState.name;
        }

        private void Update()
        {
            if(_currentState)
            {
                _currentState.Update();

                foreach(Transition trans in _currentState.Transitions)
                {
                    if (CheckTransition(trans)) break;
                }
            }
        }

        private bool CheckTransition(Transition trans)
        {
            if (trans.Check())
            {
                trans.OnCheck();
                _currentState.OnExit();
                _currentState = Instantiate(trans.NextState);
                _currentState.Initialize(this);
                _currentState.OnEnter();
                _debugText.text = _currentState.name;
                return true;
            }
            else return false;
        }
    }
}
