using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace BHR
{
    public class FadeWithDistanceUI : MonoBehaviour
    {
        private float _fadeTransitionDistance = 15f;

        [SerializeField, Tooltip("If empty, MainCamera")] private Transform _target;
        [Min(15f), Tooltip("At this distance, the object became invisible")]
        public float DistanceFade;
        [SerializeField, Tooltip("If true, the color fades away as the target approaching rather than the contrary")] private bool _invert = false;
        private Color _baseColor;

        private void Awake()
        {
            if (_target == null) _target = GameObject.FindWithTag("MainCamera").transform;

            if (TryGetComponent<Image>(out Image image))
                _baseColor = image.color;
            else if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
                _baseColor = text.color;
            else if (TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
                _baseColor = spriteRenderer.color;
            else
                Debug.LogWarning($"No color component detected on {gameObject.name}");
        }

        private void LateUpdate()
        {
            Color fadeColor = new Color(_baseColor.r,_baseColor.g,_baseColor.b, 0f);
            float distance = Vector3.Distance(transform.position, _target.position);
            float percents = Mathf.Clamp01((distance - DistanceFade + _fadeTransitionDistance) / _fadeTransitionDistance);
            if(_invert) percents = 1 - percents;
            Color targetColor = Color.Lerp(_baseColor, fadeColor, percents);

            if (TryGetComponent<Image>(out Image image))
                image.color = targetColor;
            else if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
                text.color = targetColor;
            else if (TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
                spriteRenderer.color = targetColor;
        }
    }
}
