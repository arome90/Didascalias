using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;
using System.Linq;
using Oculus.Platform.Models;
using UnityEngine.UIElements;

namespace ClassRoomVR
{
    [System.Serializable]
    public class Student : MonoBehaviour
    {
        private FieldOfVision vision;
        public FieldOfVision distracted;
        private FieldOfVision[] distractedArray;
        private State state;

        // Serialized fields for customization in the Inspector
        [SerializeField] private Gender gender;
        [SerializeField] private bool problematic = false;
        [SerializeField] private TextMesh studentNameText;
        private Desk desk;
        [SerializeField] private RuntimeAnimatorController animatorController;
        private Animator animator;
        private AudioSource audioSource;
        private NavMeshAgent navMeshAgent;
        private new MeshCollider collider;
        [SerializeField] private Transform target;
        private Vector3 actualTargetPosition;
        private Dictionary<FieldOfVision, Vector3> targets;
        [SerializeField] private MultiAimConstraint headConstraint;
        [SerializeField] private TextMesh attentionText;
        private StudentBehavior behavior;
        private Transform player;

        #region Getters
        // Getter methods for accessing properties
        public Desk GetDesk() => desk;
        public Gender GetGender() => gender;
        public bool IsProblematicStudent() => problematic;
        public AudioSource GetAudioSource() => audioSource;
        #endregion
       public float visionFromOnFoot;
        float visionTeacher;
        private void Awake()
        {
            // Initialize references and components
            collider = GetComponent<MeshCollider>();
            animator= GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            behavior = GetComponent<StudentBehavior>();
            state = State.Sitting;
            visionTeacher = 0;
            distractedArray = System.Enum.GetValues(typeof(FieldOfVision)).Cast<FieldOfVision>()
                .Where(c => (distracted & c) == c)
                .ToArray();
            //voiceGenerator = GetComponent<VoiceGenerator>();
        }

        // Methods to set student's parameters and create their body
        public void SetParameters(Transform player,string name, Gender gender)
        {
            this.player = player;            
            transform.name = name;
            studentNameText.text = name;
            this.gender = gender;
        }

        public void CreateBody(GameObject prefab)
        {
            GameObject body = InstantiateAndAddCollider(prefab);
            ConfigureAnimator(body);
            headConstraint.data.constrainedObject = GetHeadBone();
            var rigbuilder = body.AddComponent<RigBuilder>();
            rigbuilder.layers.Add(new RigLayer(body.transform.GetChild(body.transform.childCount-1).GetComponent<Rig>(), true));
            rigbuilder.Build();
        }

        private GameObject InstantiateAndAddCollider(GameObject prefab)
        {
            GameObject body = Instantiate(prefab, transform);
            transform.GetChild(1).parent = body.transform;
            collider = body.AddComponent<MeshCollider>();
            return body;
        }

        private void ConfigureAnimator(GameObject body)
        {
            animator = body.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }
        }
        private Transform GetHeadBone()
        {
            Transform body = transform.GetChild(1);
            int index = body.childCount - 4;
            return body.GetChild(index).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(0);
        }

        // Methods to set and manage the student's behavior and actions
        public void SetProblematicStudent()
        {
            studentNameText.color = Color.red;
            problematic = true;
        }

        public void SetDesk(Desk d)
        {
            desk = d;
        }

        public void SetTargets(Transform[] transforms)
        {
            // Set target positions for different field of vision options
            targets = new Dictionary<FieldOfVision, Vector3>
            {
                { FieldOfVision.Up, transform.up * 2f },
                { FieldOfVision.Right, transform.right },
                { FieldOfVision.Down, transform.up / -2 },
                { FieldOfVision.Left, -transform.right },
                { FieldOfVision.Window, transforms[0].position },
                { FieldOfVision.Door, transforms[1].position },
                { FieldOfVision.Teacher, Vector3.zero }
            };
        }

        public void PayAttention()
        {
            behavior.SetAttention();
            SetDirection(FieldOfVision.Teacher);
        }

        public void GetDistracted()
        {
            SetDirection(distractedArray[Random.Range(0, distractedArray.Length)]);
        }

        // Update method to handle student's behavior and animations
        private void Update()
        {
            if (GameManager.Instance.IsPause) return;
            HandleInput();
            UpdateTargetPosition();
            if(attentionText!=null) attentionText.text = behavior.AttentionLevel.ToString("0.##");
        }

