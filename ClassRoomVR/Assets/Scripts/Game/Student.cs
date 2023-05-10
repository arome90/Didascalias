using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Linq;

namespace ClassRoomVR
{
    [System.Serializable]
    public class Student : MonoBehaviour
    {
        [System.Flags]
        public enum Vision
        {
             Arriba = 1, Derecha = 2, 
            Abajo = 4, Izquierda = 8, Ventana = 16,  Puerta = 32, Profesor = 64
        };
        Vision vision;
        public Vision distracted;
        Vision[] distractedArray;

        public enum Gender { Women, Men };
        private enum State { Sit, Stand };
        State state;
        [SerializeField] Gender gender;
        [SerializeField] string name;
        [SerializeField] bool problematic = false;
        int problematicPercentage = 10;
        [SerializeField] TextMesh text;
        Vector3 deskPosition;

        [SerializeField]
        private RuntimeAnimatorController controller;

        Animator animator;
        AudioSource audio;
        NavMeshAgent agent;
        Collider collider;
        Vector3 dest;

        [SerializeField] Transform target;
        Vector3 targetPositionActual;
        Dictionary<Vision,Vector3> targets;
       
        [SerializeField] MultiAimConstraint head;

        //de momento 
        [SerializeField]
        TextMesh atencion;

        StudentBehavior behavior;

        Transform professor;


        private void Start()
        {
            audio = GetComponent<AudioSource>();
            agent = GetComponent<NavMeshAgent>();
            behavior = GetComponent<StudentBehavior>();
            state = State.Sit;
            professor = Camera.main.transform;
            distractedArray= System.Enum.GetValues(typeof(Vision)).Cast<Vision>()
                   .Where(c => (distracted & c) == c)    // or use HasFlag in .NET4
                   .ToArray();
           


        }
        public void SetParameters(string na, Gender s)
        {
            
            name = na;
            transform.name = na;
            text.text = na;
            gender = s;

            
        }

        public void CreateBody(GameObject obj)
        {
            GameObject body = Instantiate(obj, transform);
            body.AddComponent<MeshCollider>();

            animator = body.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = controller;
            }
            collider = transform.GetChild(transform.childCount - 1).GetComponent<Collider>();
            
            head.data.constrainedObject = getHeadBone();
            transform.GetComponent<RigBuilder>().Build();
        }




        //TODO: cambiar esto al meter nuevos prefabs
        private Transform getHeadBone()
        {
            Transform body = transform.GetChild(2);
            int i = body.childCount - 3;
            return body.GetChild(i).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(0);
        }

        public void SetProblematicStudent()
        {
            text.color = Color.red;
            problematic = true;
        }




        public void SetDesk(Vector3 pos)
        {
            deskPosition = pos;
        }

        public Vector3 GetDesk() { return deskPosition; }
        public Gender GetSex() { return gender; }
        public string GetName() { return name; }
        public Collider GetCollider() { return collider; }
        public bool GetProblematicStudent() { return problematic; }
        public AudioSource getAudio() { return audio; }



        public void SetTargets(Transform[] tar)
        {
            targets = new Dictionary<Vision, Vector3>();
            targets.Add(Vision.Arriba,new Vector3(0, 3, 1));
            targets.Add(Vision.Derecha,Vector3.right);
            targets.Add(Vision.Abajo, new Vector3(0, -1, 1));
            targets.Add(Vision.Izquierda, Vector3.left);
            targets.Add(Vision.Ventana,tar[0].position);
            targets.Add(Vision.Puerta,tar[1].position);
            targets.Add(Vision.Profesor,Vector3.zero);

        }


        //En futuro implementar atencion al profesor jugando con source objects de multi aim
        public void PayAttention()
        {
            SetDirection(Vision.Profesor);
        }

        public void GetDistracted()
        {
            SetDirection(distractedArray[Random.Range(0,distractedArray.Length)]);
        }


