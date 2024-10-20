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
        private const float BlinkIntervalMin = 2f;
        private const float BlinkIntervalMax = 3f;
        private SkinnedMeshRenderer _meshRenderer;
        private float[] _blendShapeWeights;
        private const int SkinnedMeshIndex = 5;
        EventSittingAnimations sittingAnim;
        private void Start()
        {
            student = GetComponent<Student>();
            animator = GetComponent<Animator>();
            StartCoroutine(RandomBlink());
            _blendShapeWeights = new float[6];
            sittingAnim = EventSittingAnimations.None;
            StartCoroutine(CallLineAfterDelay());

        }
        /// <summary>
        /// Espera un segundo para obtener el componente SkinnedMeshRenderer.
        /// </summary>
        private IEnumerator CallLineAfterDelay()
        {
            yield return new WaitForSeconds(1f);
            _meshRenderer = transform.GetChild(SkinnedMeshIndex).GetComponent<SkinnedMeshRenderer>();
        }
        /// <summary>
        /// 
        /// </summary>

        /// <param name="onComplete">Acción a ejecutar al finalizar la corrutina.</param>
        /// <returns>Retorna un IEnumerator necesario para las corrutinas.</returns>
        public IEnumerator PlaySitAction(EventSittingAnimations anim)
        {
            sittingAnim = anim;

            switch (anim)
            {
                case EventSittingAnimations.Yelling:
                    {
                        animator.SetInteger("Action", (int)anim);

                        yield return new WaitForSeconds(5.0f);

                        animator.SetInteger("Action", -1);
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk);
                        sittingAnim = EventSittingAnimations.None;
                        break;
                    }
                case EventSittingAnimations.RiseHand:
                    {
                        animator.SetInteger("Action", (int)anim);

                        //  yield return new WaitForSeconds(Random.Range(0.0f, 5.0f));

                        //  student.GenerateText($"Profe, una duda");

                        yield return new WaitForSeconds(Random.Range(10.0f, 20.0f));

                        animator.SetInteger("Action", -1);
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk);
                        sittingAnim = EventSittingAnimations.None;
                        break;
                    }
                case EventSittingAnimations.PlayingPhone:
                    {
                        animator.SetInteger("Action", (int)anim);

                        yield return new WaitForSeconds(Random.Range(10.0f, 20.0f));

                        animator.SetInteger("Action", -1);
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk);
                        sittingAnim = EventSittingAnimations.None;
                        break;
                    }
                case EventSittingAnimations.Swinging:
                    {
                        animator.SetInteger("Action", (int)anim);
                      
                        yield return new WaitForSeconds(Random.Range(10.0f, 20.0f));

                        animator.SetInteger("Action", -1);
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk);
                        sittingAnim = EventSittingAnimations.None;
                        break;
                    }
                case EventSittingAnimations.Sleeping:
                    {
                        animator.SetInteger("Action", (int)anim);
                        SetBlendShape(Expressions.CloseEyes, 100f);
                        yield return new WaitForSeconds(Random.Range(10.0f, 20.0f));

                        animator.SetInteger("Action", -1);
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk);
                        sittingAnim = EventSittingAnimations.None;
                        SetBlendShape(Expressions.CloseEyes, 0f);
                        StartCoroutine(RandomBlink());
                        break;
                    }
                case EventSittingAnimations.Attending:
                    {
                        animator.SetInteger("Action", (int)anim);

                        yield return new WaitForSeconds(Random.Range(10.0f, 20.0f));

                        animator.SetInteger("Action", -1);
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk);
                        sittingAnim = EventSittingAnimations.None;
                        break;
                    }
            }


        }


        /// <summary>
        /// Maneja el parpadeo aleatorio del estudiante.
        /// </summary>
        private IEnumerator RandomBlink()
        {
            while (sittingAnim!= EventSittingAnimations.Sleeping)
            {
                yield return Blink(Expressions.CloseEyes);
                yield return new WaitForSeconds(Random.Range(BlinkIntervalMin, BlinkIntervalMax));
            }
        }

        /// <summary>
        /// Ejecuta un parpadeo.
        /// </summary>
        /// <param name="expresion">Expresión de parpadeo.</param>
        private IEnumerator Blink(Expressions expresion)
        {
            SetBlendShape(expresion, 100f);
            yield return new WaitForSeconds(0.3f);
            SetBlendShape(expresion, 0f);
        }

        /// <summary>
        /// Establece el peso de una forma de mezcla en el renderizador.
        /// </summary>
        /// <param name="expresion">Expresión de la forma de mezcla.</param>
        /// <param name="value">Valor del peso.</param>
        public void SetBlendShape(Expressions expresion, float value)
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.SetBlendShapeWeight((int)expresion, value);
            }
        }

        /// <summary>
        /// Cambia la expresión del estudiante suavemente.
        /// </summary>
        /// <param name="exp">Expresión a cambiar.</param>
        private IEnumerator ChangeExpression(Expressions exp)
        {
            int expressionIndex = (int)exp;
            while (_meshRenderer.GetBlendShapeWeight(expressionIndex) < 100f)
            {
                for (int i = 0; i < _blendShapeWeights.Length; i++)
                {
                    float changeValue = i == expressionIndex ? 15f : -20f;
                    _blendShapeWeights[i] = Mathf.Clamp(_blendShapeWeights[i] + changeValue, 0f, 100f);
                    _meshRenderer.SetBlendShapeWeight(i, _blendShapeWeights[i]);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}