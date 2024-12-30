using ClassRoomVR;

namespace BehaviorDesigner.Runtime.Tasks.Unity.Math
{
    [TaskCategory("Math")]
    [TaskDescription("Checks the attention: less than, less than or equal to, equal to, not equal to, greater than or equal to, or greater than.")]
    public class CheckAttention : Conditional
    {
        public enum Operation
        {
            LessThan,
            LessThanOrEqualTo,
            EqualTo,
            NotEqualTo,
            GreaterThanOrEqualTo,
            GreaterThan
        }

        [Tooltip("The operation to perform")]
        public Operation operation;
        [Tooltip("The first float")]
        private float attention;
        [Tooltip("The second float")]
        public SharedFloat float2;

        private StudentBehavior studentBehavior;

        public override void OnStart()
        {
            base.OnStart();
            studentBehavior = gameObject.GetComponent<StudentBehavior>();
        }

        public override TaskStatus OnUpdate()
        {
            attention = studentBehavior.AttentionLevel;
            switch (operation)
            {
                case Operation.LessThan:
                    return attention < float2.Value ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.LessThanOrEqualTo:
                    return attention <= float2.Value ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.EqualTo:
                    return UnityEngine.Mathf.Approximately(attention, float2.Value) ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.NotEqualTo:
                    return !UnityEngine.Mathf.Approximately(attention, float2.Value) ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.GreaterThanOrEqualTo:
                    return attention >= float2.Value ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.GreaterThan:
                    return attention > float2.Value ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Failure;
        }

        public override void OnReset()
        {
            operation = Operation.LessThan;
            attention = 0;
            float2.Value = 0;
        }
    }
}