        // Methods to handle user input and trigger actions
        private void HandleInput()
        {
            for (int i = 0; i <= 6; i++)
            {
                KeyCode keyCode = KeyCode.Keypad0 + i;
                if (Input.GetKeyDown(keyCode))
                {
                    animator.SetInteger("Accion", i);
                    if (i == 1)
                    {
                        desk.Balancearse();
                    }
                }
            }

            // Check specific keys for different actions
            KeyCode[] actionKeys = { KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha6, KeyCode.Alpha5 };

            foreach (KeyCode key in actionKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    HandleActionInput(key);
                }
            }
        }

        private void HandleActionInput(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Alpha7:
                    PayAttention();
                    break;
                case KeyCode.Alpha8:
                    GetDistracted();
                    break;
                case KeyCode.Alpha6:
                    StartCoroutine(Nod());
                    break;
                case KeyCode.Alpha5:
                    StartCoroutine(ShakeHead());
                    break;
            }
        }

        // Coroutine methods for nodding and shaking head animations
        private float smoothTime = 0.15f;
        private float maxSpeed = 2f;
        private Vector3 currentVelocity;
        private void UpdateTargetPosition()
        {
            studentNameText.transform.LookAt(player);
            studentNameText.transform.rotation = Quaternion.LookRotation(player.forward);

            if (vision != FieldOfVision.Teacher && state == State.Sitting)
            {
                target.position = Vector3.SmoothDamp(target.position, actualTargetPosition, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);
            }
            else if (vision == FieldOfVision.Teacher)
            {
                target.position = Vector3.MoveTowards(target.position, player.position - Vector3.up * visionTeacher, 5.0f * Time.deltaTime);
            }
        
        }

        IEnumerator Nod()
        {
            for (int i = 0; i < 2; i++)
            {
                SetDirection(FieldOfVision.Up);
                while (Vector2.Distance(target.position, actualTargetPosition) > 0.5f)
                    yield return null;
                SetDirection(FieldOfVision.Down);
                while (Vector2.Distance(target.position, actualTargetPosition) > 0.5f)
                    yield return null;
            }
        }

        IEnumerator ShakeHead()
        {
            for (int i = 0; i < 2; i++)
            {
                SetDirection(FieldOfVision.Right);
                while (Vector2.Distance(target.position, actualTargetPosition) > 0.2f)
                    yield return null;
                SetDirection(FieldOfVision.Left);
                while (Vector2.Distance(target.position, actualTargetPosition) > 0.2f)
                    yield return null;
            }
        }

        // Method to set the direction of student's attention
        private void SetDirection(FieldOfVision fieldOfVision)
        {
            vision = fieldOfVision;
            switch (vision)
            {
                case FieldOfVision.Up:
                case FieldOfVision.Down:
                case FieldOfVision.Right:
                case FieldOfVision.Left:
                    actualTargetPosition = transform.position + targets[vision] + transform.forward;
                    break;
                case FieldOfVision.Door:
                case FieldOfVision.Window:
                    actualTargetPosition = targets[vision];
                    break;
            }
        }

        // Methods to play animations and actions
        public void PlayAnimation(string stateName)
        {
            animator.Play(stateName);
        }

        public void PlayDisruptiveAction(string stateName, AudioClip clip)
        {
           // int i =Animator.StringToHash("onFoot");
            animator.Play(stateName);
            audioSource.clip = clip;
            audioSource.Play();
        }

        // Method to set the student as not problematic
        public void SetNotProblematicStudent()
        {
            studentNameText.color = Color.black;
            problematic = false;
            if (state == State.Standing)
                SitBack();
        }

        // Method to check if the student is in the player's field of vision
        public bool IsStudentInFieldOfVision()
        {
            Plane[] cameraFrustum;
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            var bounds = collider.bounds;
            bounds.center += new Vector3(0, 1f, 0);
            return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        }

        // Methods to handle movement and behavior
        #region Movement

        // Coroutine to complete the move to a destination
        IEnumerator OnCompleteMove(Vector3 destination,float breakDistance)
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                yield return null;
            
            state = State.Standing;
            visionTeacher = visionFromOnFoot;
            target.position = transform.position + transform.forward + targets[FieldOfVision.Up];
            studentNameText.gameObject.transform.localPosition = new Vector3(0, 1.75f, 0);
            navMeshAgent.SetDestination(destination);
            while (Distance(transform.position, destination, breakDistance))
                yield return null;
            navMeshAgent.enabled = false;
            animator.Play("Standing");
        }

        bool Distance(Vector3 tranform, Vector3 dest, float breakDistance) 
        {
            Vector2 punto1Proyectado = new Vector2(tranform.x, tranform.z);
            Vector2 punto2Proyectado = new Vector2(dest.x, dest.z);
            return Vector2.Distance(punto1Proyectado, punto2Proyectado)> breakDistance;


        }
        // Coroutine to complete the sit back action
        IEnumerator OnCompleteSitBack()
        {
            while (Distance(transform.position, desk.GetPositionStudent(), 0.1f))
            {
                yield return null;
            }
            navMeshAgent.enabled = false;
            transform.rotation = desk.transform.rotation;
            transform.position = desk.transform.position - new Vector3(0,0,0.1f);
            animator.SetBool("onFoot", false);
            desk.PlayAnimacionMesa(Animaciones.SitGanas);
            studentNameText.gameObject.transform.localPosition = new Vector3(0, 1.25f, 0);
            state = State.Sitting;
            visionTeacher = 0;
            //while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Sitting"))
            //    yield return null;
            //transform.rotation = desk.transform.rotation;



        }

        // Method to make the student sit back in their desk
        public void SitBack()
        {
            navMeshAgent.enabled = true;

            navMeshAgent.SetDestination(desk.GetPositionStudent());
            animator.Play("Walking");
            StartCoroutine(OnCompleteSitBack());
        }

        // Method to move the student to a specific destination
        public void MoveTo(Vector3 destination,float breakDistance)
        {
            navMeshAgent.enabled = true;

            if (state == State.Sitting)
            {
                animator.SetBool("onFoot", true);
                desk.PlayAnimacionMesa(Animaciones.Levantar);
            }
            else
            {
                animator.Play("Walking");
            }
            StartCoroutine(OnCompleteMove(destination, breakDistance));
        }

        // Method to change the student's desk
        public void ChangeDesk(Desk d)
        {
            desk = d;
            if (state == State.Standing)
            {
                SitBack();
            }
            else
            {
                animator.SetBool("onFoot", true);
                StartCoroutine(OnCompleteStandChange());
            }
        }

        // Coroutine to complete the stand change action
        IEnumerator OnCompleteStandChange()
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                yield return null;
            studentNameText.gameObject.transform.localPosition = new Vector3(0, 1.75f, 0);
            SitBack();
        }

        #endregion

        // Method to get the student's behavior
        #region Behavior

        public StudentBehavior GetBehavior()=> behavior;
        #endregion

        //VoiceGenerator voiceGenerator;
        //public void GenerateText()
        //{
        //    voiceGenerator.GenerateVoiceClipAsync("hola");
        //}
    }
}
