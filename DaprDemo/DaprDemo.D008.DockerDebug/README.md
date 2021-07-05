# Docker Debug

[Debug .NET Core within a container](https://code.visualstudio.com/docs/containers/debug-netcore)

Create Project
```
dotnet new webapi -n DaprDemo.D008.DockerDebug
cd DaprDemo.D008.DockerDebug
```

Open the Command Palette (⇧⌘P) and enter Docker: Add Docker Files to Workspace

Debug using "Docker .Net Core Launch" configuration

Setup SSL as described in the link at the top