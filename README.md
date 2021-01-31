# Unity 使用计算着色器示例
从基础案例开始，逐渐深入了解计算着色器，以及计算着色器相对于其他着色器的优势。

# 一：向量操作
在这里使用了和 DX 12 龙书类似的案例，在 DX 中使用计算着色器是比较麻烦的操作，但是得到的回报也是很好的。在这里使用了 2 x 2 = 4 个线程组，每个线程组中有 2 x 2 = 4 个线程，分别计算 2 x 2 x 2 x 2 = 16 个向量相加的结果，然后将结果从 GPU 中取回。
![image](/Images/VectorAdd/PrevList.png)
![image](/Images/VectorAdd/NextList.png)
![image](/Images/VectorAdd/ResultList.png)

# 二：边缘检测
边缘检测是一种常见的操作，使用计算着色器实现边缘检测也是十分高效的操作。具体实现细节可以参考我的 DX 12 习题解答[13 章第 6 题](https://github.com/NiHan1997/DirectX12Exercise/tree/master/Chapter_13/Exercise_6)，其中在 Unity 中和 DX 中并没有太多差异。
![image](/Images/EdgeDetection/Origin.png)
![image](/Images/EdgeDetection/Edge01.png)
![image](/Images/EdgeDetection/Edge02.png)

# 三：绘制简单的几何体
计算着色器为我们提供的功能远不止图像处理那么简单，事实上，计算着色器的功能可以高效地替代几何着色器和曲面细分着色器，只是在处理的过程中会比较麻烦，但是得到的性能提示绝对是值得的。
在这里使用计算着色器简单绘制了 100 颗草的网格，这是为后面无尽草地渲染做准备，也是计算着色器操作顶点的入门示例。
![image](/Images/GrassPrepare/Grass01.png)
![image](/Images/GrassPrepare/Grass02.png)

# 四：无尽草地实时渲染
在之前我们使用几何着色器实现过无尽草地渲染，但是像 IOS 这样的设备，没有提供几何着色器的支持，我们就只能使用其他方法，所幸计算着色器能够更好地满足我们在效果和性能上的需求。实现的算法参考了之前的几何着色器版本。  
同时开启阴影投射和阴影接收，在细分 25 次之后，几何着色器性能显著下降，但计算着色器性能相对稳定，综合看来计算着色器都优于几何着色器的表现。  
几何着色器性能：
![image](/Images/Grass/Grass01.png)
计算着色器性能：
![image](/Images/Grass/Grass02.png)
最后附一张比较好看的图：
![image](/Images/Grass/Grass03.png)
