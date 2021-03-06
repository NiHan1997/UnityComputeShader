using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 无尽草地实时渲染在计算着色器实现.
/// </summary>
public class Grass : MonoBehaviour
{
    [SerializeField]
    private ComputeShader grassComputeShader;

    [SerializeField]
    private Material grassMaterial;

    [SerializeField]
    private Mesh sourceMesh;

    [SerializeField]
    private GrassSettings grassSettings;                    // 草地相关设置项.

    private ComputeBuffer sourceVertexBuffer;               // 顶点输入源.
    private ComputeBuffer sourceTriangleBuffer;             // 三角形输入源, 在这里只需要索引第几个三角形即可.
    private ComputeBuffer drawTriangleBuffer;               // 最终需要绘制的三角形.
    private ComputeBuffer argsBuffer;                       // 计算着色器需要的内部参数.

    private int kernelIndex;                                // 计算着色器索引.
    private int dispatchSize;                               // 分配的线程组数量.
    private Bounds localBounds;                             // 包围盒.

    private const int SOURCE_VERT_STRIDE = sizeof(float) * 3;               // 每个输入顶点的大小.
    private const int SOURCE_TRI_STRIDE = sizeof(int);                      // 每个输入三角形的大小.
    private const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 1) * 3);      // 绘制每个三角形需要的大小数据.
    private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;               // 计算着色器内部参数数据大小.
    
    /// <summary>
    /// 每一帧都需要重置这个数据, 数据的计算的由计算着色器内部控制的, 我们只考虑其中的初始状态即可.
    /// 0: 每一个实例需要的顶点数量, 在这里我们只需要 1 个实例;
    /// 1: 实例数量;
    /// 2: 开始绘制的顶点索引偏移;
    /// 3: 开始绘制的实例索引偏移.
    /// </summary>
    private int[] argsBufferReset = new int[] { 0, 1, 0, 0 };

    /// <summary>
    /// 这个结构体的数据会发送到 GPU 计算着色器, 加上这个标签的布局类型可确保数据按顺序进行布局.
    /// 也就是说, 加上这个标签后我们可以不用考虑 Vector4 的对齐问题.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct SourceVertex
    {
        public Vector3 position;
    }

    private void Start()
    {
        // 创建一个较大的 Mesh, 用于存放更多的草地.
        sourceMesh = gameObject.GetComponent<CreatePlaneMesh>().CreatePlane();
        sourceMesh.name = "Plane Sub";

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

        // 初始化 Buffer.
        sourceVertexBuffer = new ComputeBuffer(vertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceVertexBuffer.SetData(vertices);
        
        sourceTriangleBuffer = new ComputeBuffer(tris.Length, SOURCE_TRI_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceTriangleBuffer.SetData(tris);

        drawTriangleBuffer = new ComputeBuffer(numSourceTriangles * (2 * grassSettings.maxBladeSegments - 1), DRAW_STRIDE, ComputeBufferType.Append);
        drawTriangleBuffer.SetCounterValue(0);

        argsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);

        // 设置数据.
        kernelIndex = grassComputeShader.FindKernel("CSMain");

        // 设置 Buffer.
        grassComputeShader.SetBuffer(kernelIndex, "_SourceVertexBuffer", sourceVertexBuffer);
        grassComputeShader.SetBuffer(kernelIndex, "_SourceTriangleBuffer", sourceTriangleBuffer);
        grassComputeShader.SetBuffer(kernelIndex, "_DrawTriangles", drawTriangleBuffer);
        grassComputeShader.SetBuffer(kernelIndex, "_IndirectArgsBuffer", argsBuffer);

        // 设置数量.
        grassComputeShader.SetInt("_NumSourceTriangles", numSourceTriangles);

        // 设置草地数据.
        grassComputeShader.SetFloat("_BladeWidth", grassSettings.bladeWidth);
        grassComputeShader.SetFloat("_BladeWidthRandom", grassSettings.bladeWidthRandom);
        grassComputeShader.SetFloat("_BladeHeight", grassSettings.bladeHeight);
        grassComputeShader.SetFloat("_BladeHeightRandom", grassSettings.bladeHeightRandom);
        grassComputeShader.SetFloat("_BendRotationRandom", grassSettings.bendRotationRandom);
        grassComputeShader.SetFloat("_BladeForward", grassSettings.bladeForward);
        grassComputeShader.SetTexture(kernelIndex, "_WindTexture", grassSettings.windTexture);
        grassComputeShader.SetVector("_WindTextureST", grassSettings.windTextureST);
        grassComputeShader.SetFloat("_WindStrength", grassSettings.windStrength);
        grassComputeShader.SetVector("_WindFrequency", grassSettings.windFrequency);

        grassMaterial.SetBuffer("_DrawTriangles", drawTriangleBuffer);

        // 计算线程分配.
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
        grassComputeShader.SetMatrix("_ObjectToWorld", transform.localToWorldMatrix);

        grassComputeShader.Dispatch(kernelIndex, dispatchSize, 1, 1);

        Graphics.DrawProceduralIndirect(grassMaterial, bounds, MeshTopology.Triangles, argsBuffer, 0,
            null, null, grassSettings.shadowCastingMode, grassSettings.receiveShadows, gameObject.layer);
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

    private Bounds TransformBounds(Bounds boundsOS)
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
