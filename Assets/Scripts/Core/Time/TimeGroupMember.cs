using UnityEngine;
using Core.Time;

/// <summary>
/// 时间组成员：纯粹的时间数据提供者。
/// 只根据所属时间组的系数计算并提供 DeltaTime、TimeScale、VirtualTime，
/// 不包含任何移动、旋转、动画等业务逻辑。
/// </summary>
public class TimeGroupMember : MonoBehaviour
{
    [Tooltip("所属时间组")]
    public TimeGroupType timeGroup = TimeGroupType.Enemy;

    /// <summary>
    /// 当前帧该组的时间缩放量
    /// </summary>
    public float DeltaTime { get; private set; }

    /// <summary>
    /// 所在组当前的时间缩放系数
    /// </summary>
    public float TimeScale { get; private set; }

    /// <summary>
    /// 虚拟时间，按该组的实际时间流速累计。
    /// 用于传给 Timer 的 customTimeProvider，实现与该组同步的时间流逝。
    /// </summary>
    public float VirtualTime { get; private set; }

    private void Update()
    {
        TimeScale = CustomTimeManager.Instance.GetTimeScale(timeGroup);
        DeltaTime = Time.deltaTime * TimeScale;
        VirtualTime += DeltaTime;
    }

    /// <summary>
    /// 重置虚拟时间
    /// </summary>
    public void ResetVirtualTime()
    {
        VirtualTime = 0f;
    }
}