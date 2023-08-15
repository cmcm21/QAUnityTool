using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputFieldsManager : MonoBehaviour
{
    private TMP_InputField _inputField;
    private void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
        Keyboard.current.onTextInput += character =>
        {
            _inputField.ForceLabelUpdate();
        };
    }
}
