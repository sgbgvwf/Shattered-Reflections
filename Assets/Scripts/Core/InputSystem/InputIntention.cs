using UnityEngine;
using Core.Data;
using System;
using Core.OwnTimer;

namespace Core.Input
{
    [System.Flags]
    [Serializable]
    public enum IntentMask
    {
        Move         = 1 << 0,
        Interact     = 1 << 1,
        Lock         = 1 << 2,
        LightAttack  = 1 << 3,   // 短按普攻
        HeavyAttack  = 1 << 4,   // 长按重击
        ViewScale    = 1 << 5,
        Jump         = 1 << 6,
        Dodge        = 1 << 7,
        Skill        = 1 << 8,
        Finisher     = 1 << 9,
        Back         = 1 << 10,
        All          = ~0
    }

    public class InputIntention : MonoBehaviour
    {
        [Header("Blackboard")]
        [SerializeField] private PlayerBlackboard _blackboard;

        [Header("Intent Masks")]
        [SerializeField] private IntentMask _disabledIntents;

        // 瞬时意图
        private bool jump, dodge, skill, finisher, interact, lockOn, back;
        private bool lightAttack, heavyAttack;   // 分离普攻和重击

        // 持续意图
        public Vector2 MoveIntent      { get; private set; }
        public Vector2 ViewScaleIntent { get; private set; }

        private InputSystemManager input;

        [SerializeField] private float _heavyChargeThreshold = 0.4f; // 长按判定阈值
        private Timer _heavyAttackTimer;

        private void Awake()
        {
            input = InputSystemManager.Instance;
            if (_blackboard == null)
                _blackboard = GetComponent<PlayerBlackboard>();
        }

        private void OnEnable()
        {
            input.OnJumpPressed      += OnJump;
            input.OnAttackPressed    += OnAttack;
            input.OnAttackReleased   += OnAttackCanceled;
            input.OnDodgePressed     += OnDodge;
            input.OnSkillPressed     += OnSkill;
            input.OnFinisherPressed  += OnFinisher;
            input.OnInteractPressed  += OnInteract;
            input.OnLockPressed      += OnLock;
            input.OnBackPressed      += OnBack;
        }

        private void OnDisable()
        {
            input.OnJumpPressed      -= OnJump;
            input.OnAttackPressed    -= OnAttack;
            input.OnAttackReleased   -= OnAttackCanceled;
            input.OnDodgePressed     -= OnDodge;
            input.OnSkillPressed     -= OnSkill;
            input.OnFinisherPressed  -= OnFinisher;
            input.OnInteractPressed  -= OnInteract;
            input.OnLockPressed      -= OnLock;
            input.OnBackPressed      -= OnBack;

            // 清理计时器
            if (_heavyAttackTimer != null && !_heavyAttackTimer.IsDone)
                Timer.Cancel(_heavyAttackTimer);
        }

        private void Update()
        {
            MoveIntent      = _disabledIntents.HasFlag(IntentMask.Move)      ? Vector2.zero : input.MoveInput;
            ViewScaleIntent = _disabledIntents.HasFlag(IntentMask.ViewScale) ? Vector2.zero : input.ViewScaleInput;
        }

        // 行为树条件节点消费意图（读取并重置）
        public bool ConsumeJumpIntent     => Consume(ref jump);
        public bool ConsumeLightAttackIntent => Consume(ref lightAttack);
        public bool ConsumeHeavyAttackIntent => Consume(ref heavyAttack);
        public bool ConsumeDodgeIntent    => Consume(ref dodge);
        public bool ConsumeSkillIntent    => Consume(ref skill);
        public bool ConsumeFinisherIntent => Consume(ref finisher);
        public bool ConsumeInteractIntent => Consume(ref interact);
        public bool ConsumeLockIntent     => Consume(ref lockOn);
        public bool ConsumeBackIntent     => Consume(ref back);

        private bool Consume(ref bool flag)
        {
            if (!flag) return false;
            flag = false;
            return true;
        }

        public void DisableIntent(IntentMask mask) => _disabledIntents |= mask;
        public void EnableIntent(IntentMask mask)  => _disabledIntents &= ~mask;
        public bool IsIntentDisabled(IntentMask mask) => (_disabledIntents & mask) != 0;

        // 轻重击
        private void OnJump() => TryIntent(IntentMask.Jump, () => _blackboard.IsGrounded && _blackboard.CanAct, () => jump = true);

        // 攻击键按下
        private void OnAttack()
        {
            if (_disabledIntents.HasFlag(IntentMask.LightAttack | IntentMask.HeavyAttack))
                return;

            // 取消未完成的长按计时器（防止快速连点）
            if (_heavyAttackTimer != null && !_heavyAttackTimer.IsDone)
                Timer.Cancel(_heavyAttackTimer);

            // 启动重击计时器，到期时自动触发重击意图
            _heavyAttackTimer = Timer.Register(
                _heavyChargeThreshold,
                onComplete: () => TryIntent(IntentMask.HeavyAttack, 
                                            () => _blackboard.CanAct, 
                                            () => heavyAttack = true),
                useRealTime: true   // 长按一般不受时停影响
            );
        }

        // 攻击键抬起
        private void OnAttackCanceled()
        {
            if (_disabledIntents.HasFlag(IntentMask.LightAttack | IntentMask.HeavyAttack))
                return;

            // 如果计时器还没完成，说明是短按，触发普攻意图并取消重击计时器
            if (_heavyAttackTimer != null && !_heavyAttackTimer.IsDone)
            {
                Timer.Cancel(_heavyAttackTimer);
                TryIntent(IntentMask.LightAttack, 
                          () => _blackboard.CanAct, 
                          () => lightAttack = true);
            }
            // 若计时器已完成，重击意图已触发，这里无需额外操作
        }

        private void OnDodge()    => TryIntent(IntentMask.Dodge,    () => _blackboard.CanAct,                          () => dodge = true);
        private void OnSkill()    => TryIntent(IntentMask.Skill,    () => _blackboard.CanAct,                          () => skill = true);
        private void OnFinisher() => TryIntent(IntentMask.Finisher, () => _blackboard.CanAct,                          () => finisher = true);
        private void OnInteract() => TryIntent(IntentMask.Interact, () => _blackboard.CanAct && _blackboard.CanInteract,() => interact = true);
        private void OnLock()     => TryIntent(IntentMask.Lock,     () => _blackboard.CanAct,                          () => lockOn = true);
        private void OnBack()     => TryIntent(IntentMask.Back,     () => true,                                       () => back = true);

        private void TryIntent(IntentMask mask, Func<bool> condition, Action setIntent)
        {
            if (_disabledIntents.HasFlag(mask))
                return;

            input.ExecuteOrBuffer(setIntent, condition);
        }
    }
}