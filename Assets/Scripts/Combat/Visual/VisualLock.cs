using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Combat.Visual
{
    public class VisualLock
    {
        /// <summary>
        /// 增加目标
        /// </summary>
        /// <param name="targetGroup">目标组</param>
        /// <param name="target">目标</param>
        /// <param name="weight">权重</param>
        /// <param name="radius">半径</param>
        private void AddTarget(CinemachineTargetGroup targetGroup, Transform target, float weight, float radius)
        {
            targetGroup.AddMember(target, weight, radius);    //TODO:摄像机目标组的成员的半径需要测试调整
        }

        /// <summary>
        /// 移除目标
        /// </summary>
        /// <param name="targetGroup">目标组</param>
        /// <param name="target">目标</param>
        public void RemoveTarget(CinemachineTargetGroup targetGroup, Transform target)
        {
            if (target == null || targetGroup == null) return;
            targetGroup.RemoveMember(target);
        }

        /// <summary>
        /// 修改目标的权重
        /// </summary>
        /// <param name="targetGroup">目标组</param>
        /// <param name="target">目标</param>
        /// <param name="newWeight">新权重值</param>
        public void SetTargetWeight(CinemachineTargetGroup targetGroup, Transform target, float newWeight)
        {
            if (target == null || targetGroup == null) return;

            int index = targetGroup.FindMember(target);
            if (index < 0) return;

            CinemachineTargetGroup.Target t = targetGroup.m_Targets[index];

            t.weight = Mathf.Max(0, newWeight); // 权重不能为负

            targetGroup.m_Targets[index] = t;
        }

        /// <summary>
        /// 修改目标的半径
        /// </summary>
        /// <param name="targetGroup">目标组</param>
        /// <param name="target">目标</param>
        /// <param name="newRadius">新半径值</param>
        public void SetTargetRadius(CinemachineTargetGroup targetGroup, Transform target, float newRadius)
        {
            if (target == null || targetGroup == null) return;
            
            int index = targetGroup.FindMember(target);
            if (index < 0) return;

            CinemachineTargetGroup.Target t = targetGroup.m_Targets[index];
            t.radius = Mathf.Max(0, newRadius);
            targetGroup.m_Targets[index] = t;
        }

    }

}
