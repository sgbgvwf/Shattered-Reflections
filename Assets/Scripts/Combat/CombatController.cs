using UnityEngine;
using Core.Data;
using Core.Input;
using Combat.Visual;
using Combat.LockVisual;

namespace Combat
{
    /// <summary>
    /// 战斗控制器：编排锁定敌人、摄像机切换、黑板同步。
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        [Header("输入意图")]
        [SerializeField] private InputIntention _inputIntention;

        [Header("锁定检测")]
        [SerializeField] private LockEnemy _lockEnemy;

        [Header("视觉")]
        [SerializeField] private VisualManager _visualManager;
        [SerializeField] private VisualLock _visualLock;
        [SerializeField] private LockIndicator _lockIndicator;

        [Header("黑板")]
        [SerializeField] private PlayerBlackboard _blackboard;

        [Header("资源")]
        public int attackCount;
        public float skillEnegy;
        public float finisherEnegy;

        /// <summary> 当前锁定的目标（同步到行为树黑板） </summary>
        public Transform lockObject { get; private set; }

        private bool _isLocked;

        private void Start()
        {
            _visualLock.targetA = transform;
        }

        private void Update()
        {
            if (_inputIntention != null && _inputIntention.ConsumeLockIntent)
                ToggleLock();

            if (_isLocked)
            {
                if (lockObject == null || !lockObject.gameObject.activeInHierarchy)
                {
                    Unlock();
                    return;
                }
                _blackboard.TargetDistance = Vector3.Distance(transform.position, lockObject.position);
            }
        }

        private void ToggleLock()
        {
            if (_isLocked)
                Unlock();
            else
                Lock();
        }

        private void Lock()
        {
            Transform target = _lockEnemy.DetectBestTarget();
            if (target == null) return;

            lockObject = target;
            _visualLock.targetB = target;
            _visualManager.SetVisualMode(VisualMode.TraceLock);
            if (_lockIndicator != null)
                _lockIndicator.SetTarget(target);

            _blackboard.HasTarget = true;
            _isLocked = true;
        }

        private void Unlock()
        {
            lockObject = null;
            _visualLock.targetB = null;
            _visualManager.SetVisualMode(VisualMode.TraceFree);
            if (_lockIndicator != null)
                _lockIndicator.SetTarget(null);

            _blackboard.HasTarget = false;
            _blackboard.TargetDistance = 0f;
            _isLocked = false;
        }
    }
}
