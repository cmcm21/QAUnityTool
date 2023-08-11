using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 30;
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
