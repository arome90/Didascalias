using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Ordenar de acuerdo con los pesos y asociar un probi de probabilidad a cada Nodo secundario ")]
    [TaskIcon("{SkinColor}PrioritySelectorIcon.png")]
    public class PriorityRandomSelector : Composite
    {
        [Tooltip("Probability factor [0.5,1]")]
        public float probability = 0.5f;

        [Tooltip("Seed the random number generator to make things easier to debug")]
        public int seed = 0;
        [Tooltip("Do we want to use the seed?")]
        public bool useSeed = false;

        // The index of the child that is currently running or is about to run.
        private int currentChildIndex = 0;
        // The task status of every child task.
        private TaskStatus executionStatus = TaskStatus.Inactive;
        // The order to run its children in. 
        private List<int> childrenExecutionOrder = new List<int>();


        public override void OnAwake()
        {
            // If specified, use the seed provided.
            if (useSeed)
            {
                Random.InitState(seed);
            }
        }

        public override float GetPriority()
        {
            if (children.Count == 0) return 0;
            return children[childrenExecutionOrder[0]].GetPriority();
        }

        public override void OnStart()
        {
            // Make sure the list is empty before we add child indexes to it.
            childrenExecutionOrder.Clear();

            // Loop through each child task and determine its priority. The higher the priority the lower it goes within the list. The task with the highest
            // priority will be first in the list and will be executed first.
            for (int i = 0; i < children.Count; ++i)
            {
                float priority = children[i].GetPriority();
                int insertIndex = childrenExecutionOrder.Count;
                for (int j = 0; j < childrenExecutionOrder.Count; ++j)
                {
                    if (children[childrenExecutionOrder[j]].GetPriority() < priority)
                    {
                        insertIndex = j;
                        break;
                    }
                }
                childrenExecutionOrder.Insert(insertIndex, i);
            }

            float end_num = probability * Mathf.Pow(1 - probability, children.Count - 1);
            float aux = Random.Range(0, 1 - end_num);
            int k = 0;
            while (aux > 0)
            {
                k++;
                aux -= probability * Mathf.Pow(1 - probability, k - 1);
            }
            currentChildIndex = k - 1;

        }

        public override int CurrentChildIndex()
        {
            // Use the execution order list in order to determine the current child index.
            return childrenExecutionOrder[currentChildIndex];
        }

        public override bool CanExecute()
        {
            // We can continue to execuate as long as we have children that haven't been executed and no child has returned success.
            return executionStatus != TaskStatus.Success && executionStatus != TaskStatus.Failure;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            executionStatus = childStatus;
        }

        public override void OnConditionalAbort(int childIndex)
        {
            // Set the current child index to the index that caused the abort
            currentChildIndex = childIndex;
            executionStatus = TaskStatus.Inactive;
        }

        public override void OnEnd()
        {
            // All of the children have run. Reset the variables back to their starting values.
            executionStatus = TaskStatus.Inactive;
            currentChildIndex = 0;
        }
    }
}