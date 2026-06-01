using UnityEngine;

namespace Combat.Visual
{
    /// <summary>
    /// 锁定指示器：场景中只放一个，锁定后跟随目标位置，面朝摄像机，屏幕大小恒定。
    /// 挂在一个带 SpriteRenderer 的独立 GameObject 上。
    /// </summary>
    public class LockIndicator : MonoBehaviour
    {
        [Header("屏幕大小")]
        [Tooltip("距离 × screenScale = 世界缩放。")]
        [SerializeField] [Range(0.005f, 5f)] private float _screenScale;

        private Camera _camera;
        private Transform _target;
        private Vector3 _initLocalScale;

        private void Awake()
        {
            _camera = Camera.main;
            _initLocalScale = transform.localScale;
            gameObject.SetActive(false);
        }

        /// <summary> 设置跟随目标，传 null 则隐藏 </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            if (target != null)
                gameObject.SetActive(true);
            else
                gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_camera == null || _target == null)
            {
                gameObject.SetActive(false);
                return;
            }
            transform.position = _target.position;
            transform.rotation = _camera.transform.rotation;

            float dist = Vector3.Distance(_target.position, _camera.transform.position);
            transform.localScale = _initLocalScale * dist * _screenScale;
        }
    }
}
