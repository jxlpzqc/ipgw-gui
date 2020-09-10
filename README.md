<img src="https://raw.githubusercontent.com/jxlpzqc/ipgw-gui/master/Logo.png" alt="(NEU IPGateway Assistant)" style="zoom:33%;" />

# 东大网关助手

![MIT License](https://img.shields.io/badge/License-MIT-green.svg)![Language](https://img.shields.io/badge/Language-C#-blue.svg)

**东北大学校园网关Windows图形客户端**，采用[unbyte/ipgw](https://github.com/unbyte/ipgw)的作为连接驱动程序。

![ScreenShoot1](https://s1.ax1x.com/2020/09/10/wJWp7D.jpg)



**当前版本为测试版！可能会产生各种功能BUG**



# 构建和运行

该程序可采用Visual Studio 或命令行构建运行，目标平台可采用 `.net framework 4.6.1` 或 `.net core 3.1 `

## 采用Visual Studio构建运行

- 打开Visual Studio，打开项目sln文件
- 选择运行项目为 `NEU.IPGateway.UI`
- 选择运行平台为`.net framework 4.6.1` 或 `.net core 3.1`
- 按 `Ctrl+F5` 或者单击 `Debug` > `Start Without Debugging`

## 采用命令行构建运行

- 请先安装dotnet sdk

- 进入项目目录

```
cd src/NEU.IPGateway/NEU.IPGateway.UI
```

- 若构建平台为.net framework 4.6.1

```
dotnet run --framework=net461
```

- 若构建平台为.net core 3.1

```
dotnet run --framework=net461
```



# 功能列表

- [x] 账号管理
- [x] 账号保护
- [x] 连接、断开、强制断开
- [x] 显示当前账号信息
- [x] 后台托盘运行
- [x] 接入校园网自动提醒连接
- [x] 开机自动运行
- [x] 国际化（目前支持中文和英文，期待更多语言）
- [x] 网络诊断
- [x] 官方服务链接
- [ ] 自动更新
- [ ] 错误报告上传



# 规划

- [ ] 发布正式版
- [ ] 打包安装包
- [ ] 搭建自动更新和错误报告服务器
- [ ] 搭建网站进行软件分发
- [ ] 使用.NET实现网关驱动，使其成为纯.NET项目，便于跨平台
- [ ] 使用NEU.IPGateway.Core项目，集成Xamarin.Forms开发Android App
- [ ] 增加更多语言支持

# 贡献

我们非常希望您能为该项目贡献源代码或相关规划项目。

如果您在使用该程序时遭遇各种Bug或其他问题，请在该代码仓库新建issue或发送电子邮件到<jxlpzqc@live.com> 。

我们也欢迎您Fork本仓库，并提交 Pull Request。

# 开放源代码许可

[MIT License](https://github.com/jxlpzqc/ipgw-gui/blob/master/LICENSE)