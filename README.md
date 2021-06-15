# 哔哩哔哩 UWP 

第三方哔哩哔哩UWP客户端

下载及常见问题见：[https://www.showdoc.com.cn/biliuwpv4](https://www.showdoc.com.cn/biliuwpv4)

## 截图

![QQ截图20200730165337.png](https://vip1.loli.net/2020/08/02/rGLMwtVSYmaKgxi.png)

## 说明

运行项目需要添加FFmpeg引用。

- Nuget添加FFmpegInteropX.FFmpegUWP包。此包不支持HTTPS，可能无法正常观看直播。

- [下载](https://xiaoyaocz.lanzoui.com/i6aLtpn0kcf)并引用已编译的包，此包支持HTTPS。

	修改BiliLite.csproj里的FFmpeg路径
		
	```
	<Content Include="FFmpeg路径\$(PlatformTarget)\bin\*.dll" />
	```
	
- 自定义编译。详见[FFmpegInteropX](https://github.com/ffmpeginteropx/FFmpegInteropX)

## 参考及引用

[SYEngine](https://github.com/ShanYe/SYEngine)

[FFmpegInteropX](https://github.com/ffmpeginteropx/FFmpegInteropX)

[bilibili-API-collect](https://github.com/SocialSisterYi/bilibili-API-collect)

[bilibili-grpc-api](https://github.com/SeeFlowerX/bilibili-grpc-api)

[FontAwesome5](https://github.com/MartinTopfstedt/FontAwesome5)

[waslibs](https://github.com/wasteam/waslibs)