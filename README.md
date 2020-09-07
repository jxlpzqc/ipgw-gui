# 东大网关助手

![MIT License](https://img.shields.io/badge/license-MIT-green.svg)

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

