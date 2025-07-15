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
└── samples/              # 使用示例