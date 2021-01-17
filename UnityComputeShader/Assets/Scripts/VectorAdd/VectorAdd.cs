using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 使用计算着色器计算向量相加的示例.
/// </summary>
public class VectorAdd : MonoBehaviour
{
    /// <summary>
    /// 计算着色器.
    /// </summary>
    [SerializeField]
    private ComputeShader m_ComputeShader;

    // 分别存储结果、加数和被加数.
    private ComputeBuffer resultBuffer;
    private ComputeBuffer prevBuffer;
    private ComputeBuffer nextBuffer;

    /// <summary>
    /// 相加的数量.
    /// </summary>
    [SerializeField]
    private int arraySize = 16;

    /// <summary>
    /// 加数集合.
    /// </summary>
    [SerializeField]
    private List<Vector3> prevList;

    /// <summary>
    /// 被加数集合.
    /// </summary>
    [SerializeField]
    private List<Vector3> nextList;

    /// <summary>
    /// 结果集合.
    /// </summary>
    [SerializeField]
    private List<Vector3> resultList;

    /// <summary>
    /// 用于存储计算着色器的句柄.
    /// </summary>
    private int kernelIndex;

    private void Start()
    {
        InitList();
        InitBuffers();
        InitBufferData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_ComputeShader.Dispatch(kernelIndex, 2, 2, 1);

            Vector3[] arrData = new Vector3[arraySize];
            resultBuffer.GetData(arrData);
            resultList.AddRange(arrData);
        }
    }

    private void OnDestroy()
    {
        if (resultBuffer != null)
            resultBuffer.Release();

        if (prevBuffer != null)
            prevBuffer.Release();

        if (nextBuffer != null)
            nextBuffer.Release();
    }

    /// <summary>
    /// 集合相关数据初始化.
    /// </summary>
    private void InitList()
    {
        resultList = new List<Vector3>(arraySize);
        prevList = new List<Vector3>(arraySize);
        nextList = new List<Vector3>(arraySize);

        for (int i = 0; i < arraySize; ++i)
        {
            prevList.Add(new Vector3((int)(Random.value * 10), (int)(Random.value * 20), (int)(Random.value * 30)));
            nextList.Add(new Vector3((int)(Random.value * 40), (int)(Random.value * 50), (int)(Random.value * 60)));
        }
    }

    /// <summary>
    /// 计算着色器需要的缓冲区初始化.
    /// </summary>
    private void InitBuffers()
    {
        prevBuffer = new ComputeBuffer(prevList.Count, 12);
        prevBuffer.SetData(prevList);

        nextBuffer = new ComputeBuffer(nextList.Count, 12);
        nextBuffer.SetData(nextList);

        resultBuffer = new ComputeBuffer(resultList.Capacity, 12);
    }

    /// <summary>
    /// 将计算着色器需要的数据发送.
    /// </summary>
    private void InitBufferData()
    {
        kernelIndex = m_ComputeShader.FindKernel("CSMain");
        m_ComputeShader.SetBuffer(kernelIndex, "Result", resultBuffer);
        m_ComputeShader.SetBuffer(kernelIndex, "PrevVertices", prevBuffer);
        m_ComputeShader.SetBuffer(kernelIndex, "NextVertices", nextBuffer);
    }
}
