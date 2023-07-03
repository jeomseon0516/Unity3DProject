using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererManager : MonoBehaviour
{
    [SerializeField]
    private Renderer _renderer;

    private void Awake()
    {
        TryGetComponent(out _renderer);
    }


    void Update()
    {
    }
}
