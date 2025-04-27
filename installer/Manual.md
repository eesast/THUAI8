# THUAI8 下载器使用说明

- 首次打开下载器后，会在 `C:\Users\用户名\Documents\` 路径下生成 `THUAI8` 文件夹，用于存储下载器的配置信息和缓存，请勿随意修改该文件夹内容。

## Installer（安装器）

### 下载功能

选择下载路径时，请确保选择一个空文件夹。

- 首次下载完成后，`下载` 按钮会自动变为 `移动` 按钮，此时可以选择新的空文件夹路径进行移动操作。
- 下载过程中提供了 `暂停`/`继续` 按钮和 `取消` 按钮，方便您控制下载进程。
- 界面提供两个进度条：上方进度条显示已下载的文件数量，下方进度条显示当前文件的下载进度。
- 如果下载出现问题，且 `是否已下载选手包` 复选框显示为勾选状态，请关闭下载器，删除已下载的文件，并将 `C:\Users\用户名\Documents\THUAI8\config.json` 中的 `"Installed": true` 改为 `"Installed": false`。

### 更新功能

使用更新功能前，需要先点击 `检查更新` 按钮。

- **请勿对非选手包路径执行更新操作，这可能导致文件丢失。**
- **更新功能会智能保留您对选手代码所做的修改**，同时更新框架和库文件，无需担心代码被覆盖。
- 首次下载完成后建议立即检查更新，因为初始下载的版本可能不是最新的。
- 更新过程中可以在日志区域观察更新详情。

### 日志功能

下载器界面底部设有日志显示区：

- 日志区域实时显示下载、更新等操作的进度信息和状态报告。
- 遇到问题时，日志区域会显示错误信息，帮助您诊断和解决问题。
- 日志内容自动滚动，始终显示最新的操作信息。

## Launcher（启动器）

### 开发环境

- C++：在 `%InstallPath%\CAPI\cpp\CAPI.sln` 中进行开发，通常只需修改 `%InstallPath%\CAPI\cpp\API\src\AI.cpp` 文件，然后编译项目。
- Python：确保计算机已安装 `python` 和 `pip`，执行 `%InstallPath%\CAPI\python\generate_proto.cmd`（Windows系统）或 `%InstallPath%\CAPI\python\generate_proto.sh`（Mac/Linux系统），等待 protos 文件夹生成后，在 `%InstallPath%\CAPI\python\PyAPI\AI.py` 中编写代码。

修改启动器中的 `IP`、`Port`、`Language`、`Playback File` 或 `Playback Speed` 参数后，必须点击 `保存` 按钮使设置生效。

### Debug（调试）

**注意：启动器可能需要管理员权限才能正常运行**

#### 本地调试步骤

1. 设置 `Port` 值，默认为 `8888`
2. 选择 `Server` 标签页，调整 `Team Count` 和 `Ship Count` 参数，然后启动 *Server*
3. 切换到 `Client` 标签页，输入 `127.0.0.1` 作为服务器地址，添加相应数量的 *Player*，设置各 *Player* 的参数和 `Language`，然后启动 *Client*
   - *Player* 总数应为 `TeamCount×(ShipCount+1)`，所有 *Player* 加入后游戏才会开始
   - `Player ID` 为 `0` 表示 `Home`（基地），其他值表示 `Ship`（舰船）
   - `Player Mode` 选择 `API` 表示使用您编写的AI代码；选择 `Manual` 表示手动控制。手动模式下，`Player ID` 不能为 `0`，`Ship Type` 不能为 `0`，且一台电脑不建议有多个手动控制的 *Player*

#### 联机调试步骤

1. 其中一台电脑启动 *Server*
2. 其他电脑输入 *Server* 电脑的 `IP` 地址和 `Port`（可通过在命令提示符中输入 `ipconfig` 查看IP地址）
3. 所有电脑共同添加足够数量的 *Player*，然后启动 *Client*

#### 观战模式

- 如果您的电脑没有设置为 `Manual` 模式的 *Player*，需要启动 *Spectator* 来观看比赛；如果已有 `Manual` 模式的 *Player*，则无需启动 *Spectator*
- *Spectator* 的 `ID` 值应大于等于 `2026`，观看同一场比赛的多个 *Spectator* 需使用不同的 `ID` 值

### Playback（回放）

每次调试结束后，系统会在 `%InstallPath%\logic\Server` 目录中生成 `114514.thuaipb` 回放文件。在 `Playback File` 输入框中输入 `114514.thuaipb`，点击 `保存` 后点击 `启动` 即可观看回放。

- 您可以对回放文件重命名，启动回放时输入相应的文件名即可
- 通过调整 `Playback Speed` 参数可以控制回放速度，数值越大回放速度越快
- 回放模式下可以暂停和继续播放，便于分析特定局面

## Login（登录）

该功能暂未开放，敬请期待。

## Other

- 如遇到问题，建议先查看日志信息
- 欢迎在比赛交流群中提出关于比赛或软件使用的问题
- 软件功能会持续更新，请定期检查更新获取最新功能

**祝各位选手比赛顺利，Debug愉快！**

@2025 EESAST
