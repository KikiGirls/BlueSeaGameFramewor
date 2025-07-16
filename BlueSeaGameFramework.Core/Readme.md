GameFramework.sln                # 解决方案文件
├── Core/                       # 核心模块
│   ├── GameFramework.Core.csproj
│   ├── README.md               # 模块说明
│   └── src/
├── Modules/                    # 可选功能模块
│   ├── Physics/
│   ├── AI/
│   └── Networking/
├── Runtime/                    # 运行时支持
├── Editor/                     # 编辑器扩展
├── Tests/                      # 单元测试
└── Demos/ 


GameFramework.Core/
├── src/
│   ├── Systems/            # 核心系统
│   │   ├── EventSystem.cs
│   │   └── Scheduler.cs
│   ├── Utilities/          # 工具类
│   │   ├── ObjectPool.cs
│   │   └── Debugger.cs
│   └── Interfaces/         # 接口定义
│       ├── IModule.cs
│       └── IService.cs
├── Properties/
└── Resources/             # 嵌入式资源


GameFramework.Physics/
├── src/
│   ├── Core/               # 核心实现
│   │   ├── CollisionSystem.cs
│   │   └── Rigidbody.cs
│   ├── Interfaces/         # 公开接口
│   │   ├── IPhysicsWorld.cs
│   │   └── ICollider.cs
│   ├── Extensions/         # 扩展方法
│   │   └── PhysicsExtensions.cs
│   └── Utils/             # 内部工具
│       └── SpatialHash.cs
├── tests/                 # 测试代码（可选）
├── docs/                 # 模块文档
└── samples/

顶级命名空间 - 公司/组织名

BlueSeaGameFramework

项目层 - 区分不同项目

BlueSeaGameFramework.Client

BlueSeaGameFramework.Server

BlueSeaGameFramework.Shared

功能模块层 - 按功能划分

Core (核心系统)

AI (人工智能)

UI (用户界面)

Network (网络通信)

Audio (音频系统)

子模块层 - 进一步细分

Core.Utilities (工具类)

Core.DI (依赖注入)

AI.BehaviorTree (行为树)

UI.Widgets (UI控件)# 使用示例