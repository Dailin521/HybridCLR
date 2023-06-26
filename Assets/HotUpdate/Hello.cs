using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Hello : MonoBehaviour
{
    private void Start()
    {
        Run();
    }
    public void Run()
    {
        Debug.Log("Hello, HybridCLR");
        Debug.Log("Hello, HybridCLR");
        Debug.Log("Hello, HybridCLR");
        Debug.Log("Hello, HybridCLR");
        Debug.Log("dal");
        Debug.Log("dal2023-06-07");
        List<string> strings = new();
        strings.Add("泛型共享测试");
        Debug.Log(strings[0]);
        Debug.Log("泛型共享测试");
    }


}
