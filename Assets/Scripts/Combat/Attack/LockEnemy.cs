using UnityEngine;

namespace Combat.LockVisual
{
    public class LockEnemy : MonoBehaviour
    {
        [Header("检测形状")]
        [SerializeField] private LockDetectShape _detectionShape;

        [Header("目标筛选")]
        [SerializeField] private bool _useTag = true;
        [SerializeField] private bool _useLayer = true;
        [SerializeField] private string _enemyTag = "Enemy";
        [SerializeField] private LayerMask _enemyLayer;

        [Header("目标评分权重")]
        [SerializeField] [Range(0f, 1f)] private float _distanceWeight;
        [SerializeField] [Range(0f, 1f)] private float _cameraCenterWeight;

        [Header("Box 参数")]
        [SerializeField] private Vector3 _boxHalfExtents;
        [SerializeField] private Vector3 _boxOffset;

        [Header("Sphere 参数")]
        [SerializeField] private float _sphereRadius;
        [SerializeField] private Vector3 _sphereOffset;

        [Header("Gizmos")]
        [SerializeField] private Color _gizmoColor = new Color(1f, 0f, 0f, 0.2f);

        private Camera _mainCamera;
        private Transform _cameraTransform;
        private Collider[] _resultBuffer = new Collider[128];

        private enum LockDetectShape
        {
            Box,
            Sphere
        }

        /// <summary> 当前锁定的目标（null表示未锁定） </summary>
        public Transform CurrentTarget { get; private set; }

        private void Awake()
        {
            _mainCamera = Camera.main;
            if (_mainCamera != null)
                _cameraTransform = _mainCamera.transform;
        }

        /// <summary> 执行检测，返回评分最优的单个目标 </summary>
        public Transform DetectBestTarget()
        {
            int hitCount = PerformOverlap();
            Transform best = SelectBestTarget(hitCount);
            CurrentTarget = best;
            return best;
        }

        /// <summary> 根据当前形状执行物理检测，返回命中数量 </summary>
        private int PerformOverlap()
        {
            return _detectionShape switch
            {
                LockDetectShape.Box => Physics.OverlapBoxNonAlloc(
                    transform.position + _boxOffset,
                    _boxHalfExtents,
                    _resultBuffer,
                    Quaternion.identity),

                LockDetectShape.Sphere => Physics.OverlapSphereNonAlloc(
                    transform.position + _sphereOffset,
                    _sphereRadius,
                    _resultBuffer),

                _ => 0
            };
        }

        /// <summary> 从命中结果中筛选Enemy并返回评分最优者 </summary>
        private Transform SelectBestTarget(int hitCount)
        {
            Transform best = null;
            float bestScore = float.MaxValue;

            Ray cameraRay = default;
            bool hasCameraRay = false;
            if (_mainCamera != null && _cameraTransform != null)
            {
                cameraRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                hasCameraRay = true;
            }

            for (int i = 0; i < hitCount; i++)
            {
                var col = _resultBuffer[i];
                if (col == null) continue;

                if (!IsEnemy(col)) continue;

                Vector3 targetPos = col.transform.position;

                float score = CalculateScore(targetPos, cameraRay, hasCameraRay);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = col.transform;
                }
            }

            return best;
        }

        private bool IsEnemy(Collider col)
        {
            if (_useTag && col.CompareTag(_enemyTag))
                return true;
            if (_useLayer && (_enemyLayer.value & (1 << col.gameObject.layer)) != 0)
                return true;
            return false;
        }

        /// <summary> 加权评分：玩家距离 + 摄像机中心射线距离 </summary>
        private float CalculateScore(Vector3 targetPos, Ray cameraRay, bool hasCameraRay)
        {
            float distToPlayer = Vector3.Distance(transform.position, targetPos);

            float distToCameraCenter = 0f;
            if (hasCameraRay)
                distToCameraCenter = DistToRay(cameraRay, targetPos);

            return _distanceWeight * distToPlayer + _cameraCenterWeight * distToCameraCenter;
        }

        private static float DistToRay(Ray ray, Vector3 point)
        {
            Vector3 toPoint = point - ray.origin;
            float t = Vector3.Dot(toPoint, ray.direction);
            t = Mathf.Max(0, t);
            return Vector3.Distance(point, ray.origin + ray.direction * t);
        }

        // ==================== Gizmos ====================

        private void DrawShapeGizmo()
        {
            switch (_detectionShape)
            {
                case LockDetectShape.Box:
                    DrawBox();
                    break;
                case LockDetectShape.Sphere:
                    DrawSphereShape();
                    break;
            }
        }

        private void DrawBox()
        {
            Vector3 center = transform.position + _boxOffset;
            Color wireColor = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 1f);

            Gizmos.color = _gizmoColor;
            Gizmos.DrawCube(center, _boxHalfExtents * 2f);
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(center, _boxHalfExtents * 2f);
        }

        private void DrawSphereShape()
        {
            Vector3 center = transform.position + _sphereOffset;
            Color wireColor = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 1f);

            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(center, _sphereRadius);
            Gizmos.color = wireColor;
            Gizmos.DrawWireSphere(center, _sphereRadius);
        }
    }
}
