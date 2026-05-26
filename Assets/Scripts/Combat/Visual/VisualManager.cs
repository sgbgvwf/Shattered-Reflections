using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Combat.Move;

namespace Combat.Visual
{
    public class VisualManager : MonoBehaviour
    {

        [Header("相机")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private CinemachineFreeLook _freeLock;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private CinemachineTargetGroup _targetGroup;

        [SerializeField] 
        private enum VisualMode
        {
            TraceOnly,  // 跟踪玩家
            TraceGroup, // 跟踪物体组
        }

        [Header("摄像机跟随模式")]
        [SerializeField] private VisualMode _visualMode;
        private Dictionary<VisualMode, CinemachineVirtualCameraBase> _cinemachineModeDict = new Dictionary<VisualMode, CinemachineVirtualCameraBase>();

        [Header("移动")]
        [SerializeField] private MoveController _moveController;

        [Header("旋转插值")]
        [Range(0f, 100f), Tooltip("插值速度")]
        [SerializeField] private float _slerpSpinVelocity = 20f;

        private CinemachineVirtualCameraBase _currentCinemachine;

        private VisualFree visualFree = new VisualFree();

        private VisualLock visualLock = new VisualLock();

        void Start()
        {
            StartRegistCinemachine();
            SetCinemachine(_visualMode);
        }


        void Update()
        {
            VisualUpdate();
        }

        private void StartRegistCinemachine()
        {
            _cinemachineModeDict?.Add(VisualMode.TraceOnly, _freeLock);
            _cinemachineModeDict?.Add(VisualMode.TraceGroup, _virtualCamera);
        }

        private void SetCinemachine(VisualMode mode)
        {
            _visualMode = mode;
            _currentCinemachine = _cinemachineModeDict[mode];
        }

        private void VisualUpdate()
        {
            if(_visualMode == VisualMode.TraceOnly)
            {
                visualFree.VisualSpin(
                _moveController._inputMove.magnitude, 
                transform,
                _cameraTransform,
                _slerpSpinVelocity);
            }
        }

        //TODO:两种视角的指定切换方法
    } 
}

