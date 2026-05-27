using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Combat.Move;
using Combat;

namespace Combat.Visual
{
    public class VisualManager : MonoBehaviour
    {

        [Header("相机")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private CinemachineFreeLook _freeLock;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;


        [SerializeField] 
        private enum VisualMode
        {
            TraceFree,  // 自由视角
            TraceLock,  // 锁定视角
        }

        [Header("摄像机跟随模式")]
        [SerializeField] private VisualMode _visualMode;
        private Dictionary<VisualMode, CinemachineVirtualCameraBase> _cinemachineModeDict = new Dictionary<VisualMode, CinemachineVirtualCameraBase>();

        [SerializeField] private VisualLock _visualLock;

        [Header("移动")]
        [SerializeField] private MoveController _moveController;

        [Header("旋转插值")]
        [Range(0f, 100f), Tooltip("插值速度")]
        [SerializeField] private float _slerpSpinVelocity = 20f;



        private VisualFree _visualFree = new VisualFree();



        private PlayerRotation _playerRotation = new PlayerRotation();

        void Start()
        {
            StartRegistCinemachine();
        }


        void Update()
        {
            VisualUpdate();
        }

        private void StartRegistCinemachine()
        {
            _cinemachineModeDict?.Add(VisualMode.TraceFree, _freeLock);
            _cinemachineModeDict?.Add(VisualMode.TraceLock, _virtualCamera);
        }


        private void VisualUpdate()
        {
            if(_visualMode == VisualMode.TraceFree)
            {
                if(_moveController._inputMove.magnitude == 0) return;

                _playerRotation.RotationSlerp(transform, _visualFree.DirectionCal(transform, _cameraTransform), _slerpSpinVelocity);
            }
            else if(_visualMode == VisualMode.TraceLock)
            {
                _playerRotation.RotationSlerp(transform, _visualLock.DirectionAtoB, _slerpSpinVelocity);
            }
        }

        //TODO:两种视角的指定切换方法
        private void SetVisualMode(VisualMode visualMode)
        {
            _visualMode = visualMode;
        }
        





    } 
}

