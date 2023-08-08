using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLimit : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 144;
    }
}
