using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
namespace ClassRoomVR {
    public class SelectAimTarget : MonoBehaviour
    {
        void Awake()
        {
                changeAimTransform();
        }

        //Metodo para la asignacion del objetivo
        private void changeAimTransform() {
            _playerAimTarget = GameManager.Instance._sceneManager._aimNoVR;    // Asignacion de transforms para la elección
            _VRPlayerAImTarget = GameManager.Instance._sceneManager._aimVR;

            if (!GameManager.Instance.getVR())                                 // Caso NO VR
            {
                var data = _ownAiming.data.sourceObjects;
                data.SetTransform(0, _playerAimTarget.transform);              // Asignamos el transform a sourceObject
                data.SetWeight(0, 1.0f);                                       // Asignamos a 1 el peso del constraint para hacerlo efectivo.
                _ownAiming.data.sourceObjects = data;                          // Asignamos el sourceObject al componente Multi Aim Constraint
            }
            else                                                               // Caso VR
            {
                var data = _ownAiming.data.sourceObjects;
                data.SetTransform(0, _VRPlayerAImTarget.transform);
                data.SetWeight(0, 1.0f);
                _ownAiming.data.sourceObjects = data;
            }
            
            _builder.Build();
        }
        private void OnDestroy() //Al destruir el objeto, limpiamos los sourceObjects
        {
            var data = _ownAiming.data.sourceObjects;
            data.Clear();
            _ownAiming.data.sourceObjects = data;
        }

        public GameObject _playerAimTarget; //Objetivo a mirar en no escenas NO VR
        public GameObject _VRPlayerAImTarget; //Objetivo a mirar en no escenas VR
        public MultiAimConstraint _ownAiming; //Componente a editar para cambiar el transform
        public RigBuilder _builder;  //Componente que debe reiniciarse tras el cambio de objetivo
    }
}