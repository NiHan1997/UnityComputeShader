using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// �޾��ݵ�ʵʱ��Ⱦ�ڼ�����ɫ��ʵ��.
/// </summary>
public class Grass : MonoBehaviour
{
    [SerializeField]
    private ComputeShader grassComputeShader;

    [SerializeField]
    private Material grassMaterial;

    [SerializeField]
    private Mesh sourceMesh;

    private ComputeBuffer sourceVertexBuffer;
    private ComputeBuffer sourceTriangleBuffer;
    private ComputeBuffer drawTriangleBuffer;
    private ComputeBuffer argsBuffer;

    private int kernelIndex;
    private int dispatchSize;
    private Bounds localBounds;

    private const int SOURCE_VERT_STRIDE = sizeof(float) * 3;
    private const int SOURCE_TRI_STRIDE = sizeof(int);
    private const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 1) * 3);
    private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;

    private int[] argsBufferReset = new int[] { 0, 1, 0, 0 };

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct SourceVertex
    {
        public Vector3 position;
    }

    private void Start()
    {
        // ��ȡ������Ϣ.
        Vector3[] positions = sourceMesh.vertices;
        int[] tris = sourceMesh.triangles;

        SourceVertex[] vertices = new SourceVertex[positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new SourceVertex()
            {
                position = positions[i],
            };
        }
        int numSourceTriangles = tris.Length / 3;

        // ��ʼ�� Buffer.
        sourceVertexBuffer = new ComputeBuffer(vertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceVertexBuffer.SetData(vertices);
        
        sourceTriangleBuffer = new ComputeBuffer(tris.Length, SOURCE_TRI_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceTriangleBuffer.SetData(tris);

        drawTriangleBuffer = new ComputeBuffer(numSourceTriangles, DRAW_STRIDE, ComputeBufferType.Append);
        drawTriangleBuffer.SetCounterValue(0);

        argsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);

        // ��������.
        kernelIndex = grassComputeShader.FindKernel("CSMain");

        grassComputeShader.SetBuffer(kernelIndex, "SourceVertexBuffer", sourceVertexBuffer);
        grassComputeShader.SetBuffer(kernelIndex, "SourceTriangleBuffer", sourceTriangleBuffer);
        grassComputeShader.SetBuffer(kernelIndex, "DrawTriangles", drawTriangleBuffer);
        grassComputeShader.SetBuffer(kernelIndex, "IndirectArgsBuffer", argsBuffer);
        grassComputeShader.SetInt("_numSourceTriangles", numSourceTriangles);

        grassMaterial.SetBuffer("DrawTriangles", drawTriangleBuffer);

        // �����̷߳���.
        grassComputeShader.GetKernelThreadGroupSizes(kernelIndex, out uint threadGroupSize, out _, out _);
        dispatchSize = Mathf.CeilToInt((float)numSourceTriangles / threadGroupSize);

        localBounds = sourceMesh.bounds;
        localBounds.Expand(1);
    }

    private void Update()
    {
        drawTriangleBuffer.SetCounterValue(0);
        argsBuffer.SetData(argsBufferReset);

        Bounds bounds = TransformBounds(localBounds);
        grassComputeShader.SetMatrix("_objectToWorld", transform.localToWorldMatrix);

        grassComputeShader.Dispatch(kernelIndex, dispatchSize, 1, 1);

        Graphics.DrawProceduralIndirect(grassMaterial, bounds, MeshTopology.Triangles, argsBuffer, 0,
            null, null, ShadowCastingMode.Off, true, gameObject.layer);
    }

    private void OnDestroy()
    {
        if (sourceVertexBuffer != null)
            sourceVertexBuffer.Release();

        if (sourceTriangleBuffer != null)
            sourceTriangleBuffer.Release();

        if (drawTriangleBuffer != null)
            drawTriangleBuffer.Release();

        if (argsBuffer != null)
            argsBuffer.Release();
    }

    public Bounds TransformBounds(Bounds boundsOS)
    {
        Vector3 center = transform.TransformPoint(boundsOS.center);

        Vector3 extents = boundsOS.extents;
        Vector3 axisX = transform.TransformVector(extents.x, 0, 0);
        Vector3 axisY = transform.TransformVector(0, extents.y, 0);
        Vector3 axisZ = transform.TransformVector(0, 0, extents.z);

        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }
}