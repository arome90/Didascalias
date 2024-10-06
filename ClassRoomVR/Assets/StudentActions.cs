using BehaviorDesigner.Runtime.Tasks.Unity.Math;
using MathNet.Numerics.Distributions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Acciones no disruptivas
    /// </summary>
    public class StudentActions : MonoBehaviour
    {
        private Student student;
        private Animator animator;

        private void Start()
        {
            student = GetComponent<Student>();
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 
        /// </summary>
   
        /// <param name="onComplete">Acción a ejecutar al finalizar la corrutina.</param>
        /// <returns>Retorna un IEnumerator necesario para las corrutinas.</returns>
        public IEnumerator PlaySitAction(EventSittingAnimations anim)
        {

            switch (anim)
            {
                case EventSittingAnimations.RiseHand:
                    {
                        animator.SetInteger("Action", (int)anim);
             
                        yield return new WaitForSeconds(Random.Range(0.0f, 5.0f));

                        student.GenerateText($"Profe, una duda");

                        yield return new WaitForSeconds(Random.Range(5.0f, 10.0f)); 

                        animator.SetInteger("Action",-1 );
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk );

                        break;
                    }
            }
         

        }
    }
}