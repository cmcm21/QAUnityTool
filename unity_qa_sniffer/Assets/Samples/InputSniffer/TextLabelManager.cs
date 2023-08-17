using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TextLabelManager : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Keyboard.current.onTextInput += OnKeyPressed;
    }

    private void OnKeyPressed(char character)
    {
        _text.text += character;
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= OnKeyPressed;
    }
}
