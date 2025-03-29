using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace installer.Services
{
    /// <summary>
    /// 模拟回放客户端，用于测试PlaybackController通信功能
    /// 注意：这个类只用于开发测试，不会在实际应用中使用
    /// </summary>
    public class MockPlaybackClient
    {
        private readonly int _controlPort;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string _playbackFilePath;
        private readonly double _initialSpeed;
        private int _currentFrame = 0;
        private int _totalFrames;
        private bool _isPaused = false;
        private bool _isConnected = false;
        private double _playbackSpeed;
        private DateTime _startTime;
        private TcpClient _controlConnection;
        
        public bool IsRunning { get; private set; }
        
        public MockPlaybackClient(string playbackFilePath, double initialSpeed, int controlPort)
        {
            _playbackFilePath = playbackFilePath;
            _initialSpeed = initialSpeed;
            _playbackSpeed = initialSpeed;
            _controlPort = controlPort;
            
            // 模拟从回放文件读取总帧数
            FileInfo fileInfo = new FileInfo(playbackFilePath);
            _totalFrames = (int)(fileInfo.Length / 1024);  // 假设每帧1KB
        }
        
        public async Task StartAsync()
        {
            try
            {
                // 模拟初始化
                await Task.Delay(500);
                
                // 连接到控制器
                await ConnectToControllerAsync();
                
                if (_isConnected)
                {
                    Debug.WriteLine($"MockClient: 已连接到控制器");
                    IsRunning = true;
                    _startTime = DateTime.Now;
                    
                    // 启动模拟播放线程
                    _ = Task.Run(RunPlaybackLoopAsync, _cts.Token);
                }
                else
                {
                    Debug.WriteLine($"MockClient: 无法连接到控制器");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MockClient: 启动错误 - {ex.Message}");
            }
        }
        
        private async Task ConnectToControllerAsync()
        {
            try
            {
                _controlConnection = new TcpClient();
                await _controlConnection.ConnectAsync(IPAddress.Loopback, _controlPort);
                _isConnected = true;
                
                // 启动命令监听
                _ = Task.Run(ListenForCommandsAsync, _cts.Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MockClient: 连接控制器失败 - {ex.Message}");
                _isConnected = false;
            }
        }
        
        private async Task ListenForCommandsAsync()
        {
            try
            {
                byte[] buffer = new byte[1024];
                NetworkStream stream = _controlConnection.GetStream();
                
                while (_isConnected && !_cts.Token.IsCancellationRequested)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                        if (bytesRead > 0)
                        {
                            string command = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            ProcessCommand(command);
                        }
                    }
                    
                    await Task.Delay(100, _cts.Token);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Debug.WriteLine($"MockClient: 监听命令错误 - {ex.Message}");
                _isConnected = false;
            }
        }
        
        private void ProcessCommand(string command)
        {
            Debug.WriteLine($"MockClient: 收到命令 - {command}");
            
            if (command == "PAUSE")
            {
                _isPaused = true;
                SendStatusResponse();
            }
            else if (command == "RESUME")
            {
                _isPaused = false;
                SendStatusResponse();
            }
            else if (command == "STATUS" || command == "INFO")
            {
                SendStatusResponse();
            }
            else if (command.StartsWith("SPEED:"))
            {
                string speedValue = command.Substring(6);
                if (double.TryParse(speedValue, out double speed))
                {
                    _playbackSpeed = Math.Max(0.25, Math.Min(4.0, speed));
                    Debug.WriteLine($"MockClient: 速度已更新为 {_playbackSpeed}x");
                    SendStatusResponse();
                }
            }
        }
        
        private void SendStatusResponse()
        {
            try
            {
                // 计算当前播放时间
                TimeSpan elapsed = TimeSpan.FromSeconds(_currentFrame / (30.0 * _playbackSpeed));
                string timeStr = $"{elapsed.Minutes:00}:{elapsed.Seconds:00}";
                
                // 构建状态响应
                string response = $"FRAME:{_currentFrame}/{_totalFrames};TIME:{timeStr};PAUSED:{(_isPaused ? "TRUE" : "FALSE")};COMPLETED:{(_currentFrame >= _totalFrames ? "TRUE" : "FALSE")};SPEED:{_playbackSpeed}";
                
                // 发送响应
                if (_isConnected)
                {
                    NetworkStream stream = _controlConnection.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                    Debug.WriteLine($"MockClient: 发送状态 - {response}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MockClient: 发送状态错误 - {ex.Message}");
                _isConnected = false;
            }
        }
        
        private async Task RunPlaybackLoopAsync()
        {
            try
            {
                while (IsRunning && _currentFrame < _totalFrames && !_cts.Token.IsCancellationRequested)
                {
                    if (!_isPaused)
                    {
                        // 模拟帧率为30fps
                        await Task.Delay((int)(1000 / (30 * _playbackSpeed)), _cts.Token);
                        
                        _currentFrame++;
                        
                        // 每10帧发送一次状态更新
                        if (_currentFrame % 10 == 0)
                        {
                            SendStatusResponse();
                        }
                    }
                    else
                    {
                        // 暂停状态下降低CPU使用率
                        await Task.Delay(100, _cts.Token);
                    }
                }
                
                // 播放结束
                if (_currentFrame >= _totalFrames)
                {
                    _currentFrame = _totalFrames;
                    SendStatusResponse();
                    Debug.WriteLine("MockClient: 播放已完成");
                }
                
                IsRunning = false;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("MockClient: 播放被取消");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MockClient: 播放循环错误 - {ex.Message}");
            }
            finally
            {
                IsRunning = false;
            }
        }
        
        public void Stop()
        {
            try
            {
                _cts.Cancel();
                IsRunning = false;
                
                if (_controlConnection != null)
                {
                    _controlConnection.Close();
                    _controlConnection.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MockClient: 停止错误 - {ex.Message}");
            }
        }
    }
} 