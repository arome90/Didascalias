using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;
using System.Linq;

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
        [SerializeField] private string studentName;
        [SerializeField] private bool problematic = false;
        [SerializeField] private TextMesh studentNameText;
        private Desk desk;
        [SerializeField] private RuntimeAnimatorController animatorController;
        private Animator animator;
        private AudioSource audioSource;
        private NavMeshAgent navMeshAgent;
        private Collider collider;
        [SerializeField] private Transform target;
        private Vector3 actualTargetPosition;
        private Dictionary<FieldOfVision, Vector3> targets;
        [SerializeField] private MultiAimConstraint headConstraint;
        [SerializeField] private TextMesh attentionText;
        private StudentBehavior behavior;
        private Transform teacher;

        #region Getters
        // Getter methods for accessing properties
        public Desk GetDesk() => desk;
        public Gender GetGender() => gender;
        public string GetStudentName() => studentName;
        public Collider GetCollider() => collider;
        public bool IsProblematicStudent() => problematic;
        public AudioSource GetAudioSource() => audioSource;
        #endregion

        private void Start()
        {
            // Initialize references and components
            audioSource = GetComponent<AudioSource>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            behavior = GetComponent<StudentBehavior>();
            state = State.Sitting;
            teacher = GameManager.Instance.GetPlayer().transform;
            distractedArray = System.Enum.GetValues(typeof(FieldOfVision)).Cast<FieldOfVision>()
                .Where(c => (distracted & c) == c)
                .ToArray();
            //voiceGenerator = GetComponent<VoiceGenerator>();
        }

        // Methods to set student's parameters and create their body
        public void SetParameters(string name, Gender gender)
        {
            studentName = name;
            transform.name = name;
            studentNameText.text = name;
            this.gender = gender;
        }

        public void CreateBody(GameObject prefab)
        {
            GameObject body = InstantiateAndAddCollider(prefab);
            ConfigureAnimator(body);
            SetupHeadConstraint();
            BuildRig();
        }

        private GameObject InstantiateAndAddCollider(GameObject prefab)
        {
            GameObject body = Instantiate(prefab, transform);
            body.AddComponent<MeshCollider>();
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

        private void SetupHeadConstraint()
        {
            collider = transform.GetChild(transform.childCount - 1).GetComponent<Collider>();
            headConstraint.data.constrainedObject = GetHeadBone();
        }

        private void BuildRig()
        {
            transform.GetComponent<RigBuilder>().Build();
        }

        private Transform GetHeadBone()
        {
            Transform body = transform.GetChild(2);
            int index = body.childCount - 3;
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
            targets = new Dictionary<FieldOfVision, Vector3>();
            targets.Add(FieldOfVision.Up, transform.up * 2f);
            targets.Add(FieldOfVision.Right, transform.right);
            targets.Add(FieldOfVision.Down, transform.up / -2);
            targets.Add(FieldOfVision.Left, -transform.right);
            targets.Add(FieldOfVision.Window, transforms[0].position);
            targets.Add(FieldOfVision.Door, transforms[1].position);
            targets.Add(FieldOfVision.Teacher, Vector3.zero);
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
            attentionText.text = behavior.AttentionLevel.ToString("0.##");
        }

        // Methods to handle user input and trigger actions
        private void HandleInput()
        {
            // Check numeric keypad input for different field of vision options
            for (int i = 0; i < 8; i++)
            {
                KeyCode keyCode = KeyCode.Keypad0 + i;

                if (Input.GetKeyDown(keyCode))
                {
                    FieldOfVision fieldOfVision = targets.ElementAt(i).Key;
                    Debug.Log(i + " " + fieldOfVision);
                    SetDirection(fieldOfVision);
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

            if (vision != FieldOfVision.Teacher && state== State.Sitting)
            {
                target.position = Vector3.SmoothDamp(target.position, actualTargetPosition, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);
            }
            else if (vision == FieldOfVision.Teacher)
            {
                target.position = Vector3.MoveTowards(target.position, teacher.position, 5.0f * Time.deltaTime);
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
            var bounds = GetCollider().bounds;
            bounds.center += new Vector3(0, 1f, 0);
            return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        }

        // Methods to handle movement and behavior
        #region Movement

        // Coroutine to complete the move to a destination
        IEnumerator OnCompleteMove(Vector3 destination)
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                yield return null;
            state = State.Standing;
            target.position = transform.position + transform.forward + targets[FieldOfVision.Up];
            studentNameText.gameObject.transform.localPosition = new Vector3(0, 1.75f, 0);
            navMeshAgent.SetDestination(destination);
            while (Vector3.Distance(transform.position, destination) > 0.5f)
                yield return null;
            animator.Play("Standing");
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        // Coroutine to complete the sit back action
        IEnumerator OnCompleteSitBack()
        {
            while (Vector3.Distance(transform.position, desk.GetPositionStudent()) > 0.3f)
            {
                yield return null;
            }
            animator.SetBool("onFoot", false);
            studentNameText.gameObject.transform.localPosition = new Vector3(0, 1.25f, 0);
            navMeshAgent.enabled = false;
            transform.rotation = desk.transform.rotation;
            transform.position = desk.GetPositionStudent();
            state = State.Sitting;
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
        public void MoveTo(Vector3 destination)
        {
            navMeshAgent.enabled = true;
            if (state == State.Sitting)
            {
                animator.SetBool("onFoot", true);
            }
            else
            {
                animator.Play("Walking");
            }
            StartCoroutine(OnCompleteMove(destination));
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
