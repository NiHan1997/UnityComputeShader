using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ɫ��ʵ�ֱ�Ե���Ч��.
/// </summary>
public class EdgeDetectionCompute : MonoBehaviour
{
    /// <summary>
    /// ������ɫ�����Ӧ�õĲ���.
    /// </summary>
    private Material m_Material;

    [SerializeField]
    private ComputeShader m_ComputeShader;

    /// <summary>
    /// ������ɫ������.
    /// </summary>
    [SerializeField]
    private Texture2D inputTexture;

    /// <summary>
    /// ������ɫ�����.
    /// </summary>
    private RenderTexture outputTexture;

    /// <summary>
    /// ��Ե��ɫǿ��.
    /// </summary>
    [SerializeField, Range(0, 1)]
    private float onlyEdgeColor = 0.5f;

    /// <summary>
    /// ��Ե��ɫ��ȿ���.
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
