# Unity 使用计算着色器示例
从基础案例开始，逐渐深入了解计算着色器，以及计算着色器相对于其他着色器的优势。

# 一：向量操作
在这里使用了和 DX 12 龙书类似的案例，在 DX 中使用计算着色器是比较麻烦的操作，但是得到的回报也是很好的。在这里使用了 2 x 2 = 4 个线程组，每个线程组中有 2 x 2 = 4 个线程，分别计算 2 x 2 x 2 x 2 = 16 个向量相加的结果，然后将结果从 GPU 中取回。
![image](/Images/VectorAdd/PrevList.png)
![image](/Images/VectorAdd/NextList.png)
![image](/Images/VectorAdd/ResultList.png)
