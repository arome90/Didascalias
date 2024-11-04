#region Assembly BehaviorDesigner.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\jianu\Documents\College\TFG\Didascalias\ClassRoomVR\Assets\Libs\BehaviourDesigner\Behavior Designer\Runtime\BehaviorDesigner.Runtime.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public abstract class EmoParentTask : Task
    {
        [SerializeField]
        protected List<Task> children;

        public List<Task> Children
        {
            get
            {
                return children;
            }
            private set
            {
                children = value;
            }
        }

        public virtual int MaxChildren()
        {
            return int.MaxValue;
        }

        public virtual bool CanRunParallelChildren()
        {
            return false;
        }

        public virtual int CurrentChildIndex()
        {
            return 0;
        }

        public virtual bool CanExecute()
        {
            return true;
        }

        public virtual TaskStatus Decorate(TaskStatus status)
        {
            return status;
        }

        public virtual bool CanReevaluate()
        {
            return false;
        }

        public virtual void OnChildExecuted(TaskStatus childStatus)
        {
        }

        public virtual void OnChildExecuted(int childIndex, TaskStatus childStatus)
        {
        }

        public virtual void OnChildStarted()
        {
        }

        public virtual void OnChildStarted(int childIndex)
        {
        }

        public virtual TaskStatus OverrideStatus(TaskStatus status)
        {
            return status;
        }

        public virtual TaskStatus OverrideStatus()
        {
            return TaskStatus.Running;
        }

        public virtual void OnConditionalAbort(int childIndex)
        {
        }

        public override float GetUtility()
        {
            float num = 0f;
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] != null && !children[i].Disabled)
                    {
                        num += children[i].GetUtility();
                    }
                }
            }

            return num;
        }

        public override void OnDrawGizmos()
        {
            if (children == null)
            {
                return;
            }

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null && !children[i].Disabled)
                {
                    children[i].OnDrawGizmos();
                }
            }
        }

        public void AddChild(Task child, int index)
        {
            if (children == null)
            {
                children = new List<Task>();
            }

            children.Insert(index, child);
        }

        public void ReplaceAddChild(Task child, int index)
        {
            if (children != null && index < children.Count)
            {
                children[index] = child;
            }
            else
            {
                AddChild(child, index);
            }
        }
    }
}
#if false // Decompilation log
'240' items in cache
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files\Unity\Hub\Editor\2022.3.25f1\Editor\Data\UnityReferenceAssemblies\unity-4.8-api\mscorlib.dll'
------------------
Resolve: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files\Unity\Hub\Editor\2022.3.25f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files\Unity\Hub\Editor\2022.3.25f1\Editor\Data\UnityReferenceAssemblies\unity-4.8-api\System.dll'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files\Unity\Hub\Editor\2022.3.25f1\Editor\Data\UnityReferenceAssemblies\unity-4.8-api\System.Core.dll'
------------------
Resolve: 'UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files\Unity\Hub\Editor\2022.3.25f1\Editor\Data\Managed\UnityEngine\UnityEngine.AnimationModule.dll'
------------------
Resolve: 'UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files\Unity\Hub\Editor\2022.3.25f1\Editor\Data\Managed\UnityEngine\UnityEngine.IMGUIModule.dll'
------------------
Resolve: 'UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files\Unity\Hub\Editor\2022.3.25f1\Editor\Data\Managed\UnityEngine\UnityEngine.PhysicsModule.dll'
------------------
Resolve: 'UnityEngine.Physics2DModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.Physics2DModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files\Unity\Hub\Editor\2022.3.25f1\Editor\Data\Managed\UnityEngine\UnityEngine.Physics2DModule.dll'
#endif
