using System;
using System.Collections;
using System.IO;
using System.Management.Instrumentation;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDll : MonoBehaviour
{
    void Start()
    {
        // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
       string fileUrl = "https://cdn-1257380158.cos.ap-nanjing.myqcloud.com/WebGLTest/HotUpdate.dll.bytes";

        StartCoroutine(ReadAllBytes(fileUrl, (bytes) =>
        {
            if (bytes != null)
            {
                // Do something with the bytes
                Debug.Log("Successfully read " + bytes.Length + " bytes.");
                Assembly hotUpdateAss = Assembly.Load(bytes);
                Type type = hotUpdateAss.GetType("Hello");
                MethodInfo method = type.GetMethod("Run");
                object instance = Activator.CreateInstance(type);  // 创建一个类实例
                Debug.Log("instance: " + instance);
                object result = method.Invoke(instance, null);  // 调用方法
                Debug.Log("result: " + result);
                // 检查并处理返回结果
                if (result != null)
                {
                    // 如果方法有返回值，可以在这里对结果进行处理
                    Debug.Log("方法返回结果: " + result.ToString());
                }
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
                MethodInfo method = type.GetMethod("Run");
                object instance = Activator.CreateInstance(type);  // 创建一个类实例
                Debug.Log("instance: " + instance);
                object result = method.Invoke(instance, null);  // 调用方法
                Debug.Log("result: " + result);
                // 检查并处理返回结果
                if (result != null)
                {
                    // 如果方法有返回值，可以在这里对结果进行处理
                    Debug.Log("方法返回结果: " + result.ToString());
                }
            }
            else
            {
                // Handle the failure to read the bytes
                Debug.LogError("Failed to read bytes from " + fileUrl);
            }
        }));
#endif
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
