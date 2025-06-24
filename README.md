### 163MusicLyrics增加WebApi功能
- 项目基于`https://github.com/jitwxs/163MusicLyrics`
- 增加WebApi功能，支持通过HTTP请求获取歌曲列表和歌词

### 接口
- 测试接口: `/test`, 测试接口，返回`Hello World!
- 获取歌曲列表: `/listSong?name=歌曲名`, 可选参数source, 0: 网易云(需后台设置Cookie) 1(默认): QQ音乐
- 获取歌词: `/getLyric?id=歌曲ID`, 根据列表接口返回的id获取歌词信息, 也可传入source参数

### 打包修改文件
1. `MusicLyricApp.csproj` 增加依赖
```xml
<PackageReference Include="Microsoft.AspNetCore.App" Version="9.0.2" />
```
2. `Program.cs` 增加启动WebApi的代码
```c#
using System.Threading.Tasks;

Logger.Info("Application starting...");
// 新增下面这一行即可
Task.Run(() => WebApiStartup.StartWebApi(args));
```
3. `Program.cs`同目录下新建`WebApiStartup.cs`, 已放到仓库

### 其他
- 启动可以添加参数`-p8080`, 指定webapi端口, 默认`43210`

# 其他说明
- 页面效果图
- <img src="https://gcore.jsdelivr.net/gh/dhjz/163MusicLyrics-webapi@master/test.jpg" style="width: 340px;"/>
- <img src="https://gcore.jsdelivr.net/gh/dhjz/163MusicLyrics-webapi@master/listSong.jpg" style="width: 340px;"/>
- <img src="https://gcore.jsdelivr.net/gh/dhjz/163MusicLyrics-webapi@master/getLyric.jpg" style="width: 340px;"/>
