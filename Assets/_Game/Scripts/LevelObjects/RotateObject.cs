using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace BHR
{
    public class RotateObject : MonoBehaviour
    {
        [SerializeField, Tooltip("If empty, this object itslef")] private Transform _object;
        [SerializeField, ShowIf(nameof(OtherObjectSelected)), Tooltip("Rotate around selected object rather than rotate around itself")] private bool _rotateAround;
        [SerializeField] private float _speed;
        [SerializeField, EnumToggleButtons] private Axes _axes;
        [SerializeField, Tooltip("Will set a random force in the authorized axes"), HideIf(nameof(_rotateAround))] 
        private bool _randomStartDirection = false;

        [SerializeField, ShowIf(nameof(ShowXCoef)), Tooltip("Specific coef applying to the speed for X axe")] private float _xCoef = 1f;
        [SerializeField, ShowIf(nameof(ShowYCoef)), Tooltip("Specific coef applying to the speed for Y axe")] private float _yCoef = 1f;
        [SerializeField, ShowIf(nameof(ShowZCoef)), Tooltip("Specific coef applying to the speed for Z axe")] private float _zCoef = 1f;

        [SerializeField, ShowIf(nameof(ShowXFork)), Tooltip("Random X force will be between the fork (inclusive)")] private Vector2 _xFork = Vector2.up;
        [SerializeField, ShowIf(nameof(ShowYFork)), Tooltip("Random Y force will be between the fork (inclusive)")] private Vector2 _yFork = Vector2.up;
        [SerializeField, ShowIf(nameof(ShowZFork)), Tooltip("Random Z force will be between the fork (inclusive)")] private Vector2 _zFork = Vector2.up;

        [SerializeField, ReadOnly]
        private Vector3 _direction;

        #region Odin stuff
        private bool ShowXCoef => AxeEnabled(Axes.X) && !_randomStartDirection && !_rotateAround;
        private bool ShowYCoef => AxeEnabled(Axes.Y) && !_randomStartDirection && !_rotateAround;
        private bool ShowZCoef => AxeEnabled(Axes.Z) && !_randomStartDirection && !_rotateAround;
        private bool ShowXFork => AxeEnabled(Axes.X) && _randomStartDirection && !_rotateAround;
        private bool ShowYFork => AxeEnabled(Axes.Y) && _randomStartDirection && !_rotateAround;
        private bool ShowZFork => AxeEnabled(Axes.Z) && _randomStartDirection && !_rotateAround;
        #endregion

        private void Awake()
        {
            if(_object == null)
                _object = transform;

            float x=0, y=0, z=0;
            if (AxeEnabled(Axes.X)) x = _randomStartDirection ? Random.Range(_xFork.x, _xFork.y) : 1 * _xCoef;
            if (AxeEnabled(Axes.Y)) y = _randomStartDirection ? Random.Range(_yFork.x, _yFork.y) : 1 * _yCoef;
            if (AxeEnabled(Axes.Z)) z = _randomStartDirection ? Random.Range(_zFork.x, _zFork.y) : 1 * _zCoef;
            _direction = new Vector3(x, y, z);
        }

        private bool AxeEnabled(Axes axe) => (_axes & axe) != 0;
        private bool OtherObjectSelected()
        {
            if (_object != null && _object != transform)
                return true;
            else
            {
                _rotateAround = false;
                return false;
            }
        }

        private void FixedUpdate()
        {
            Rotate();
        }

        private void Rotate()
        {
            Vector3 vel = _direction * _speed * Time.deltaTime * GameManager.Instance.GameTimeScale;
            if (_rotateAround)
            {
                if (OtherObjectSelected())
                    transform.RotateAround(_object.position, _direction, _speed * Time.deltaTime * GameManager.Instance.GameTimeScale);
                else
                    Debug.LogError($"{gameObject.name}'s rotating around a null object or itself");
            }
            else
                _object.Rotate(vel);
        }
    }
}
