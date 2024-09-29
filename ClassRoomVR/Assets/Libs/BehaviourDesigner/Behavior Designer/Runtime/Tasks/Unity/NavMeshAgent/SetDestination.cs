using UnityEngine;
using UnityEngine.AI;

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityNavMeshAgent
{
    [TaskCategory("Unity/NavMeshAgent")]
    [TaskDescription("Sets the destination of the agent in world-space units. Returns Success if the destination is valid.")]
    public class SetDestination: Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;
        [SharedRequired]
        [Tooltip("The NavMeshAgent destination")]
        public Vector3 destination;


        public Transform destinationGameObject;

        // cache the navmeshagent component
        private NavMeshAgent navMeshAgent;
        private GameObject prevGameObject;

        public override void OnStart()
        {
            var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (currentGameObject != prevGameObject) {
                navMeshAgent = currentGameObject.GetComponent<NavMeshAgent>();
                prevGameObject = currentGameObject;
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (navMeshAgent == null) {
                Debug.LogWarning("NavMeshAgent is null");
                return TaskStatus.Failure;
            }

            if(destinationGameObject != null)
            {
                return navMeshAgent.SetDestination(destinationGameObject.localPosition) ? TaskStatus.Success : TaskStatus.Failure;

            }
            else
            {
                return navMeshAgent.SetDestination(destination) ? TaskStatus.Success : TaskStatus.Failure;

            }
        }

        public override void OnReset()
        {
        }
    }
}