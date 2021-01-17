using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 计算着色器实现边缘检测效果.
/// </summary>
public class EdgeDetectionCompute : MonoBehaviour
{
    /// <summary>
    /// 计算着色器结果应用的材质.
    /// </summary>
    private Material m_Material;

    [SerializeField]
    private ComputeShader m_ComputeShader;

    /// <summary>
    /// 计算着色器输入.
    /// </summary>
    [SerializeField]
    private Texture2D inputTexture;

    /// <summary>
    /// 计算着色器输出.
    /// </summary>
    private RenderTexture outputTexture;

    /// <summary>
    /// 边缘颜色强度.
    /// </summary>
    [SerializeField, Range(0, 1)]
    private float onlyEdgeColor = 0.5f;

    /// <summary>
    /// 边缘颜色深度控制.
    /// </summary>
    [SerializeField, Range(0, 1)]
    private float edgeColorMinus = 0.5f;

    private int kernel;

    private void Start()
    {
        m_Material = gameObject.GetComponent<MeshRenderer>().material;

        kernel = m_ComputeShader.FindKernel("SobelCS");
        outputTexture = new RenderTexture(inputTexture.width, inputTexture.height, 1);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        m_ComputeShader.SetTexture(kernel, "InputTexture", inputTexture);
        m_ComputeShader.SetTexture(kernel, "OutputTexture", outputTexture);
    }

    private void Update()
    {
        m_ComputeShader.SetFloat("onlyEdge", onlyEdgeColor);
        m_ComputeShader.SetFloat("edgeColorMinus", edgeColorMinus);

        m_ComputeShader.Dispatch(kernel, Mathf.CeilToInt(inputTexture.width / 256.0f), inputTexture.height, 1);
        m_Material.SetTexture("_MainTex", outputTexture);
    }

    private void OnDestroy()
    {
        if (outputTexture != null)
            outputTexture.Release();
    }
}
