using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class hello : MonoBehaviour
{
    private void Start()
    {
        Run();
        LoadScene();
    }
    public void Run()
    {
        Debug.Log("Hello, HybridCLR");
        Debug.Log("dal");
        Debug.Log("dal2023-06-07");
        List<string> strings = new();
        strings.Add("泛型共享测试");
        Debug.Log(strings[0]);
        Debug.Log("泛型共享测试");

    }
    public void LoadScene()
    {

        var package = YooAssets.GetPackage("DefaultPackage");
        var sceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
        //开始跳转场景
        bool activateOnLoad = true;
        Debug.Log("开始加载场景");
        SceneOperationHandle scene = package.LoadSceneAsync("Start", sceneMode, activateOnLoad);

        Debug.Log($"Prefab name is {scene}");
    }
}
