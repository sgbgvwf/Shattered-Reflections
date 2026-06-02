using UnityEngine;
using Cinemachine;

namespace Combat.Visual
{
    /// <summary>
    /// 3D 战斗锁定摄像机（双目标）—— 固定距离版本（角度偏移）
    /// - 摄像机始终保持在玩家后方（远离敌人的外半球），距离固定为 distance。
    /// - 水平方向锁定于 A→B 连线的世界水平投影。
    /// - 摄像机在后方球面上的位置由水平/垂直角度偏移决定。
    /// - 竖直俯仰由焦点 LookAtPoint 自由决定，不再约束。
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

        [Header("距离")]
        [Tooltip("摄像机到玩家(TargetA)的固定距离")]
        public float distance = 5f;

        [Header("角度偏移 (代替原来的高度/横向偏移)")]
        [Tooltip("水平方向偏移角度（绕世界向上轴）。0 = 正后方，正数 = 右侧。")]
        [Range(-90f, 90f)] public float horizontalAngle = 0f;
        [Tooltip("垂直方向偏移角度（绕局部右轴）。0 = 平视，正数 = 俯视。")]
        [Range(-90f, 90f)] public float verticalAngle = 0f;

        [Header("视线焦点 (影响俯仰)")]
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

        // 已移除 verticalMaxAngle 相关约束

        [Header("滞后与平滑")]
        [Tooltip("位置跟随的平滑时间（越小反应越快）。")]
        public float positionSmoothTime = 0.3f;
        [Tooltip("位置跟随的最大速度（单位/秒），用于产生滞后感。")]
        public float maxFollowSpeed = 10f;
        [Tooltip("旋转平滑时间（越小反应越灵敏）。")]
        public float rotationSmoothTime = 0.2f;

        // 公开属性：从 A 指向 B 的世界方向（每帧更新）
        public Vector3 DirectionAtoB { get; private set; }

        // 缓存组件与平滑状态
        private Transform camTransform;
        private Vector3 velocityPos;
        private Vector3 velocityRotEuler;

        private void Start()
        {
            CacheComponents();
        }

        private void FixedUpdate()
        {
            if (virtualCamera == null || targetA == null) return;

            if (camTransform == null || camTransform != virtualCamera.transform)
                camTransform = virtualCamera.transform;

            Vector3 aPos = targetA.position;

            // ---------- 方向向量 ----------
            Vector3 dirAtoB;
            Vector3 abHorizontalDir;
            Vector3 behindDir;
            Vector3 rightDir;

            bool hasValidB = targetB != null && (targetB.position - aPos).sqrMagnitude > 0.001f;
            if (hasValidB)
            {
                dirAtoB = (targetB.position - aPos).normalized;
            }
            else
            {
                dirAtoB = Vector3.ProjectOnPlane(targetA.forward, Vector3.up);
                if (dirAtoB.sqrMagnitude < 0.001f) dirAtoB = Vector3.forward;
                else dirAtoB.Normalize();
            }

            abHorizontalDir = Vector3.ProjectOnPlane(dirAtoB, Vector3.up);
            if (abHorizontalDir.sqrMagnitude < 0.001f) abHorizontalDir = Vector3.forward;
            else abHorizontalDir.Normalize();

            behindDir = -abHorizontalDir;
            rightDir = Vector3.Cross(Vector3.up, behindDir).normalized;
            DirectionAtoB = dirAtoB;

            // ---------- 视线焦点 ----------
            Vector3 lookAtPoint = GetLookAtPoint(aPos, abHorizontalDir, rightDir, dirAtoB);

            // ---------- 摄像机理想位置（角度偏移）----------
            Vector3 targetPosition = CalculatePositionWithAngles(aPos, behindDir, rightDir);

            // ---------- 位置平滑 ----------
            Vector3 newPosition = Vector3.SmoothDamp(
                camTransform.position, targetPosition,
                ref velocityPos, positionSmoothTime, maxFollowSpeed, Time.deltaTime);

            // ---------- 视线方向（水平锁定 + 焦点俯仰）----------
            Vector3 desiredForward = ComputeLookDirection(newPosition, lookAtPoint, abHorizontalDir);

            // ---------- 旋转平滑 ----------
            Quaternion targetRotation = Quaternion.LookRotation(desiredForward, Vector3.up);
            Quaternion newRotation = SmoothDampQuaternion(
                camTransform.rotation, targetRotation, ref velocityRotEuler, rotationSmoothTime, Time.deltaTime);

            // ---------- 应用 ----------
            camTransform.position = newPosition;
            camTransform.rotation = newRotation;
        }

        /// <summary>
        /// 根据角度偏移计算固定距离的摄像机位置。
        /// 基础方向为玩家后方，先后绕世界Y轴（水平角）和局部右轴（垂直角）旋转。
        /// </summary>
        private Vector3 CalculatePositionWithAngles(Vector3 aPos, Vector3 behindDir, Vector3 rightDir)
        {
            // 水平旋转（绕世界Y轴）
            Quaternion horizontalRot = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
            Vector3 camDir = horizontalRot * behindDir;

            // 垂直旋转（绕当前方向的右向量）
            Vector3 currentRight = Vector3.Cross(Vector3.up, camDir).normalized;
            Quaternion verticalRot = Quaternion.AngleAxis(verticalAngle, currentRight);
            camDir = verticalRot * camDir;

            return aPos + camDir * distance;
        }

