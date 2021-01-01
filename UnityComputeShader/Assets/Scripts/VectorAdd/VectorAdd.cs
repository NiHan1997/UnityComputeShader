using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ʹ�ü�����ɫ������������ӵ�ʾ��.
/// </summary>
public class VectorAdd : MonoBehaviour
{
    /// <summary>
    /// ������ɫ��.
    /// </summary>
    [SerializeField]
    private ComputeShader m_ComputeShader;

    // �ֱ�洢����������ͱ�����.
    private ComputeBuffer resultBuffer;
    private ComputeBuffer prevBuffer;
    private ComputeBuffer nextBuffer;

    /// <summary>
    /// ��ӵ�����.
    /// </summary>
    [SerializeField]
    private int arraySize = 16;

    /// <summary>
    /// ��������.
    /// </summary>
    [SerializeField]
    private List<Vector3> prevList;

    /// <summary>
    /// ����������.
    /// </summary>
    [SerializeField]
    private List<Vector3> nextList;

    /// <summary>
    /// �������.
    /// </summary>
    [SerializeField]
    private List<Vector3> resultList;

    /// <summary>
    /// ���ڴ洢������ɫ���ľ��.
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
    /// ����������ݳ�ʼ��.
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
    /// ������ɫ����Ҫ�Ļ�������ʼ��.
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
    /// ��������ɫ����Ҫ�����ݷ���.
    /// </summary>
    private void InitBufferData()
    {
        kernelIndex = m_ComputeShader.FindKernel("CSMain");
        m_ComputeShader.SetBuffer(kernelIndex, "Result", resultBuffer);
        m_ComputeShader.SetBuffer(kernelIndex, "PrevVertices", prevBuffer);
        m_ComputeShader.SetBuffer(kernelIndex, "NextVertices", nextBuffer);
    }
}
