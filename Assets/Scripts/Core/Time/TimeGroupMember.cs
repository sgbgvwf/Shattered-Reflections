using UnityEngine;
using Core.Time;

/// <summary>
/// 时间组成员：纯粹的时间数据提供者。
/// 每帧提供该组的 DeltaTime、TimeScale，以及实例累计的 VirtualTime。
/// 注意：每个实例的 VirtualTime 独立累加，若需要全局一致的组时间，请使用 CustomTimeManager.GetGroupVirtualTime。
/// </summary>
public class TimeGroupMember : MonoBehaviour
{
    [Tooltip("所属时间组")]
    public TimeGroupType timeGroup = TimeGroupType.Enemy;

    public float DeltaTime { get; private set; }
    public float TimeScale { get; private set; }
    public float VirtualTime { get; private set; }

    private void Update()
    {
        TimeScale = CustomTimeManager.Instance.GetTimeScale(timeGroup);
        DeltaTime = Time.deltaTime * TimeScale;
        VirtualTime += DeltaTime;
    }
}