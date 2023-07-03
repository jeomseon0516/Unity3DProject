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
    private IEnumerator setColor()
    {
        if (ReferenceEquals(_renderer, null) || !_renderer) yield break;

        Material material = new Material(Shader.Find("Legacy Shaders/Transparent/Specular"));
        Color color = material.color;

        while (color.a > 0.5f)
        {
            color.a -= Time.deltaTime;
            _renderer.material.color = color;

            yield return null;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            StartCoroutine(setColor());
    }
}
