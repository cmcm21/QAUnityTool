using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeManager : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 30;
    [SerializeField] private float moveSpeed = 20;
    private bool _canRotate = false;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    
    private void Update()
    {
       if(_canRotate) 
           transform.Rotate(Vector3.up,rotateSpeed*Time.deltaTime);
       
       if(Keyboard.current.downArrowKey.isPressed)
           transform.Translate(Vector3.down * Time.deltaTime * moveSpeed);
       if(Keyboard.current.rightArrowKey.isPressed)
           transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
       if(Keyboard.current.leftArrowKey.isPressed)
           transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
       if(Keyboard.current.upArrowKey.isPressed)
           transform.Translate(Vector3.up * Time.deltaTime * moveSpeed);
    }

    public void Rotate()
    {
        _canRotate = !_canRotate;
    }

    public void ChangeColor()
    {
        var materialColor = _meshRenderer.material.color;
        _meshRenderer.material.color = materialColor == Color.blue ? Color.white : Color.blue;
    }
}
