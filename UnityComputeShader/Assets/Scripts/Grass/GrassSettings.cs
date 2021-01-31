using System;
using UnityEngine;

/// <summary>
/// 草地的相关设置.
/// </summary>
[Serializable]
public class GrassSettings
{
    [Tooltip("草地叶片的基础宽度.")]
    public float bladeWidth = 0.05f;

    [Tooltip("草地叶片宽度的随机范围.")]
    public float bladeWidthRandom = 0.02f;

    [Tooltip("草地叶片的基础高度.")]
    public float bladeHeight = 0.2f;

    [Tooltip("草地叶片高度的随机范围.")]
    public float bladeHeightRandom = 0.1f;
    
    [Tooltip("草地叶片最大分段数量.")]
    public int maxBladeSegments = 3;

    [Tooltip("草的弯曲程度.")]
    public float bendRotationRandom;

    [Tooltip("草地的朝向, 越小草越直.")]
    public float bladeForward;

    [Tooltip("风向的随机噪声纹理.")]
    public Texture2D windTexture;

    [Tooltip("风向的随机噪声纹理采样参数.")]
    public Vector4 windTextureST;

    [Tooltip("调整风力.")]
    public float windStrength;

    [Tooltip("调整风速.")]
    public Vector2 windFrequency;
}
