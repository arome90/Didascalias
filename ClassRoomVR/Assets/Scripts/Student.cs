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
        public enum Sex { Women, Men };
        private enum State {Sit,Stand};
        State state;
        [SerializeField] Sex sex;
        [SerializeField] string name;
        [SerializeField] bool problematic=false;

        [SerializeField] TextMesh text;
        

        [SerializeField] RigLayer rig;

        Vector3 deskPosition;

        [SerializeField]
        private RuntimeAnimatorController controller;

        Animator animator;
        AudioSource audio;
        NavMeshAgent agent;

        Vector3 dest;
        private void Start()
        {
            audio = GetComponent<AudioSource>();
            agent = GetComponent<NavMeshAgent>();
            state = State.Sit;
        }
        public void SetParameters(string na, int s)
        {
            name = na;
            transform.name = na;
            text.text = na;
            sex = (Sex)s;
            

        }


        public void CreateBody(GameObject obj)
        {
            GameObject body = Instantiate(obj, transform);
            body.AddComponent<MeshCollider>();
            //obj.AddComponent<RigBuilder>();
            //if (rig != null)
            //{
            //    obj.GetComponent<RigBuilder>().layers.Add(rig);
            //}

             animator = body.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = controller;
            }
           
        }


        
        public void SetProblematicStudent()
        {
            text.color = Color.red;
            problematic = true;
        }

        public bool GetProblematicStudent()
        {
            return problematic;
        }

        public void SetDesk(Vector3 pos)
        {
            deskPosition = pos;
        }

        public Vector3 GetDesk()
        {
            return deskPosition;
        }

        public Sex GetSex() { return sex; }
        public string GetName() { return name; }

        public void PlayAnimation(string stateName) 
        {
            animator.Play(stateName);

        }

        public AudioSource getAudio()
        {
            return audio;
        }

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

        public void SitBack() 
        {
            agent.enabled = true;
            agent.SetDestination(deskPosition);
            animator.Play("Walking");
            StartCoroutine(OnCompleteSitBack());

        }
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

        public void ChangeDesk(Vector3 pos)
        {
            deskPosition = pos;
            animator.SetBool("onFoot", true);
            StartCoroutine(OnCompleteStandChange());
        }

        IEnumerator OnCompleteStandChange()
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                yield return null;
            text.gameObject.transform.localPosition = new Vector3(0, 1.75f, 0);
            SitBack();
        }

    }

}