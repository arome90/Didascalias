using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BehaviorDesigner.Runtime.Tasks
{


    [TaskDescription("Returns a TaskStatus of running. Will only stop when interrupted or a conditional abort is triggered.")]
    [TaskIcon("{SkinColor}IdleIcon.png")]
    public class IniEmoBehaviorTree : Action
    {

        public override void OnStart()
        {

        }


        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Running;
        }
    }
}