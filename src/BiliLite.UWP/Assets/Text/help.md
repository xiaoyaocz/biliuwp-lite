### 说明

此应用是哔哩哔哩第三方UWP客户端！第三方客户端！第三方客户端！第三方客户端！

使用遇到问题，请先阅读以下内容，如果以下内容无法解决你的问题，请至此处开启一个新的讨论https://github.com/ywmoyue/biliuwp-lite/discussions 


### 权限说明

* 访问Internet连接

* 后台媒体播放：无此权限最小化应用会暂停视频

* 使用视频库：存放下载的视频

* 使用图片库：存放视频截图

### 应用无法联网

大概率是系统开启了网络代理的问题，请关闭网络代理或按以下文章设置代理：

[https://www.jianshu.com/p/9d1566aa94cf](https://www.jianshu.com/p/9d1566aa94cf)

### 视频或直播无法播放

如果你遇到无法播放视频问题，请参考以下解决方案

* 尝试更换视频清晰度

* 如果是付费视频，在网页或手机客户端购买后再看

* 在播放设置打开/关闭硬解视频

* 在播放设置中更换视频类型

* 尝试更新您的显卡驱动或使用核显打开应用

* 如果你的视频编码选择了HEVC，请检查是否安装了 [HEVC扩展](ms-windows-store://pdp/?productid=9n4wgh0z6vhq)

* 如果你的视频编码选择了AV1，请检查是否安装了[AV1扩展](ms-windows-store://pdp/?productid=9MVZQVXJBQ9V)，同时部分清晰度可能需要HEVC扩展


### 播放视频掉帧或卡死

可能是显卡垂直同步问题

N卡参考：[https://www.ithome.com/html/it/318382.htm](https://www.ithome.com/html/it/318382.htm)

A卡参考: [http://bbs.pcbeta.com/viewthread-1830950-1-1.html](http://bbs.pcbeta.com/viewthread-1830950-1-1.html)

### 视频或直播出现绿屏

尝试在播放设置打开/关闭硬解视频

### 应用全局快捷键

快捷键 | 功能 
-|-|-
鼠标中键/侧键 | 返回上一页/关闭标签页
Ctrl + W | 关闭标签页
Ctrl + T | 新建标签页

### 播放器快捷键

快捷键 | 功能 
-|-|-
空格 | 播放/暂停
回车 / F11 / F  | 全屏/取消全屏 
Esc| 取消全屏 
F12 / W | 铺满窗口/取消铺满 
O / P | 跳过OP(快进90秒) 
↑ | 增大10%音量 *
↓ | 减小10%音量 *
→ | 快进3秒 
← | 回退3秒 
F10 | 截图 
Z / N / < | 上一P
X / M / > | 下一P
F9 / D | 打开/关闭弹幕
F3 / V | 静音/取消静音
F1 / ; | 减少播放速度
F2 / ' | 加快播放速度

*调节的是视频音量而非系统音量

### 部分图片无法显示

在应用商店中下载 Webp图片扩展

Webp图片扩展安装地址:[ms-windows-store://pdp/?productid=9PG2DK419DRG](ms-windows-store://pdp/?productid=9PG2DK419DRG)

### 中文乱码

系统设置-时间和语言-语言-管理语言设置-管理-非Unicode程序的语言-更改系统区域设置为中文,重启电脑

参考:https://jingyan.baidu.com/article/d8072ac4ba20cfec94cefd48.html

### 视频没有弹幕

在播放器中打开弹幕设置，查看弹幕数量，如果弹幕数量不为0，请尝试到设置-弹幕屏蔽中检查关键词和正则设置。

如果弹幕数量为0，请点击 `更新弹幕` 按钮更新弹幕

### 搜索框不自动跳转页面

默认搜索时会将关键词做一个匹配，例如av10086、ss36297会自动跳转到视频页面

如果您不想匹配跳转，请在关键词前加上'@'符号，如@av10086

### 下载视频失败

* 检查是否处于数据网络，如果处于数据网络请在设置中打开允许付费网络下载

* 检查磁盘空间是否充足

如果无法重新连接下载，请删除下载任务及下载文件重新下载

### 加载旧版本下载的视频

到设置-下载中打开 加载旧版下载的视频 选项

如果旧版本下载时修改过目录，也需要修改旧版本下载目录

### 默认存放位置

截图:`%USERPROFILE%\Pictures\哔哩哔哩截图`

下载:`%USERPROFILE%\Videos\哔哩哔哩下载`

`%USERPROFILE%` = `C:\Users\用户名`

### 日志文件

路径:`%USERPROFILE%\AppData\Local\Packages\5422.502643927C6AD_vn31kc91nth4r\LocalState\log`

为了你的账号安全，发送日志时请一定要确保access_key/access_token/csrf=...里的内容已清除或已经被修改为{hasValue}

[点击打开日志存放目录](OpenLog)
