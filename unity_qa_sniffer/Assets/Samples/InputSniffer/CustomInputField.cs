using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomInputField : InputField
{
    private void Start()
    {
        Keyboard.current.onTextInput += CurrentOnTextInput;        
    }

    private void CurrentOnTextInput(char character)
    {
        text += character;
    }

    private void Update()
    {
        
    }
}
