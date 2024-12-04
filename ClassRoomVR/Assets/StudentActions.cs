using BehaviorDesigner.Runtime.Tasks.Unity.Math;
using MathNet.Numerics.Distributions;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Acciones no disruptivas
    /// </summary>
    public class StudentActions : MonoBehaviour
    {
        private Student student;
        private Vector3 studentTr;
        private Animator animator;
        private const float BlinkIntervalMin = 2f;
        private const float BlinkIntervalMax = 3f;
        private SkinnedMeshRenderer _meshRenderer;
        private float[] _blendShapeWeights;
        private const int SkinnedMeshIndex = 5;
        EventSittingAnimations sittingAnim;
        [SerializeField]
        private GameObject phone;
        private void Start()
        {
            studentTr =GetComponent<Transform>().transform.position;
            student = GetComponent<Student>();
            animator = GetComponent<Animator>();
            StartCoroutine(RandomBlink());
            _blendShapeWeights = new float[6];
            sittingAnim = EventSittingAnimations.None;
            StartCoroutine(CallLineAfterDelay());
            phone.SetActive(false);

            //REFACTOR
            //props.GetCharacterProps().BoneAttachments[1].Complements[0].mesh.GameObject().SetActive(false);
            _meshRenderer = transform.GetChild(SkinnedMeshIndex).GetComponent<SkinnedMeshRenderer>();
            SetBlendShape(Expressions.Sleep, 0);

        }
        /// <summary>
        /// Espera un segundo para obtener el componente SkinnedMeshRenderer.
        /// </summary>
        private IEnumerator CallLineAfterDelay()
        {
            yield return new WaitForSeconds(1f);
           // _meshRenderer = transform.GetChild(SkinnedMeshIndex).GetComponent<SkinnedMeshRenderer>();
        }
        /// <summary>
        /// 
        /// </summary>

        /// <param name="onComplete">Acción a ejecutar al finalizar la corrutina.</param>
        /// <returns>Retorna un IEnumerator necesario para las corrutinas.</returns>
        public void PlaySitAction(EventSittingAnimations anim)
        {
            sittingAnim = anim;

            phone.SetActive(false);
            SetBlendShape(Expressions.Sleep, 0);
            StartCoroutine(RandomBlink());
            //GetComponent<Transform>().SetPositionAndRotation(studentTr, Quaternion.identity);
            Debug.Log(gameObject.name + " " + anim.ToString());
            switch (anim)
            {
                case EventSittingAnimations.None:
                    {
                        animator.SetInteger("Action", -1);
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk);
                        sittingAnim = EventSittingAnimations.None;

                        break;
                    }
                case EventSittingAnimations.Yelling:
                    {
                        animator.SetInteger("Action", (int)anim);
                        break;
                    }
                case EventSittingAnimations.RiseHand:
                    {
                        animator.SetInteger("Action", (int)anim);
                        break;
                    }
                case EventSittingAnimations.PlayingPhone:
                    {
                        animator.SetInteger("Action", (int)anim);
                        phone.SetActive(true);
                        break;
                    }
                case EventSittingAnimations.Swinging:
                    {
                        animator.SetInteger("Action", (int)anim); 
                      
                        break;
                    }
                case EventSittingAnimations.Sleeping:
                    {
                        animator.SetInteger("Action", (int)anim);
                        SetBlendShape(Expressions.Sleep, 100f);

                        break;
                    }
                case EventSittingAnimations.Attending:
                    {
                        animator.SetInteger("Action", (int)anim);
                        break;
                    }
            }


        }

        public int getAction()
        {
            return (int)sittingAnim;
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