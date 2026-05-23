using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePhysicsDetection : MonoBehaviour
{

    public Vector3 y_Offset;
    public Vector3 halfExtents;

    /*[HideInInspector] */
    public bool isGround;

    [SerializeField] private string playerLayerName = "Player"; // Player层级名称（可在Inspector改）
    private LayerMask _excludePlayerLayerMask; // 排除Player的层级掩码

    private void FixedUpdate()
    {
        GroundDetect();
    }

    private void Awake()
    {
        ExcludeGroundDetect();
    }
    private void ExcludeGroundDetect()
    {
        // 初始化：构建“除Player外所有层级”的掩码
        int playerLayerID = LayerMask.NameToLayer(playerLayerName);
        // 安全校验：防止层级名称错误导致的bug
        if (playerLayerID == -1)
        {
            Debug.LogError($"未找到名为 {playerLayerName} 的层级！请检查层级命名");
            _excludePlayerLayerMask = ~0; // 若出错则选中所有层级
        }
        else
        {
            // 核心：~(1 << playerLayerID) 取反Player层级，& 0xFFFFFF 限制在32位内
            _excludePlayerLayerMask = ~(1 << playerLayerID) & 0xFFFFFF;
        }
    }

    private void GroundDetect()
    {
        Collider[] colliders = new Collider[256];
        int hitCount = Physics.OverlapBoxNonAlloc(
            transform.position + y_Offset, // 检测盒中心
            halfExtents,                  // 检测盒半尺寸
            colliders,                    // 存储碰撞体的数组
            Quaternion.identity,          // 检测盒旋转（默认无旋转）
            _excludePlayerLayerMask       // 关键：排除Player的层级掩码
        );

        isGround = hitCount > 0;
    }




    private void OnDrawGizmosSelected()
    {

        Gizmos.DrawWireCube(transform.position + y_Offset, halfExtents * 2);
    }
}