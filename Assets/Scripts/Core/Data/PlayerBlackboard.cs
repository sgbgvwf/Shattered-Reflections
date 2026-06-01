using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// 玩家黑板：为意图层、行为树、动画等提供统一的状态查询。
    /// 所有属性由外部系统（CharacterController, StateMachine 等）负责写入。
    /// </summary>
    public class PlayerBlackboard : MonoBehaviour
    {
        [Header("Movement")]
        public bool IsGrounded;
        public bool IsMoving;
        public bool IsRunning;
        public bool IsCrouching;
        public bool IsInAir;
        public float MoveSpeed;
        public Vector3 Velocity;
        public Vector3 InputDirection;   // 玩家输入移动方向（世界空间）

        [Header("Actions")]
        public bool CanAct = true;                // 全局动作许可
        public bool CanInteract = true;           // 交互许可
        public bool CanAttack = true;             // 攻击许可（可单独禁用）
        public bool CanDodge = true;              // 闪避许可
        public bool CanJump = true;               // 跳跃许可
        public bool CanUseSkill = true;           // 技能许可
        public bool CanUseFinisher = true;        // 大招许可

        [Header("Combat")]
        public bool IsStunned;                    // 眩晕
        public bool IsStrokenToFly;               // 击飞
        public bool IsRigidity;                   // 僵直
        public bool IsInvincible;                 // 无敌帧（闪避/某些技能）
        public bool HasTarget;                    // 是否锁定目标
        public float TargetDistance;              // 到锁定目标的距离
        public int LightAttackCombo;              // 轻击段数
        public bool IsInExecutionRange;           // 是否在处决/终结技范围内

        [Header("Status")]
        public float Health;                      // 当前生命
        public float HealthPercent;               // 当前生命百分比 (0~1)
        public float Stamina;                     // 当前耐力
        public float StaminaPercent;              // 当前耐力百分比 (0~1)
        public bool HasEnoughStaminaForDodge;     // 是否有足够耐力闪避
        public bool IsDead;                       // 死亡

        [Header("State Machine")]
        public int CurrentStateId;                // 当前动画状态机状态ID（可选）
        public string CurrentStateName;           // 当前状态名称
        public bool IsAttacking;                  // 是否正在攻击动作中
        public bool IsDodging;                    // 是否正在闪避动作中
        public bool IsJumping;                    // 是否正在跳跃动作中
        public bool IsUsingSkill;                 // 是否正在技能动作中
        public bool IsInFinisher;                 // 是否正在终结技演出中
        public float AttackComboWindow;           // 当前攻击连招窗口剩余时间（0表示不在窗口期）


        [Header("Interaction")]
        public bool IsNearInteractable;           // 附近有可交互物
        public string NearestInteractableTag;     // 最近可交互物的标签（便于条件判断）
        public bool IsInDialogue;                 // 对话中
        public bool IsInMenu;                     // 菜单开启中（通常由外部UI系统写入）

        [Header("Buff / Debuff")]
        public bool IsBuffed;                     // 增益状态
        public bool IsWeakened;                   // 虚弱状态
        public float DamageMultiplier = 1f;       // 伤害倍率
        public float SpeedMultiplier = 1f;        // 速度倍率
    }
}