using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �޾��ݵ���Ⱦǰ��׼��, ����һƬ�򵥵Ĳݵ�.
/// </summary>
public class GrassPrepare : MonoBehaviour
{
    [SerializeField]
    private ComputeShader m_ComputeShader;

    [SerializeField]
    private Material m_Material;

    /// <summary>
    /// һ�òݵ� Mesh, 7 ������, 15 ������.
    /// </summary>
    private Mesh bladeMesh;
    private ComputeBuffer positionBuffer;

    private int kernelIndex;

    private void Start()
    {
        bladeMesh = new Mesh();
        bladeMesh.vertices = new Vector3[7];
        bladeMesh.SetIndices(new[]
        {
            0, 1, 2,
            2, 1, 3,
            3, 1, 4,
            3, 4, 5,
            4, 6, 5
        },
        MeshTopology.Triangles, 0);
        bladeMesh.UploadMeshData(true);

        kernelIndex = m_ComputeShader.FindKernel("CSMain");
        positionBuffer = new ComputeBuffer(7, 12);
    }

    private void Update()
    {
        m_ComputeShader.SetBuffer(kernelIndex, "PositionBuffer", positionBuffer);
        m_ComputeShader.Dispatch(kernelIndex, 1, 1, 1);

        m_Material.SetBuffer("_PositionBuffer", positionBuffer);

        Graphics.DrawMeshInstancedProcedural(bladeMesh, 0, m_Material,
           new Bounds(transform.position, transform.lossyScale), 100);
    }

    private void OnDisable()
    {
        if (positionBuffer != null)
            positionBuffer.Release();
    }
}
