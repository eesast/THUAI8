using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace installer.Services
{
    public class DownloadService : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isPaused;
        private readonly object _lockObject = new object();

        public DownloadService()
        {
            _httpClient = new HttpClient();
        }

        public async Task DownloadFileAsync(string url, string filePath, IProgress<double> progress)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _isPaused = false;

            try
            {
                var fileInfo = new FileInfo(filePath);
                var directory = fileInfo.Directory;
                if (!directory.Exists)
                {
                    directory.Create();
                }

                var tempFilePath = filePath + ".tmp";
                var downloadPosition = 0L;

                if (File.Exists(tempFilePath))
                {
                    downloadPosition = new FileInfo(tempFilePath).Length;
                }

                using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
                {
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token))
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[8192];
                        var bytesRead = 0L;

                        while (true)
                        {
                            while (_isPaused)
                            {
                                await Task.Delay(100, _cancellationTokenSource.Token);
                            }

                            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                            if (read == 0) break;

                            await fileStream.WriteAsync(buffer, 0, read, _cancellationTokenSource.Token);
                            bytesRead += read;

                            if (totalBytes != -1)
                            {
                                var progressValue = (double)bytesRead / totalBytes;
                                progress.Report(progressValue);
                            }
                        }
                    }
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(tempFilePath, filePath);
            }
            catch (OperationCanceledException)
            {
                // 下载被取消
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void Pause()
        {
            lock (_lockObject)
            {
                _isPaused = true;
            }
        }

        public void Resume()
        {
            lock (_lockObject)
            {
                _isPaused = false;
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}