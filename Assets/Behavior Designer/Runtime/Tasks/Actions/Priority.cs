using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("优先级参数节点")]
    [TaskIcon("{SkinColor}LogIcon.png")]
    public class Priority : Action
    {
        [Tooltip("优先级")]
        public SharedFloat priority;

        /// <summary>
        /// 重写获取优先级的方法
        /// </summary>
        /// <returns></returns>
        public override float GetPriority()
        {
            return priority.Value;
        }
    }
}
