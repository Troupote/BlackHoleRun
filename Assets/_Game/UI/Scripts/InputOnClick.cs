using BHR;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputOnClick : MonoBehaviour
{
    [SerializeField, Required] private Button _button;
    [SerializeField, Required] private string _input;
    [SerializeField] private bool _inputHasVector2Value;
    [SerializeField, ShowIf(nameof(_inputHasVector2Value))] private Vector2 _targetMinVector2;
    [SerializeField, ShowIf(nameof(_inputHasVector2Value))] private Vector2 _targetMaxVector2;

    private void OnEnable()
    {
        PlayersInputManager.Instance.OnUIInput.AddListener(HandleInput);
    }

    private void OnDisable()
    {
        PlayersInputManager.Instance.OnUIInput.RemoveListener(HandleInput);
    }

    private void HandleInput(InputAction.CallbackContext ctx)
    {
        if(ctx.performed && ctx.action.name == _input && (!_inputHasVector2Value || UtilitiesFunctions.IsVectorBetween(ctx.ReadValue<Vector2>(), _targetMinVector2, _targetMaxVector2 )))
            _button.onClick.Invoke();
    }
}
