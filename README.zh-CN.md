# Find The Needle 存档编辑器 · 简体中文版

基于 [Yukitus86/find-the-needle-savegame-editor](https://github.com/Yukitus86/find-the-needle-savegame-editor) 的 v2.1.0 维护，保留上游 MIT 许可证及版权。
上游基线提交：`fd9d7ea836eca9a6756105ca03288b20c9935c48`。
本项目为非官方工具，与游戏开发者无隶属关系。

## 此 Fork 的区别

- 新增简体中文，保留英语和德语；标题栏切换，语言选择自动记忆。
- 首次启动默认简体中文；已有 `lang=en` / `lang=de` 设置继续生效。
- 界面、对话框、科技、物品、建筑、备份说明已汉化。术语暂译及限制见 [术语说明](docs/TERMINOLOGY.zh-CN.md)。
- **每次点击“保存”或按 Ctrl+S，都会将 `state.debug_used` 写为布尔值 `false`。** 这与上游每次写入 `true` 的行为不同，包括原值已经是 `true` 的存档。
- 自动备份仍保存修改前的文件。恢复备份和导出 `.bin` 会保留原始内容；再次正常保存时才强制写入 `false`。此行为并不保证游戏加载后的标记状态，也不保证任何排行榜结果。
- 生涯档案 `profile.cfg` 继续只读，原始数据中的 `board_*`、`run_timed` 继续锁定；`debug_used` 由保存逻辑管理。

## 使用

1. Windows 10/11 x64，直接运行 `FindTheNeedleEditor.exe`，不需要安装 .NET。
2. 编辑前先退出游戏，防止游戏的自动保存覆盖修改。
3. 建议先备份存档，通过“打开文件…”选择副本测试。程序也会自动寻找游戏存档槽。
4. 修改后点击“保存”（Ctrl+S）。每次保存前自动备份；“备份”页面可以手动备份、恢复及打开备份目录。
5. 标题栏“中文 / EN / DE”切换语言；保留已加载存档、未保存修改及当前页面。

正式版默认存档位置：
`%APPDATA%\Godot\app_userdata\FindTheNeedle\saves\slot_0.dat`

自动备份目录：游戏用户目录下的 `NeedleEditorBackups`。
语言设置：`%APPDATA%\FindTheNeedleEditor\settings.ini`。
测试过程中没有写回真实存档；交付程序供用户自行操作，点击保存会写入当前打开的文件。

## 构建与验证

安装 .NET 10 SDK 后运行 `build.cmd`。继续沿用上游的两步流程：

```powershell
.\build.cmd
# 内部先 dotnet build -c Release，再 dotnet publish -c Release --no-build
# 最后复制单文件 EXE 到仓库根目录
```

开发辅助：

```powershell
$env:FTN_OPEN = 'D:\YourTestDirectory\slot_0.dat'
$env:FTN_SETTINGS = 'D:\YourTestDirectory\settings.ini'
.\FindTheNeedleEditor.exe --shots D:\YourTestDirectory\shots --lang zh-CN
# --lang en / de / zh-CN 只覆盖当前启动，不覆盖已保存的语言选择
dotnet run --project tests\CoreTests
dotnet run --project tests\SaveProbe -- '<只读源存档>' '<测试输出目录>'
dotnet run --project tests\UiTests -c Release -- '<测试存档副本>' '<界面测试输出目录>'
```

截图功能仅读取存档并渲染页面，不保存编辑。`FTN_OPEN` 选择初始打开的文件，侧栏仍列出自动检测到的游戏存档；需要完全隔离路径的开发测试使用 `UiTests`。
真实游戏加载、玩法完整性和排行榜行为均需用户实测。本期没有新增配方分析或价格计数功能。
