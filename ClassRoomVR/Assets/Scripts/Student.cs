using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ClassRoomVR
{
    [System.Serializable]
    public class Student : MonoBehaviour
    {
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

        [SerializeField]Transform target;
        Vector3 targetPosition;
        Transform[] targets;

        [SerializeField] MultiAimConstraint head;
        

        private void Start()
        {
            audio = GetComponent<AudioSource>();
            agent = GetComponent<NavMeshAgent>();
            state = State.Sit;
            //Invoke("cambiar", 2);
            

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
            collider = transform.GetChild(transform.childCount-1).GetComponent<Collider>();
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
            targets = tar;
            ////Posiciones desde el estudiante
            //Vector3[] dir = getDirections();
            ////tar = posiciones globales para todos
            //int pos = Random.Range(0, tar.Length + dir.Length);
            //if (pos < tar.Length) Debug.Log(tar[pos].localPosition);
            ////Debug.Log(target.localPosition);
            //target.position = pos < tar.Length ? tar[pos].localPosition : dir[pos- tar.Length] ;
            ////Debug.Log(pos + " " + name + target.localPosition);
        }

        void cambiar()
        {
            //Posiciones desde el estudiante
            Vector3[] dir = getDirections();
            //tar = posiciones globales para todos
            int pos = Random.Range(0, targets.Length + dir.Length);
            if (pos < targets.Length) Debug.Log(targets[pos].localPosition);
            Debug.Log(target.localPosition);
            targetPosition= pos < targets.Length ? targets[pos].localPosition : dir[pos - targets.Length];

            //target.position = 
            //Debug.Log(pos + " " + name + target.localPosition);
        }


        public void PayAttention() 
        {
            //target.position = targets[0].localPosition;
            targetPosition = targets[0].localPosition;
        }

        public void GetDistracted() 
        {

            Vector3[] dir = getDirections();
            //tar = posiciones globales para todos
            int pos = Random.Range(1, targets.Length + dir.Length);
            targetPosition = pos < targets.Length ? targets[pos].localPosition : dir[pos - targets.Length];

            // target.position = pos < targets.Length ? targets[pos].localPosition : dir[pos - targets.Length];
        }


        private void Update()
        {
            //for (int i = 0; i < 7; i++)
            //{
            //    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            //    {
            //        Vector3[] dir = getDirections();

            //        target.position = i < targets.Length ? targets[i].localPosition : dir[i - targets.Length];
            //    }

            //}

            if (Input.GetKeyDown(KeyCode.Alpha1)) 
            {
                PayAttention();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) 
            {
                GetDistracted();
            }


           target.position = Vector3.MoveTowards(target.position, targetPosition, 3.0f* Time.deltaTime);


        }


       

        private Vector3[] getDirections() 
        {
            Vector3[] vec = new Vector3[4];
            vec[0] = transform.localPosition + new Vector3(0, 1.5f, 1);
            vec[1] = transform.localPosition + Vector3.right;
            vec[2] = transform.localPosition + new Vector3(0,-1, 1);
            vec[3] = transform.localPosition + Vector3.left;
            return vec;

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
            Invoke("SetNoProblematicStudent", 2.0f);
        }

        public void SetNoProblematicStudent() 
        {
            text.color = Color.black;
            problematic = false;
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
    }

}