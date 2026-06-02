using UnityEngine;
using Cinemachine;
using Combat.Move;
using Core.Input;

namespace Combat.Visual
{
    public enum VisualMode
    {
        TraceFree,  // 自由视角
        TraceLock,  // 锁定视角
    }

    public class VisualManager : MonoBehaviour
    {
        [Header("相机")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private CinemachineFreeLook _freeLock;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;

        [Header("摄像机跟随模式")]
        [SerializeField] private VisualMode _visualMode;

        [SerializeField] private VisualLock _visualLock;

        [Header("移动")]
        [SerializeField] private MoveController _moveController;

        [Header("旋转插值")]
        [Range(0f, 100f), Tooltip("插值速度")]
        [SerializeField] private float _slerpSpinVelocity = 20f;

        [Header("缩放")]
        [SerializeField] private InputIntention _inputIntention;
        [SerializeField] [Range(0.01f, 5f)] private float _zoomSensitivity = 0.5f;
        [SerializeField] private float _zoomSmoothTime = 0.15f;

        [Header("自由视角缩放范围")]
        [SerializeField] private float _freeMinDistance;
        [SerializeField] private float _freeMaxDistance;

        [Header("锁定视角缩放范围")]
        [SerializeField] private float _lockMinDistance;
        [SerializeField] private float _lockMaxDistance;

        // FreeLook 独立缩放状态：每个轨道保持各自初始比例
        private float _freeCurrent, _freeTarget, _freeVelocity;
        private float[] _freeInitRadius = new float[3];
        private float[] _freeInitHeight = new float[3];

        // VisualLock 独立缩放状态（仅距离）
        private float _lockCurrent, _lockTarget, _lockVelocity;

        private VisualFree _visualFree = new VisualFree();
        private PlayerRotation _playerRotation = new PlayerRotation();

        void Start()
        {
            if (_freeLock != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    _freeInitRadius[i] = _freeLock.m_Orbits[i].m_Radius;
                    _freeInitHeight[i] = _freeLock.m_Orbits[i].m_Height;
                }
                _freeCurrent = _freeInitRadius[1]; // 中轨道作为缩放基准
            }
            if (_visualLock != null)
            {
                _lockCurrent = _visualLock.distance;
                // 注意：不再需要计算高度比例，因为角度偏移是固定的
            }
            _freeTarget = _freeCurrent;
            _lockTarget = _lockCurrent;
            SwitchCamera(_visualMode);
        }

        void Update()
        {
            HandleZoom();
            VisualUpdate();
        }

        /// <summary> 切换视角模式，激活对应相机并关闭另一个 </summary>
        public void SetVisualMode(VisualMode visualMode)
        {
            if (visualMode != _visualMode)
                TransferZoom(visualMode);

            _visualMode = visualMode;
            SwitchCamera(visualMode);
        }

        /// <summary> 切换时按百分比映射缩放，保持相对距离一致 </summary>
        private void TransferZoom(VisualMode toMode)
        {
            if (toMode == VisualMode.TraceFree)
            {
                float t = _lockMaxDistance > _lockMinDistance
                    ? Mathf.InverseLerp(_lockMinDistance, _lockMaxDistance, _lockTarget)
                    : 0f;
                _freeTarget = Mathf.Lerp(_freeMinDistance, _freeMaxDistance, t);
                _freeCurrent = _freeTarget;
            }
            else
            {
                float t = _freeMaxDistance > _freeMinDistance
                    ? Mathf.InverseLerp(_freeMinDistance, _freeMaxDistance, _freeTarget)
                    : 0f;
                _lockTarget = Mathf.Lerp(_lockMinDistance, _lockMaxDistance, t);
                _lockCurrent = _lockTarget;
            }
        }

        private void SwitchCamera(VisualMode mode)
        {
            bool isFree = mode == VisualMode.TraceFree;

            // 切换到自由视角时，将 FreeLook 轴值拉到 VisualLock 相机附近
            if (isFree && _freeLock != null && _visualLock != null && _virtualCamera != null)
            {
                Vector3 toCam = _virtualCamera.transform.position - transform.position;
                if (toCam.sqrMagnitude > 0.001f)
                {
                    Vector3 toCamH = Vector3.ProjectOnPlane(toCam, Vector3.up).normalized;
                    Vector3 fwdH = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                    if (fwdH.sqrMagnitude < 0.001f) fwdH = Vector3.forward;
                    fwdH.Normalize();

                    float x = Vector3.SignedAngle(-fwdH, toCamH, Vector3.up);
                    if (x < 0f) x += 360f;

                    float targetH = toCam.y;
                    float bottomH = _freeInitHeight[0];
                    float midH = _freeInitHeight[1];
                    float topH = _freeInitHeight[2];

                    float y;
                    if (targetH >= midH)
                    {
                        float t = Mathf.Approximately(topH, midH) ? 0f : (targetH - midH) / (topH - midH);
                        y = 0.5f + 0.5f * t;
                    }
                    else
                    {
                        float t = Mathf.Approximately(midH, bottomH) ? 0f : (targetH - bottomH) / (midH - bottomH);
                        y = 0.5f * t;
                    }
                    y = Mathf.Clamp(y, 0f, 1f);

                    _freeLock.m_XAxis.Value = x;
                    _freeLock.m_YAxis.Value = y;
                }
            }

            if (_freeLock != null)
                _freeLock.gameObject.SetActive(isFree);

            if (_virtualCamera != null)
                _virtualCamera.gameObject.SetActive(!isFree);
        }

        private void HandleZoom()
        {
            if (_inputIntention == null) return;

            float scroll = _inputIntention.ViewScaleIntent.y;

            if (_visualMode == VisualMode.TraceFree && _freeLock != null)
            {
                if (!Mathf.Approximately(scroll, 0f))
                {
                    _freeTarget -= scroll * _zoomSensitivity;
                    _freeTarget = Mathf.Clamp(_freeTarget, _freeMinDistance, _freeMaxDistance);
                }
                _freeCurrent = Mathf.SmoothDamp(_freeCurrent, _freeTarget, ref _freeVelocity, _zoomSmoothTime);
                float factor = _freeInitRadius[1] > 0f ? _freeCurrent / _freeInitRadius[1] : 1f;
                for (int i = 0; i < 3; i++)
                {
                    _freeLock.m_Orbits[i].m_Radius = _freeInitRadius[i] * factor;
                    _freeLock.m_Orbits[i].m_Height = _freeInitHeight[i] * factor;
                }
            }
            else if (_visualMode == VisualMode.TraceLock && _visualLock != null)
            {
                if (!Mathf.Approximately(scroll, 0f))
                {
                    _lockTarget -= scroll * _zoomSensitivity;
                    _lockTarget = Mathf.Clamp(_lockTarget, _lockMinDistance, _lockMaxDistance);
                }
                _lockCurrent = Mathf.SmoothDamp(_lockCurrent, _lockTarget, ref _lockVelocity, _zoomSmoothTime);
                // 仅更新距离，角度偏移保持不变
                _visualLock.distance = _lockCurrent;
                // 移除对 heightOffset 和 lateralOffset 的赋值
            }
        }

        private void VisualUpdate()
        {
            if (_visualMode == VisualMode.TraceFree)
            {
                if (_moveController._inputMove.magnitude == 0) return;
                _playerRotation.RotationSlerp(transform, _visualFree.DirectionCal(transform, _cameraTransform), _slerpSpinVelocity);
            }
            else if (_visualMode == VisualMode.TraceLock)
            {
                _playerRotation.RotationSlerp(transform, _visualLock.DirectionAtoB, _slerpSpinVelocity);
            }
        }
    }
}