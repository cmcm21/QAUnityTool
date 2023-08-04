using System.Collections;
using TagwizzQASniffer.Editor;
using TagwizzQASniffer.Core;
using System.Collections.Generic;
using UnityEngine;

public class Mymonobehaivor : MonoBehaviour
{
    private SnifferCore _snifferCore;
    void Start()
    {
        _snifferCore = new SnifferCore();
        _snifferCore.Init();
    }
}
