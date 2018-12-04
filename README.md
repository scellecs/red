# Red
<img align="right" width="160px" height="160px" src="Assets/Data/logo.png">

Toolchain based on Reactive Extensions for Unity Game Engine.
  
Extends the capabilities of [UniRx](https://github.com/neuecc/UniRx), allowing you to conveniently resolve dependencies and implement loose coupling between components or modules of your architecture.  

- **Containers** solve local and global dependencies and make it possible to abandon static classes, singletons.  
- **Contracts** make it easy to build MVP for UI and further interaction with business logic. 

## How to install

- Go to [Releases](https://github.com/X-Crew/Red/releases) and download latest .unitypackage
- If your Unity version is lower than 2018.3, open your Player Settings in Unity project and change Scripting Runtime Version to `.Net 4.x Equivalent`
- Import UniRx from [AssetStore](https://assetstore.unity.com/packages/tools/integration/unirx-reactive-extensions-for-unity-17276)
- Import Red

> Note:  
> Sometimes the solution is not updated to latest C# 7  
> Close your IDE and delete all .csproj and .sln from main folder  
> It will be recreated with the correct version  


## Introduction

First of all, you should be well versed in Rx and specifically in [UniRx](https://github.com/neuecc/UniRx).  
Learn this in detail if you still do not know it.  

## License

Red is licensed under [MIT License](LICENSE).