        /// <summary>
        /// 视线焦点世界坐标（基于模式 + 偏移）
        /// </summary>
        private Vector3 GetLookAtPoint(Vector3 aPos, Vector3 abHorizontalDir, Vector3 rightDir, Vector3 dirAtoB)
        {
            Vector3 basePoint = lookAtMode switch
            {
                LookAtMode.TargetA => aPos,
                LookAtMode.TargetB => targetB != null ? targetB.position : aPos,
                _ => targetB != null ? (aPos + targetB.position) * 0.5f : aPos
            };

            return basePoint
                + rightDir * lookAtOffset.x
                + Vector3.up * lookAtOffset.y
                + dirAtoB * lookAtOffset.z;
        }

        /// <summary>
        /// 计算最终视线方向：
        /// - 水平分量严格等于 horizontalDir（锁定敌人水平方位）。
        /// - 竖直分量由摄像机到焦点的俯仰角自由决定。
        /// </summary>
        private Vector3 ComputeLookDirection(Vector3 cameraPos, Vector3 lookAtPoint, Vector3 horizontalDir)
        {
            Vector3 toLook = lookAtPoint - cameraPos;
            if (toLook.sqrMagnitude < 0.0001f)
                return horizontalDir;

            toLook.Normalize();

            // 利用焦点方向的俯仰角，构建锁定水平方向的新方向
            float pitch = Mathf.Atan2(toLook.y, new Vector2(toLook.x, toLook.z).magnitude);
            float cosPitch = Mathf.Cos(pitch);
            float sinPitch = Mathf.Sin(pitch);

            return (horizontalDir * cosPitch + Vector3.up * sinPitch).normalized;
        }

        /// <summary>
        /// 四元数平滑旋转（欧拉角 SmoothDamp）
        /// </summary>
        private Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target,
            ref Vector3 velocityEuler, float smoothTime, float deltaTime)
        {
            Vector3 curEuler = current.eulerAngles;
            Vector3 tarEuler = target.eulerAngles;

            Vector3 delta = tarEuler - curEuler;
            delta.x = Mathf.DeltaAngle(0f, delta.x);
            delta.y = Mathf.DeltaAngle(0f, delta.y);
            delta.z = Mathf.DeltaAngle(0f, delta.z);

            Vector3 newEuler = Vector3.SmoothDamp(curEuler, curEuler + delta,
                ref velocityEuler, smoothTime, float.PositiveInfinity, deltaTime);
            return Quaternion.Euler(newEuler);
        }

        private void CacheComponents()
        {
            if (virtualCamera == null)
            {
                virtualCamera = GetComponent<CinemachineVirtualCamera>();
                if (virtualCamera == null)
                    Debug.LogError("VisualLock: 未指定 Virtual Camera，且当前物体上没有 CinemachineVirtualCamera 组件。");
            }
            if (virtualCamera != null)
                camTransform = virtualCamera.transform;
        }

        private void OnDisable()
        {
            velocityPos = Vector3.zero;
            velocityRotEuler = Vector3.zero;
        }

        // ==================== 可视化（已适配角度偏移） ====================
        private void OnDrawGizmosSelected()
        {
            if (targetA == null) return;

            Vector3 aPos = targetA.position;
            Vector3 dirAtoB;
            bool hasValidB = targetB != null && (targetB.position - aPos).sqrMagnitude > 0.001f;
            if (hasValidB)
                dirAtoB = (targetB.position - aPos).normalized;
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

            Vector3 lookAtPoint = GetLookAtPoint(aPos, abHorizontalDir, rightDir, dirAtoB);
            Vector3 idealCamPos = CalculatePositionWithAngles(aPos, behindDir, rightDir);

            // 目标和 AB 线
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(aPos, 0.2f);
            if (targetB != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(targetB.position, 0.2f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(aPos, targetB.position);
            }

            // 摄像机位置
            if (virtualCamera != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(virtualCamera.transform.position, 0.15f);
                Gizmos.DrawLine(virtualCamera.transform.position, idealCamPos);
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(idealCamPos, 0.25f);
            Gizmos.DrawLine(aPos, idealCamPos);

            // 固定距离球体
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f);
            Gizmos.DrawWireSphere(aPos, distance);

            // 水平锁定方向
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(aPos, abHorizontalDir * 2f);

            // 焦点
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lookAtPoint, 0.15f);
            if (virtualCamera != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawRay(virtualCamera.transform.position, virtualCamera.transform.forward * 2f);
            }

            // 角度偏移示意（绘制后方基准方向与偏移后方向）
            Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
            Gizmos.DrawRay(aPos, behindDir * distance);
            Gizmos.DrawRay(aPos, (idealCamPos - aPos).normalized * distance);
        }
    }
}