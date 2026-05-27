using UnityEngine;
using Cinemachine;

namespace Combat.Visual
{

    /// <summary>
    /// 3D 战斗锁定摄像机（双目标）—— 最终版本
    /// - 位置跟随 AB 的相对关系，不受目标 A 自身旋转影响。
    /// - 水平方向锁定于 A→B 连线的世界水平投影。
    /// - 竖直方向由焦点 LookAtPoint 决定，并约束在 AB 连线俯仰角 ± VerticalMaxAngle 范围内。
    /// - 所有过渡均平滑处理。
    /// 
    /// 使用前请将虚拟摄像机上的 Body/Aim 设为 Do Nothing。
    /// </summary>
    public class VisualLock : MonoBehaviour
    {
        [Header("目标")]
        [Tooltip("摄像机位置跟随的主目标（玩家）")]
        public Transform targetA;
        [Tooltip("需要保持在视野内的次要目标（敌人）")]
        public Transform targetB;

        [Header("Cinemachine 组件")]
        [Tooltip("要控制的虚拟摄像机（需将 Body 和 Aim 设为 Do Nothing）")]
        public CinemachineVirtualCamera virtualCamera;

        [Header("位置跟随 (基于 AB 相对关系)")]
        [Tooltip("偏移量释义：x = 左右偏移（沿 world right 方向），y = 高度偏移（世界向上），z = 前后偏移（沿 A 的后方，正值更远）。")]
        public Vector3 followOffset = new Vector3(0, 2, -5);

        [Header("视线焦点 (只影响竖直方向的俯仰)")]
        [Tooltip("用于计算俯仰的目标点：TargetA、TargetB 或两者的中点。")]
        public LookAtMode lookAtMode = LookAtMode.Midpoint;
        [Tooltip("相对于所选目标点的额外偏移（基于 AB 方向的世界空间偏移）。x = 左右，y = 上下，z = 沿 AB 方向。")]
        public Vector3 lookAtOffset = new Vector3(0, 0, 1.5f);

        public enum LookAtMode
        {
            TargetA,
            TargetB,
            Midpoint
        }

        [Header("角度约束")]
        [Tooltip("竖直方向允许的最大俯仰偏差。摄像机视线俯仰角与 AB 连线俯仰角的差值将被钳制在此范围内。")]
        [Range(0f, 90f)] public float verticalMaxAngle = 30f;

        [Header("滞后与平滑")]
        [Tooltip("位置跟随的平滑时间（越小反应越快）。")]
        public float positionSmoothTime = 0.3f;
        [Tooltip("位置跟随的最大速度（单位/秒），用于产生滞后感。")]
        public float maxFollowSpeed = 10f;
        [Tooltip("旋转平滑时间（越小反应越灵敏）。")]
        public float rotationSmoothTime = 0.2f;

        // 公开属性：从 A 指向 B 的世界方向（每帧更新）
        public Vector3 DirectionAtoB { get; private set; }

        // 内部速度缓存
        private Vector3 velocityPos;
        private Vector3 velocityRotEuler;

        private void Start()
        {
            if (virtualCamera == null)
            {
                virtualCamera = GetComponent<CinemachineVirtualCamera>();
                if (virtualCamera == null)
                    Debug.LogError("VisualLock: 未指定 Virtual Camera，且当前物体上没有 CinemachineVirtualCamera 组件。");
            }
        }

