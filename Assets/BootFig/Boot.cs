using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace JITWork
{
    public class Boot : MonoBehaviour
    {
        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
        public string version = "V1.5";
        void Awake()
        {
            Debug.Log($"资源系统运行模式：{PlayMode}");
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            DontDestroyOnLoad(this);
        }
        void Start()
        {
            StartCoroutine(LoadTest());
        }
        // 内置文件查询服务类

        IEnumerator LoadTest()
        {
            yield return null;
            // 初始化资源系统
            YooAssets.Initialize();
            // 创建默认的资源包
            var package = YooAssets.CreatePackage("DefaultPackage");
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);
            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                var initParameters = new EditorSimulateModeParameters();
                initParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
                yield return package.InitializeAsync(initParameters);
            }
            else if (PlayMode == EPlayMode.OfflinePlayMode)
            {
                var initParameters = new OfflinePlayModeParameters();
                yield return package.InitializeAsync(initParameters);
            }
            else if (PlayMode == EPlayMode.HostPlayMode)
            {
                var initParameters = new HostPlayModeParameters();
                initParameters.QueryServices = new QueryStreamingAssetsFileServices();
                initParameters.DecryptionServices = new GameDecryptionServices();
                //initParameters.DefaultHostServer = "https://cdn-1257380158.cos.ap-nanjing.myqcloud.com/CDN/PC/" + version;
                initParameters.DefaultHostServer = "https://cdn-1257380158.cos.ap-nanjing.myqcloud.com/CDN/PC/" + version;
                initParameters.FallbackHostServer = "https://cdn-1257380158.cos.ap-nanjing.myqcloud.com/CDN/PC/V1.3/";
                var initOperation = package.InitializeAsync(initParameters);
                yield return initOperation;
                if (initOperation.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"资源包初始化失败：{initOperation.Error}");
                    yield break;
                }
                Debug.Log("资源包初始化成功！");
            }
            //获取资源版本
            var operation = package.UpdatePackageVersionAsync();
            yield return operation;
            if (operation.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Debug.LogError(operation.Error);
                yield break;
            }
            string packageVersion = operation.PackageVersion;
            Debug.Log($"Updated package Version : {packageVersion}");
            //更新资源清单
            // 更新成功后自动保存版本号，作为下次初始化的版本。
            // 也可以通过operation.SavePackageVersion()方法保存。
            bool savePackageVersion = true;
            var operation2 = package.UpdatePackageManifestAsync(packageVersion, savePackageVersion);
            yield return operation2;
            if (operation2.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Debug.LogError(operation2.Error);
            }
            //更新成功
            Debug.Log("版本号" + packageVersion);
            //资源包下载
            yield return Download();
        }
        IEnumerator Download()
        {
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var package = YooAssets.GetPackage("DefaultPackage");
            var downloader = YooAssets.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            //没有需要下载的资源
            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("没有需要下载的资源");
            }
            //需要下载的文件总数和总大小
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;
            //注册回调方法
            downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
            downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
            downloader.OnDownloadOverCallback = OnDownloadOverFunction;
            downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;
            //开启下载
            downloader.BeginDownload();
            yield return downloader;
            //检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
            {
                //下载成功
                Debug.Log("下载失败");
            }
            //StartCoroutine(Start1());
            yield return StartGame();
            Debug.Log($"Prefab name is 2");
        }
        // 协程加载方式
        IEnumerator StartGame()
        {
            //加载DLL资源
            yield return DownLoadAssets();
            //启动C#热更
#if !UNITY_EDITOR
        
#else
            // Editor下无需加载，直接查找获得HotUpdate程序集
            //Assembly hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Assembly-CSharp");
#endif
            var bytesTemp = GetAssestDate("Assembly-CSharp.dll");
            if (bytesTemp != null)
            {
                // Do something with the bytes
                Debug.Log("Successfully read " + bytesTemp.Length + " bytes.");
                Assembly hotUpdateAss = Assembly.Load(bytesTemp);
                Type type = hotUpdateAss.GetType("hello");
                GameObject go = new GameObject("TestHot");
                go.AddComponent(type);
            }
            else
            {
                // Handle the failure to read the bytes
                Debug.LogError("Failed to read bytes from   " + "Assembly-CSharp.dll");
            }
            yield return null;
        }
        private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
        {
            Debug.Log("OnStartDownloadFileFunction");
        }
        private void OnDownloadOverFunction(bool isSucceed)
        {
            Debug.Log("OnDownloadOverFunction");
        }
        private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            Debug.Log("OnDownloadProgressUpdateFunction");
        }
        private void OnDownloadErrorFunction(string fileName, string error)
        {
            Debug.Log("OnDownloadErrorFunction");
        }
        // 文件解密的示例代码
        // 注意：解密类必须配合加密类。
        // 文件解密的示例代码
        // 注意：解密类必须配合加密类。
        private class GameDecryptionServices : IDecryptionServices
        {
            public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
            {
                return 32;
            }
            public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
            {
                // 如果没有内存加密方式，可以返回空
                throw new NotImplementedException();
            }
            public Stream LoadFromStream(DecryptFileInfo fileInfo)
            {
                // 如果没有流加密方式，可以返回空
                throw new NotImplementedException();
            }
            public uint GetManagedReadBufferSize()
            {
                return 1024;
            }
        }
        // 内置文件查询服务类
        private class QueryStreamingAssetsFileServices : IQueryServices
        {
            public bool QueryStreamingAssets(string fileName)
            {
                // StreamingAssetsHelper.cs是太空战机里提供的一个查询脚本。
                string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
                return StreamingAssetsHelper.FileExists($"{buildinFolderName}/{fileName}");
            }
        }
        public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };
        private static Dictionary<string, byte[]> s_assetDates = new Dictionary<string, byte[]>();
        public static byte[] GetAssestDate(string dllName)
        {
            return s_assetDates[dllName];
        }
        private void LoadMetadataForAOTAssemblies()
        {
            foreach (var dll in AOTMetaAssemblyNames)
            {
                int err = (int)HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(GetAssestDate(dll), HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{dll}. ret:{err}");
            }

        }
        IEnumerator DownLoadAssets(Action onFinish = null)
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var assets = new List<string>
            {
                "Assembly-CSharp.dll"
            }.Concat(AOTMetaAssemblyNames);
            foreach (var asset in assets)
            {
                RawFileOperationHandle handle = package.LoadRawFileAsync(asset);
                yield return handle;
                byte[] fileDate = handle.GetRawFileData();
                s_assetDates[asset] = fileDate;
            }
            LoadMetadataForAOTAssemblies();
            onFinish?.Invoke();
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

        void LoadDll()
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
    }

    public sealed class StreamingAssetsHelper
    {
        private static readonly Dictionary<string, bool> _cacheData = new Dictionary<string, bool>(1000);

#if UNITY_ANDROID && !UNITY_EDITOR
	private static AndroidJavaClass _unityPlayerClass;
	public static AndroidJavaClass UnityPlayerClass
	{
		get
		{
			if (_unityPlayerClass == null)
				_unityPlayerClass = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
			return _unityPlayerClass;
		}
	}

	private static AndroidJavaObject _currentActivity;
	public static AndroidJavaObject CurrentActivity
	{
		get
		{
			if (_currentActivity == null)
				_currentActivity = UnityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
			return _currentActivity;
		}
	}

	/// <summary>
	/// 利用安卓原生接口查询内置文件是否存在
	/// </summary>
	public static bool FileExists(string filePath)
	{
		if (_cacheData.TryGetValue(filePath, out bool result) == false)
		{
			result = CurrentActivity.Call<bool>("CheckAssetExist", filePath);
			_cacheData.Add(filePath, result);
		}
		return result;
	}
#else
        public static bool FileExists(string filePath)
        {
            if (_cacheData.TryGetValue(filePath, out bool result) == false)
            {
                result = System.IO.File.Exists(System.IO.Path.Combine(Application.streamingAssetsPath, filePath));
                _cacheData.Add(filePath, result);
            }
            return result;
        }
#endif
    }
