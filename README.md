# HybridCLR
HybridCLR_接入
YooAssets
    1.接入后需安装SBP插件包
    2.DLL文件的打包收集，需设置RawFile
HybridCLR
    1.WebGl需要全局安装，具体参考hybridcld官网文档
    2.注意AOT泛型问题
具体流程
    1.先编译DLL，hybrid先Generate all
    2.Build DLL,并复制到 StreamingAssests下和Prefabs下
    3.YooAssets配置收集规则，打包
    4.新建一个Boot场景，和Start场景。Boot场景新挂一个Boot脚本，作为启动。boot模式需要注意