using BehaviorDesigner.Runtime.Tasks.Unity.Math;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityRigidbody;
using MathNet.Numerics.Distributions;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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

        private void Start()
        {
            studentTr = GetComponent<Transform>().transform.position;
            student = GetComponent<Student>();
            animator = GetComponent<Animator>();
            StartCoroutine(RandomBlink());
            _blendShapeWeights = new float[6];
            sittingAnim = EventSittingAnimations.None;
            StartCoroutine(CallLineAfterDelay());

            //REFACTOR
            //props.GetCharacterProps().BoneAttachments[1].Complements[0].mesh.GameObject().SetActive(false);
            _meshRenderer = transform.GetChild(SkinnedMeshIndex).GetComponent<SkinnedMeshRenderer>();
            SetBlendShape(Expressions.Sleep, 0);
            StartCoroutine(RandomBlink());
            SetFacialEmotion();

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
            if (sittingAnim == EventSittingAnimations.Sleeping) StartCoroutine(RandomBlink());
            sittingAnim = anim;
            SetFacialEmotion();

            //SetBlendShape(Expressions.Sleep, 0);

            //GetComponent<Transform>().SetPositionAndRotation(studentTr, Quaternion.identity);
            //Debug.Log(gameObject.name + " " + anim.ToString());
            switch (anim)
            {
                case EventSittingAnimations.None:
                    {
                        animator.SetInteger("Action", -1);
                        animator.SetInteger("SittingRandomAction", (int)NormalSittingAnimations.SitHandsOnDesk);
                        sittingAnim = EventSittingAnimations.None;
                        ClearExpression();
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
                        break;
                    }
                case EventSittingAnimations.ConstantMoving:
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
                case EventSittingAnimations.Attending2:
                    {
                        animator.SetInteger("Action", (int)anim);
                        break;
                    }
                case EventSittingAnimations.Bored:
                    {
                        animator.SetInteger("Action", (int)anim);
                        break;
                    }
                case EventSittingAnimations.Drawing:
                    {
                        animator.SetInteger("Action", (int)anim);
                        break;
                    }
                case EventSittingAnimations.Terrified:
                    {
                        animator.SetInteger("Action", (int)anim);
                        break;
                    }
            }


        }



        public int GetAction()
        {
            return (int)sittingAnim;
        }
        public void SetFacialEmotion()
        {
            float[] emotions = student.GetEmotions();
            float max = emotions.Max();

            int index = Array.IndexOf(emotions, emotions.First(x => Math.Abs(x) == Math.Abs(max)));
            if (index == 0)
            {
                if (max < 0) SetBlendShape(Expressions.Cry, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));
                else SetBlendShape(Expressions.Smile, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));
            }
            else if (index == 1)
            {
                if (max < 0) SetBlendShape(Expressions.Bored, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));
                else SetBlendShape(Expressions.Smile, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));

            }
            else if (index == 2)
            {
                if (max < 0) SetBlendShape(Expressions.Angry, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));
                else SetBlendShape(Expressions.Smile, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));

            }
            else if (index == 3)
            {
                if (max < 0) SetBlendShape(Expressions.Bored, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));
                else SetBlendShape(Expressions.Smile, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));

            }
            else if (index == 4)
            {
                if (max < 0) SetBlendShape(Expressions.Cry, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));
                else SetBlendShape(Expressions.Smile, (float)Math.Clamp(Math.Abs(max) * 100f, 0, 100f));

            }



        }

        /// <summary>
        /// Maneja el parpadeo aleatorio del estudiante.
        /// </summary>
        public IEnumerator RandomBlink()
        {
            while (sittingAnim != EventSittingAnimations.Sleeping)
            {
                yield return Blink(Expressions.CloseEyes);
                yield return new WaitForSeconds(UnityEngine.Random.Range(BlinkIntervalMin, BlinkIntervalMax));
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

        public void ClearExpression()
        {
            for (int i = (int)Expressions.Sleep; i < (int)Expressions.EXPRESSIONS_SIZE; i++)
            {
                SetBlendShape((Expressions)i, 0);
            }
        }

        /// <summary>
        /// Cambia la expresión del estudiante suavemente.
        /// </summary>
        /// <param name="exp">Expresión a cambiar.</param>
        public IEnumerator ChangeExpression(Expressions exp)
        {
            int expressionIndex = (int)exp;
            while (_meshRenderer.GetBlendShapeWeight(expressionIndex) < 100f)
            {
                for (int i = 0; i < (int)Expressions.EXPRESSIONS_SIZE; i++)
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