#if UNITY_ANDROID && UNITY_EDITOR
/// <summary>
/// 为Github对开发者的友好，采用自动补充UnityPlayerActivity.java文件的通用姿势满足各个开发者
/// </summary>
internal class AndroidPost : UnityEditor.Android.IPostGenerateGradleAndroidProject
{
	public int callbackOrder => 99;
	public void OnPostGenerateGradleAndroidProject(string path)
	{
		path = path.Replace("\\", "/");
		string untityActivityFilePath = $"{path}/src/main/java/com/unity3d/player/UnityPlayerActivity.java";
		var readContent = System.IO.File.ReadAllLines(untityActivityFilePath);
		string postContent =
			"    //auto-gen-function \n" +
			"    public boolean CheckAssetExist(String filePath) \n" +
			"    { \n" +
			"        android.content.res.AssetManager assetManager = getAssets(); \n" +
			"        try \n" +
			"        { \n" +
			"            java.io.InputStream inputStream = assetManager.open(filePath); \n" +
			"            if (null != inputStream) \n" +
			"            { \n" +
			"                 inputStream.close(); \n" +
			"                 return true; \n" +
			"            } \n" +
			"        } \n" +
			"        catch(java.io.IOException e) \n" +
			"        { \n" +
			"        } \n" +
			"        return false; \n" +
			"    } \n" +
			"}";

		if (CheckFunctionExist(readContent) == false)
			readContent[readContent.Length - 1] = postContent;
		System.IO.File.WriteAllLines(untityActivityFilePath, readContent);
	}
	private bool CheckFunctionExist(string[] contents)
	{
		for (int i = 0; i < contents.Length; i++)
		{
			if (contents[i].Contains("CheckAssetExist"))
			{
				return true;
			}
		}
		return false;
	}
}
#endif
}