        private void FixedUpdate()
        {
            if (virtualCamera == null || targetA == null) return;

            // ---------- 预计算所有方向向量（一次计算，多次使用）----------
            Vector3 dirAtoB;          // 从 A 指向 B 的原始方向（未归一化或零向量处理）
            Vector3 abHorizontalDir;  // dirAtoB 在水平面上的投影方向
            Vector3 behindDir;        // A 的后方水平方向（与 abHorizontalDir 相反）
            Vector3 rightDir;         // 与 behindDir 正交的右侧水平方向

            bool hasValidB = targetB != null && (targetB.position - targetA.position).sqrMagnitude > 0.001f;
            if (hasValidB)
            {
                dirAtoB = (targetB.position - targetA.position).normalized;
            }
            else
            {
                // 无次要目标时，用 A 的前方水平投影作为参考
                dirAtoB = Vector3.ProjectOnPlane(targetA.forward, Vector3.up);
                if (dirAtoB == Vector3.zero) dirAtoB = Vector3.forward;
                else dirAtoB.Normalize();
            }

            abHorizontalDir = Vector3.ProjectOnPlane(dirAtoB, Vector3.up);
            // 如果 AB 完全是垂直的，水平投影可能为零，此时回退到 world forward
            if (abHorizontalDir.sqrMagnitude < 0.001f) abHorizontalDir = Vector3.forward;
            else abHorizontalDir.Normalize();

            behindDir = -abHorizontalDir;
            rightDir = Vector3.Cross(Vector3.up, behindDir).normalized;

            // 更新公开属性
            DirectionAtoB = dirAtoB;

            // ---------- 计算视线焦点 ----------
            Vector3 lookAtPoint = GetLookAtPoint(abHorizontalDir, rightDir, dirAtoB);

            // ---------- 计算摄像机理想位置 ----------
            Vector3 targetPosition = targetA.position 
                + behindDir * followOffset.z
                + Vector3.up * followOffset.y
                + rightDir * followOffset.x;

            // ---------- 位置平滑 ----------
            Transform camTransform = virtualCamera.transform;
            Vector3 newPosition = Vector3.SmoothDamp(
                camTransform.position, targetPosition,
                ref velocityPos, positionSmoothTime, maxFollowSpeed);

            // ---------- 计算约束后的最终视线方向 ----------
            Vector3 desiredForward = CalculateConstrainedForward(
                newPosition, lookAtPoint, abHorizontalDir, dirAtoB);

            // ---------- 旋转平滑 ----------
            Quaternion targetRotation = Quaternion.LookRotation(desiredForward, Vector3.up);
            Quaternion newRotation = SmoothDampQuaternion(
                camTransform.rotation, targetRotation, ref velocityRotEuler, rotationSmoothTime);

            // ---------- 应用到虚拟摄像机 ----------
            camTransform.position = newPosition;
            camTransform.rotation = newRotation;
        }

        /// <summary>
        /// 根据预计算的方向向量获取视线焦点世界坐标。
        /// </summary>
        private Vector3 GetLookAtPoint(Vector3 abHorizontalDir, Vector3 rightDir, Vector3 dirAtoB)
        {
            Vector3 basePoint = lookAtMode switch
            {
                LookAtMode.TargetA => targetA.position,
                LookAtMode.TargetB => targetB != null ? targetB.position : targetA.position,
                _ => targetB != null ? (targetA.position + targetB.position) * 0.5f : targetA.position
            };

            // 偏移基于世界方向，不依赖 A 的旋转
            return basePoint 
                + rightDir * lookAtOffset.x 
                + Vector3.up * lookAtOffset.y 
                + dirAtoB * lookAtOffset.z;
        }

        /// <summary>
        /// 计算最终约束后的视线方向：
        /// - 水平分量严格等于 horizontalDir。
        /// - 竖直分量受 AB 俯仰约束。
        /// </summary>
        private Vector3 CalculateConstrainedForward(
            Vector3 cameraPos, Vector3 lookAtPoint, 
            Vector3 horizontalDir, Vector3 dirAtoB)
        {
            if (targetB == null)
            {
                // 无次要目标时，直接看向焦点
                return (lookAtPoint - cameraPos).normalized;
            }

            // 计算 AB 连线的俯仰角
            float abPitch = Mathf.Asin(dirAtoB.y) * Mathf.Rad2Deg;

            // 看向焦点的理想俯仰角
            Vector3 dirToLook = (lookAtPoint - cameraPos).normalized;
            float horizontalLength = Vector3.ProjectOnPlane(dirToLook, Vector3.up).magnitude;
            float idealPitch = Mathf.Atan2(dirToLook.y, Mathf.Max(horizontalLength, 0.001f)) * Mathf.Rad2Deg;

            // 限制偏差
            float pitchDiff = idealPitch - abPitch;
            float clampedDiff = Mathf.Clamp(pitchDiff, -verticalMaxAngle, verticalMaxAngle);
            float targetPitch = abPitch + clampedDiff;

            // 重建方向
            float newY = Mathf.Sin(targetPitch * Mathf.Deg2Rad);
            float newHorizontalLength = Mathf.Cos(targetPitch * Mathf.Deg2Rad);
            return (horizontalDir * newHorizontalLength + Vector3.up * newY).normalized;
        }

