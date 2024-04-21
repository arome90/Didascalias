//using Newtonsoft.Json.Linq;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace ClassRoomVR
//{
//    [System.Serializable]

//    public class BlendCustom : MonoBehaviour
//    {
//        SkinnedMeshRenderer meshRenderer;
//        public enum Expresiones { LLorar, Dormido, Sonreir, Quejarse, Enfadado, Pestañear }
//        private float[] list;

//        void Start()
//        {
//            meshRenderer = transform.GetChild(5).GetComponent<SkinnedMeshRenderer>();
//            list = new float[6];

//            InvokeRepeating(nameof(SetPestañeo), 2, 4);
//        }


//        public void SetPestañeo()
//        {
//            SetBlendShape(Expresiones.Pestañear, 100);
//            Invoke(nameof(SetAbrirOjos), 0.2f);

//        }
//        public void SetAbrirOjos()
//        {
//            SetBlendShape(Expresiones.Pestañear, 0);

//        }
//        public void SetBlendShape(Expresiones expresion, float value)
//        {
//            meshRenderer.SetBlendShapeWeight((int)expresion, value);
//        }

//        public void SetBlendShape(int expresion, float value)
//        {
//            meshRenderer.SetBlendShapeWeight(expresion, value);
//        }

//        private void Update()
//        {

//            if (Input.GetKeyDown(KeyCode.C))
//            {
//                StopAllCoroutines();
//                StartCoroutine(SetExpression(Expresiones.Enfadado));
//            }
//            if (Input.GetKeyDown(KeyCode.V))
//            {
//                StopAllCoroutines();
//                StartCoroutine(SetExpression(Expresiones.Quejarse));
//            }
//            if (Input.GetKeyDown(KeyCode.B))
//            {
//                StopAllCoroutines();
//                StartCoroutine(SetExpression(Expresiones.Sonreir));
//            }
//            if (Input.GetKeyDown(KeyCode.N))
//            {
//                StopAllCoroutines();
//                StartCoroutine(SetExpression(Expresiones.Dormido));
//            }
//            if (Input.GetKeyDown(KeyCode.M))
//            {
//                StopAllCoroutines();
//                StartCoroutine(SetExpression(Expresiones.LLorar));
//            }

//        }


//        //public void SetExpression(int expresion) 
//        //{
//        //   StartCoroutine(SetExpression((int)Expresiones.LLorar));

//        //}
//        public IEnumerator SetExpression(Expresiones exp)
//        {
//            while (meshRenderer.GetBlendShapeWeight((int)exp) != 100)
//            {
//                for (int i = 0; i < list.Length; i++)
//                {
//                    if ((int)exp == i)
//                    {
//                        list[i] = Mathf.Min(100, list[i] + 15);
//                        SetBlendShape(exp, list[i]);
//                    }
//                    else if (meshRenderer.GetBlendShapeWeight(i) > 0)
//                    {
//                        list[i] = Mathf.Max(0, list[i] - 20);
//                        SetBlendShape(i, list[i]);
//                    }
//                }
//                yield return new WaitForSeconds(0.2f);
//            }

//        }


//    }
//}
