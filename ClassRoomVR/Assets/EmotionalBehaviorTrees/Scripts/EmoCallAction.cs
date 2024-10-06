using ClassRoomVR;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Emo
{
    [TaskCategory("Emo")]
    [TaskDescription("Llamar un accion del estudiante, Returns Success.")]
    public class EmoCallAction : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;
        public EventSittingAnimations n;
   
        private StudentActions sActions;

        public override void OnStart()
        {
            sActions=GetComponent<StudentActions>();

        }

        public override TaskStatus OnUpdate()
        {
            if (sActions == null)
            {
                Debug.LogWarning("StudentActions is null");
                return TaskStatus.Failure;
            }
            StartCoroutine(sActions.PlaySitAction(EventSittingAnimations.Yelling));

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetGameObject = null;
        }
    }
}