        /// <summary>
        /// 四元数平滑旋转（基于欧拉角的 SmoothDamp）。
        /// </summary>
        private Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target,
            ref Vector3 velocityEuler, float smoothTime)
        {
            Vector3 curEuler = current.eulerAngles;
            Vector3 tarEuler = target.eulerAngles;

            Vector3 delta = tarEuler - curEuler;
            delta.x = Mathf.DeltaAngle(0f, delta.x);
            delta.y = Mathf.DeltaAngle(0f, delta.y);
            delta.z = Mathf.DeltaAngle(0f, delta.z);

            Vector3 newEuler = Vector3.SmoothDamp(curEuler, curEuler + delta,
                ref velocityEuler, rotationSmoothTime);
            return Quaternion.Euler(newEuler);
        }

        // ==================== 可视化 ====================
        private void OnDrawGizmosSelected()
        {
            if (targetA == null) return;

            // 预先计算方向（可能未运行，所以重复一次计算，不影响性能）
            Vector3 dirAtoB;
            bool hasValidB = targetB != null && (targetB.position - targetA.position).sqrMagnitude > 0.001f;
            if (hasValidB)
                dirAtoB = (targetB.position - targetA.position).normalized;
            else
            {
                dirAtoB = Vector3.ProjectOnPlane(targetA.forward, Vector3.up);
                if (dirAtoB == Vector3.zero) dirAtoB = Vector3.forward;
                else dirAtoB.Normalize();
            }

            Vector3 abHorizontalDir = Vector3.ProjectOnPlane(dirAtoB, Vector3.up);
            if (abHorizontalDir.sqrMagnitude < 0.001f) abHorizontalDir = Vector3.forward;
            else abHorizontalDir.Normalize();

            Vector3 behindDir = -abHorizontalDir;
            Vector3 rightDir = Vector3.Cross(Vector3.up, behindDir).normalized;

            Vector3 lookAtPoint = GetLookAtPoint(abHorizontalDir, rightDir, dirAtoB);

            Vector3 idealCamPos = targetA.position 
                + behindDir * followOffset.z 
                + Vector3.up * followOffset.y 
                + rightDir * followOffset.x;

            // 目标和 AB 线
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetA.position, 0.2f);
            if (targetB != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(targetB.position, 0.2f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(targetA.position, targetB.position);
            }

            // 摄像机理想位置与实际位置
            if (virtualCamera != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(virtualCamera.transform.position, 0.15f);
                Gizmos.DrawLine(virtualCamera.transform.position, idealCamPos);
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(idealCamPos, 0.25f);
            Gizmos.DrawLine(targetA.position, idealCamPos);

            // 水平锁定方向
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(targetA.position, abHorizontalDir * 2f);

            // 视线焦点
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lookAtPoint, 0.15f);
            if (virtualCamera != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawRay(virtualCamera.transform.position, virtualCamera.transform.forward * 2f);
            }

            // 竖直俯仰约束示意
            if (targetB != null && virtualCamera != null)
            {
                Vector3 camPos = virtualCamera.transform.position;
                float abPitch = Mathf.Asin(dirAtoB.y) * Mathf.Rad2Deg;
                float highPitch = abPitch + verticalMaxAngle;
                float lowPitch = abPitch - verticalMaxAngle;

                Vector3 highDir = (abHorizontalDir * Mathf.Cos(highPitch * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(highPitch * Mathf.Deg2Rad)).normalized;
                Vector3 lowDir = (abHorizontalDir * Mathf.Cos(lowPitch * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(lowPitch * Mathf.Deg2Rad)).normalized;

                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                Gizmos.DrawRay(camPos, highDir * 2f);
                Gizmos.DrawRay(camPos, lowDir * 2f);

                Gizmos.color = Color.blue;
                Vector3 desired = (lookAtPoint - camPos).normalized;
                Gizmos.DrawRay(camPos, desired * 2f);
            }
        }
    }
}
