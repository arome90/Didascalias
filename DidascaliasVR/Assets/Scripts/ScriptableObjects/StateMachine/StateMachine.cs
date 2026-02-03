using System.Collections.Generic;
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

        /// <summary>
        /// Diccionario donde poder guardar valores relevantes para el funcionamiento de los estados
        /// En este caso estamos guardando componentes y referencias a otras entidades
        /// TODO: Ampliarlo para poder añadir otro tipo de valores (float, int, bool, etc)
        /// </summary>
        Dictionary<string, MonoBehaviour> _data;

        private void Start()
        {
            _data = new Dictionary<string, MonoBehaviour>();

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

        public void AddData(string id, MonoBehaviour data)
        {
            _data.Add(id, data);
        }

        public MonoBehaviour GetData(string id)
        {
            return _data.TryGetValue(id, out MonoBehaviour data) ? data : null;
        }

        public void RemoveData(string id)
        {
            _data.Remove(id);
        }

        private bool CheckTransition(Transition trans)
        {
            if (trans.Check())
            {
                Debug.Log(trans.name + " just validated, now entering: " + trans.NextState.name);
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
