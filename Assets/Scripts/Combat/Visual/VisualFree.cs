using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat.Visual
{
    public class VisualFree
    {
        private Vector3 _direction;

        /// <summary>
        /// 视角旋转
        /// </summary>
        /// <param name="inputMag">方向输入值</param>
        /// <param name="transform">玩家</param>
        /// <param name="cameraTransform">相机</param>
        /// <param name="slerpSpinVelocity">旋转插值速度</param>
        public void VisualSpin(float inputMag, Transform transform, Transform cameraTransform, float slerpSpinVelocity)
        {
            if (inputMag == 0)
                return;
            
            SpinSlerp(transform, cameraTransform, slerpSpinVelocity);
            if((_direction.normalized - transform.forward).sqrMagnitude < 0.0001f) return;
        }

        /// <summary>
        /// 方向旋转插值
        /// </summary>
        private void SpinSlerp(Transform transform, Transform cameraTransform, float slerpSpinVelocity)
        {
            _direction = transform.position - cameraTransform.position;
            _direction.y = 0;

            Quaternion lookRotation = Quaternion.LookRotation(_direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * slerpSpinVelocity); // TODO:自定义时间组还未使用。
        }

        public Vector3 DirectionCal(Transform transform, Transform cameraTransform)
        {
            Vector3 dir = transform.position - cameraTransform.position;
            dir.y = 0;
            return dir;
        }
    }
}

