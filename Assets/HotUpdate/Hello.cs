using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hello
{
    public void Run()
    {
        Debug.Log("Hello, HybridCLR");
        Debug.Log("Hello, HybridCLR");
        Debug.Log("Hello, HybridCLR");
        Debug.Log("Hello, HybridCLR");
        Debug.Log("dal");
        Debug.Log("dal2023-06-07");
        test();
    }
    void test()
    {
        ints.Add(1);
        strings.Add("泛型共享测试");
    }
    List<Vector3> vector3s = new();
    List<string> strings = new();
    List<int> ints = new();
}
