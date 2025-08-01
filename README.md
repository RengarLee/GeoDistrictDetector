# GeoLocationCityDetector

一个用于根据地理坐标点判断所属城市信息的.NET命令行工具。

## 功能特性

- 🗺️ **坐标城市查询**: 根据经纬度坐标查找所属的城市信息
- 📁 **多格式支持**: 支持CSV和JSON格式的城市数据文件
- 🎯 **精确算法**: 使用Haversine公式计算距离，支持多边形边界判断
- 💻 **命令行友好**: 支持命令行参数和交互式模式
- ⚡ **高性能**: 异步处理，支持大量城市数据

## 快速开始

### 构建项目

```bash
dotnet build
```

### 运行项目

#### 1. 交互式模式
```bash
dotnet run
```

#### 2. 命令行模式
```bash
dotnet run sample-cities.csv 39.9042 116.4074
```

## 使用方法

### 数据文件格式

#### CSV格式
```csv
省份,城市,区县,经度,纬度
北京市,北京市,东城区,116.4074,39.9042
上海市,上海市,黄浦区,121.4692,31.2301
```

#### JSON格式
```json
[
  {
    "Province": "北京市",
    "City": "北京市",
    "District": "东城区",
    "CenterPoint": {
      "Latitude": 39.9042,
      "Longitude": 116.4074
    },
    "Boundary": []
  }
]
```

### 坐标格式

支持以下坐标输入格式：
- `39.9042,116.4074`
- `39.9042, 116.4074`
- `39.9042 116.4074`
- `(39.9042, 116.4074)`

## 项目结构

```
GeoLocationCityDetector/
├── Models/
│   ├── GeoPoint.cs          # 地理坐标点模型
│   └── CityInfo.cs          # 城市信息模型
├── Services/
│   ├── IGeoLocationService.cs   # 地理位置服务接口
│   └── GeoLocationService.cs    # 地理位置服务实现
├── Utils/
│   └── InputValidator.cs    # 输入验证工具
├── Program.cs               # 主程序入口
└── sample-cities.csv        # 示例城市数据
```

## 核心算法

### 1. 距离计算
使用Haversine公式计算两个地理坐标点之间的球面距离：

```csharp
double a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
           Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
           Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
return earthRadius * c;
```

### 2. 点在多边形内判断
使用射线法判断点是否在多边形边界内（当有边界数据时）。

## 示例用法

### 交互式模式示例

```
=== 地理位置城市检测器 ===

请选择操作：
1. 加载城市数据文件
2. 查询坐标所属城市
3. 显示已加载的城市数量
4. 退出
请输入选项 (1-4): 1

请输入城市数据文件路径 (支持CSV和JSON格式): sample-cities.csv
成功加载 22 个城市数据
城市数据加载成功！

请选择操作：
1. 加载城市数据文件
2. 查询坐标所属城市
3. 显示已加载的城市数量
4. 退出
请输入选项 (1-4): 2

请输入坐标 (格式：纬度,经度，如：39.9042,116.4074): 39.9042,116.4074
查询结果：
  坐标: (39.9042, 116.4074)
  所属城市: 北京市 - 北京市 - 东城区
  城市中心点: (39.9042, 116.4074)
```

### 命令行模式示例

```bash
PS> dotnet run sample-cities.csv 39.9042 116.4074
=== 地理位置城市检测器 ===

加载数据文件: sample-cities.csv
成功加载 22 个城市数据
查询坐标: (39.9042, 116.4074)
结果: 北京市 - 北京市 - 东城区
```

## 扩展功能

### 添加边界数据支持
可以在城市数据中添加边界坐标点，实现更精确的城市边界判断：

```json
{
  "Province": "北京市",
  "City": "北京市",
  "District": "东城区",
  "CenterPoint": {
    "Latitude": 39.9042,
    "Longitude": 116.4074
  },
  "Boundary": [
    {"Latitude": 39.9100, "Longitude": 116.4000},
    {"Latitude": 39.9100, "Longitude": 116.4100},
    {"Latitude": 39.9000, "Longitude": 116.4100},
    {"Latitude": 39.9000, "Longitude": 116.4000}
  ]
}
```

## 技术栈

- .NET 8.0
- C# 12
- System.Text.Json (JSON处理)

## 贡献

欢迎提交Issue和Pull Request来改进这个项目！
