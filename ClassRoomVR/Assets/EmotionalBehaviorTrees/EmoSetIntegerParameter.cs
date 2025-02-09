using UnityEngine;
using System.Collections;
using ClassRoomVR;

namespace BehaviorDesigner.Runtime.Tasks.Emo.Unity.UnityAnimator
{
    [TaskCategory("Emo/Unity/Animator")]
    [TaskDescription("Sets the int parameter on an animator. Returns Success.")]
    public class SetIntegerParameter : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;
        [Tooltip("The value of the int parameter")]
        public SharedInt intValue;
        [Tooltip("Should the value be reverted back to its original value after it has been set?")]
        public bool setOnce;

        private StudentActions action;
        private GameObject prevGameObject;

        public override void OnStart()
        {
            action = GetComponent<StudentActions>();
            var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (currentGameObject != prevGameObject)
            {
                prevGameObject = currentGameObject;
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (action == null)
            {
                Debug.LogWarning("Action is null");
                return TaskStatus.Failure;
            }


            int prevValue = action.GetAction();
            action.PlaySitAction((EventSittingAnimations)(intValue.Value));
            ClimateManager.Instance.SetWeight(gameObject.name, (EventSittingAnimations)(intValue.Value));
            // animator.SetInteger(hashID, intValue.Value);
            if (setOnce)
            {
                StartCoroutine(ResetValue(prevValue));
            }

            return TaskStatus.Success;
        }

        public IEnumerator ResetValue(int origVale)
        {
            yield return null;
            action.PlaySitAction((EventSittingAnimations)(origVale));
            // animator.SetInteger(hashID, origVale);
        }

        public override void OnReset()
        {
            targetGameObject = null;
            intValue = 0;
        }
    }
}