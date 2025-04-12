using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace installer.Services
{
    public class DownloadService
    {
        private readonly HttpClient _httpClient;
        
        public DownloadService()
        {
            _httpClient = new HttpClient();
        }
        
        public async Task DownloadFileAsync(string url, string filePath, IProgress<double> progress = null)
        {
            var tempFilePath = filePath + ".temp";
            
            try
            {
                // 确保目标目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                
                // 删除任何已存在的临时文件
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
                
                using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    
                    var totalBytes = response.Content.Headers.ContentLength ?? -1;
                    
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[8192];
                        var bytesRead = 0L;
                        
                        while (true)
                        {
                            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0) break;
                            
                            await fileStream.WriteAsync(buffer, 0, read);
                            bytesRead += read;
                            
                            if (totalBytes != -1)
                            {
                                var progressValue = (double)bytesRead / totalBytes;
                                progress?.Report(progressValue);
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
            catch
            {
                // 下载失败，清理临时文件
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
                throw;
            }
        }
    }
}