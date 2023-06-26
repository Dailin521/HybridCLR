using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDll : MonoBehaviour
{
    void Start()
    {
        LoadMetadataForAOTAssemblies();
        // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
       string fileUrl = "https://cdn-1257380158.cos.ap-nanjing.myqcloud.com/WebGLTest/HotUpdate.dll.bytes";

        StartCoroutine(ReadAllBytes(fileUrl, (bytes) =>
        {
            if (bytes != null)
            {
                Debug.Log("Successfully read " + bytes.Length + " bytes.");
                Assembly hotUpdateAss = Assembly.Load(bytes);
                Type type = hotUpdateAss.GetType("Hello");
                GameObject go = new GameObject("TestHot");
                go.AddComponent(type);
            }
            else
            {
                // Handle the failure to read the bytes
                Debug.LogError("Failed to read bytes from " + fileUrl);
            }
        }));
#else
        // Editor下无需加载，直接查找获得HotUpdate程序集
        //Assembly hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");

        //string url = $"{Application.streamingAssetsPath}/HotUpdate.dll.bytes";
        //Debug.LogWarning(url);
        //Debug.LogWarning("");
        //string fileUrl = $"{Application.streamingAssetsPath}/HotUpdate.dll.bytes";
        string fileUrl = "https://cdn-1257380158.cos.ap-nanjing.myqcloud.com/WebGLTest/HotUpdate.dll.bytes";
        StartCoroutine(ReadAllBytes(fileUrl, (bytes) =>
        {
            if (bytes != null)
            {
                // Do something with the bytes
                Debug.Log("Successfully read " + bytes.Length + " bytes.");
                Assembly hotUpdateAss = Assembly.Load(bytes);
                Type type = hotUpdateAss.GetType("Hello");
                GameObject go = new GameObject("TestHot");
                go.AddComponent(type);
            }
            else
            {
                // Handle the failure to read the bytes
                Debug.LogError("Failed to read bytes from " + fileUrl);
            }
        }));
#endif
    }
    private void LoadMetadataForAOTAssemblies()
    {
        List<string> aotDllList = new List<string>
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll", // 如果使用了Linq，需要这个
            // "Newtonsoft.Json.dll", 
            // "protobuf-net.dll",
        };

        foreach (var aotDllName in aotDllList)
        {
            // string url =https://cdn-1257380158.cos.ap-nanjing.myqcloud.com/WebGLTest/AOTDLL/mscorlib.dll.bytes
            string url = "https://cdn-1257380158.cos.ap-nanjing.myqcloud.com/WebGLTest/AOTDLL/" + $"{aotDllName}.bytes";
            StartCoroutine(ReadAllBytes(url, (bytes) =>
            {
                byte[] dllBytes = bytes;
                int err = (int)HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
            }));
        }
    }
    IEnumerator ReadAllBytes(string url, Action<byte[]> onComplete)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                byte[] bytes = www.downloadHandler.data;
                onComplete?.Invoke(bytes);
            }
            else
            {
                Debug.LogError("Failed to read bytes: " + www.error);
                onComplete?.Invoke(null);
            }
        }
    }


}