        private void Update()
        {
            for (int i = 0; i < 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    Vision v = targets.ElementAt(i).Key;
                    Debug.Log(i +" "+ v);

                    SetDirection(v);
                 
                }

            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                PayAttention();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                GetDistracted();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6)) 
            {
                StartCoroutine(Asentir());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                StartCoroutine(Negar());
            }

            
            if (vision != Vision.Profesor)
            {
                target.position = Vector2.MoveTowards(target.position, targetPositionActual, 5.0f * Time.deltaTime);
            }
           // else target.position = professor.position;
            else target.position =  Vector2.MoveTowards(target.position, professor.position, 5.0f * Time.deltaTime); ;
            atencion.text = behavior.NivelAtencion.ToString("0.##");
        }


        IEnumerator  Asentir()
        {

            for (int i = 0; i < 2; i++)
            {
                SetDirection(Vision.Arriba);
                while (Vector2.Distance(target.position, targetPositionActual) > 0.05)
                    yield return null;
                SetDirection(Vision.Abajo);
                while (Vector2.Distance(target.position, targetPositionActual) > 0.05)
                    yield return null;
            }
            //while (Vector2.Distance(target.position, targetPositionActual) > 0.05)
            //    yield return null;
           // targetPositionActual = targets[0].localPosition;

        }


        IEnumerator Negar()
        {

            for (int i = 0; i < 2; i++)
            {
                SetDirection(Vision.Derecha);
                while (Vector2.Distance(target.position, targetPositionActual) > 0.05)
                    yield return null;
                SetDirection(Vision.Izquierda);
                while (Vector2.Distance(target.position, targetPositionActual) > 0.05)
                    yield return null;
            }
            //while (Vector2.Distance(target.position, targetPositionActual) > 0.05)
            //    yield return null;
            //targetPositionActual = targets[0].localPosition;
        }

        private void SetDirection(Vision vis)
        {
            vision = vis;
            switch (vision) 
            {
                case Vision.Arriba:
                case Vision.Abajo:
                case Vision.Derecha:
                case Vision.Izquierda:
                targetPositionActual = transform.position + targets[vision];
                    break;
                case Vision.Puerta:
                case Vision.Ventana:
                    targetPositionActual = targets[vision];
                    break;

            }
        }

        public void PlayAnimation(string stateName)
        {
            animator.Play(stateName);
        }


        public void PlayDisruptiveAction(string stateName, AudioClip clip)
        {
            animator.Play(stateName);
            audio.clip = clip;
            audio.Play();
            //Invoke("SetNoProblematicStudent", 2.0f);
        }

        public void SetNoProblematicStudent()
        {
            text.color = Color.black;
            problematic = false;
            if(state==State.Stand) SitBack();
        }


        public bool IsStudentOnVision()
        {
            Plane[] cameraFrustum;
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            var bounds = GetCollider().bounds;
            bounds.center += new Vector3(0, 1f, 0);
            return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        }

        #region Move

        //Corrutina para completar moviemitno
        IEnumerator OnCompleteMove()
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                yield return null;
            text.gameObject.transform.localPosition = new Vector3(0, 1.75f, 0);
            agent.SetDestination(dest);
            while (Vector3.Distance(transform.position, dest) > 0.5f)
                yield return null;
            animator.Play("Standing");
            transform.rotation = Quaternion.Euler(0, 90, 0);
            agent.enabled = false;
            state = State.Stand;
        }
        //Corrutina para completar accion de sentarse
        IEnumerator OnCompleteSitBack()
        {
            while (Vector3.Distance(transform.position, deskPosition) > 0.1f)
                yield return null;
            animator.SetBool("onFoot", false);
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Sit Down"))
                yield return null;
            text.gameObject.transform.localPosition = new Vector3(0, 1.25f, 0);
            transform.rotation = Quaternion.Euler(Vector3.zero);
            agent.enabled = false;
            state = State.Sit;
        }

        //Sentarse en tu sitio
        public void SitBack()
        {
            agent.enabled = true;
            agent.SetDestination(deskPosition);
            animator.Play("Walking");
            StartCoroutine(OnCompleteSitBack());

        }
        /// <summary>
        /// Moverse a la posicion des
        /// </summary>
        /// <param name="des"></param>
        public void MoveTo(Vector3 des)
        {
            agent.enabled = true;
            dest = des;
            if (state == State.Sit)
            {
                animator.SetBool("onFoot", true);

            }
            else
            {
                animator.Play("Walking");
            }
            StartCoroutine(OnCompleteMove());

        }

        //Cambiar de asiento 
        public void ChangeDesk(Vector3 pos)
        {
            deskPosition = pos;
            if (state == State.Stand)
            {
                SitBack();
            }
            else
            {
                animator.SetBool("onFoot", true);
                StartCoroutine(OnCompleteStandChange());
            }
        }
        //Corrutina para completar accion de cambiar sitio
        IEnumerator OnCompleteStandChange()
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                yield return null;
            text.gameObject.transform.localPosition = new Vector3(0, 1.75f, 0);
            SitBack();
        }
        #endregion


        #region Behavior

        public StudentBehavior GetBehavior() { return behavior; }

        #endregion
    }

}