using COSXML;
using COSXML.Auth;
using COSXML.CosException;
using COSXML.Model.Object;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Formats.Tar;
using COSXML.Common;
using COSXML.Transfer;
using System;
using installer.Data;
using System.Threading.Tasks;
using COSXML.Model.Bucket;
using COSXML.Model.Tag;

// 禁用对没有调用异步API的异步函数的警告
#pragma warning disable CS1998

namespace installer.Model
{
    // 添加上传进度报告类
    public class UploadReport
    {
        public long Completed { get; set; } = 0;
        public long Total { get; set; } = 0;
        public bool BigFileTraceEnabled { get; set; } = false;
        public event EventHandler<double>? ProgressChanged;

        public void OnProgressChanged(double progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }
    }

    public class Tencent_Cos
    {
        public string Appid { get; init; }      // 设置腾讯云账户的账户标识（APPID）
        public string Region { get; init; }     // 设置一个默认的存储桶地域
        public string BucketName { get; set; }
        public Logger Log;

        protected CosXmlConfig config;
        protected CosXmlServer cosXml;
        protected TransferConfig transfer;
        protected TransferManager manager;

        public DownloadReport Report;
        public UploadReport UploadReport;

        public Tencent_Cos(string appid, string region, string bucketName, Logger? _log = null)
        {
            Appid = appid; Region = region; BucketName = bucketName;
            Log = _log ?? LoggerProvider.FromConsole();
            Log.PartnerInfo = "[COS]";
            Report = new DownloadReport();
            UploadReport = new UploadReport();

            try
            {
                // 初始化CosXmlConfig（提供配置SDK接口）
                config = new CosXmlConfig.Builder()
                            .IsHttps(true)      // 设置默认 HTTPS 请求
                            .SetAppid(Appid)    // 设置腾讯云账户的账户标识 APPID
                            .SetRegion(Region)  // 设置一个默认的存储桶地域
                            .SetDebugLog(true)  // 显示日志
                            .Build();           // 创建 CosXmlConfig 对象

                // 使用全局密钥
                string secretId = MauiProgram.SecretID;
                string secretKey = MauiProgram.SecretKey;

                // 记录使用的密钥信息（安全起见只记录前几位和长度）
                if (secretId != null)
                    Log.LogInfo($"使用SecretID: {secretId.Substring(0, Math.Min(4, secretId.Length))}*** (长度:{secretId.Length})");
                else
                    Log.LogWarning("SecretID为null");

                if (secretKey != null)
                    Log.LogInfo($"使用SecretKey: {secretKey.Substring(0, Math.Min(4, secretKey.Length))}*** (长度:{secretKey.Length})");
                else
                    Log.LogWarning("SecretKey为null");

                // 确保密钥值有效，如果没有则尝试重新加载
                if (string.IsNullOrEmpty(secretId) || secretId == "***")
                {
                    Log.LogWarning("SecretID无效或未设置 - 尝试重新从资源加载");
                    try
                    {
                        // 尝试重新加载嵌入资源中的密钥
                        if (typeof(MauiProgram).GetMethod("LoadSecretFromEmbeddedResource",
                            System.Reflection.BindingFlags.Static |
                            System.Reflection.BindingFlags.NonPublic) is System.Reflection.MethodInfo method)
                        {
                            method.Invoke(null, null);
                            secretId = MauiProgram.SecretID; // 重新获取密钥
                            secretKey = MauiProgram.SecretKey;
                            Log.LogInfo("已重新加载密钥资源");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError($"重新加载密钥资源失败: {ex.Message}");
                    }

                    // 再次检查密钥是否有效
                    if (string.IsNullOrEmpty(secretId) || secretId == "***")
                    {
                        Log.LogError("无法获取有效的SecretID，COS访问将失败");
                        secretId = "placeholder"; // 使用占位符以避免空引用异常
                    }
                }

                if (string.IsNullOrEmpty(secretKey) || secretKey == "***")
                {
                    // 再次检查密钥是否已在前面的步骤中被加载
                    if (string.IsNullOrEmpty(secretKey) || secretKey == "***")
                    {
                        Log.LogError("无法获取有效的SecretKey，COS访问将失败");
                        secretKey = "placeholder"; // 使用占位符以避免空引用异常
                    }
                }

                try
                {
                    QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, 1000);
                    cosXml = new CosXmlServer(config, cosCredentialProvider);
                    transfer = new TransferConfig()
                    {
                        DivisionForDownload = 20 << 20,     // 下载分块阈值为20MB
                        SliceSizeForDownload = 10 << 20,    // 下载分块大小为10MB
                    };
                    manager = new TransferManager(cosXml, transfer);

                    Log.LogInfo($"COS客户端初始化完成: Bucket={BucketName}, Region={Region}, APPID={Appid}");

                    // 验证密钥有效性
                    if (secretId == "placeholder" || secretKey == "placeholder")
                    {
                        Log.LogWarning("使用了占位符密钥，COS访问将很可能失败");
                    }
                    else
                    {
                        Log.LogInfo("正在验证COS密钥有效性...");
                        try
                        {
                            // 使用远程存储桶检查密钥是否有效，不影响构造函数完成
                            Task.Run(() =>
                            {
                                try
                                {
                                    if (!ValidateCredentials())
                                    {
                                        Log.LogWarning("COS密钥验证失败，下载功能可能不可用");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.LogError($"验证COS密钥时发生异常: {ex.Message}");
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.LogError($"启动密钥验证任务时出错: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError($"初始化COS客户端失败: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Tencent_Cos构造函数出错: {ex.Message}");
                throw;
            }
        }

        public void UpdateSecret(string secretId, string secretKey, long durationSecond = 1000)
        {
            QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);
            cosXml = new CosXmlServer(config, cosCredentialProvider);
            manager = new TransferManager(cosXml, transfer);
        }

        public void UpdateSecret(string secretId, string secretKey, long durationSecond, string token)
        {
            QCloudCredentialProvider cosCredentialProvider = new DefaultSessionQCloudCredentialProvider(
                secretId, secretKey, durationSecond, token
            );
            cosXml = new CosXmlServer(config, cosCredentialProvider);
            manager = new TransferManager(cosXml, transfer);
        }

        public int DownloadFile(string savePath, string? remotePath = null)
        {
            int thID = Log.StartNew();
            // download_dir标记根文件夹路径，key为相对根文件夹的路径（不带./）
            // 创建存储桶
            try
            {
                Log.LogInfo(thID, $"Download task: {{\"{remotePath}\"->\"{savePath}\"}} started.");
                // 覆盖对应文件，如果无法覆盖则报错
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Log.LogWarning(thID, $"{savePath} has existed. Original file has been deleted.");
                }
                string bucket = $"{BucketName}-{Appid}";                                // 格式：BucketName-APPID
                string localDir = Path.GetDirectoryName(savePath)     // 本地文件夹
                    ?? throw new Exception("本地文件夹路径获取失败");
                string localFileName = Path.GetFileName(savePath);    // 指定本地保存的文件名
                remotePath = remotePath?.Replace('\\', '/')?.TrimStart('.').TrimStart('/');

                Log.LogInfo(thID, $"检查文件是否存在: Bucket={bucket}, RemotePath={remotePath}");

                try
                {
                    var head = cosXml.HeadObject(new HeadObjectRequest(bucket, remotePath ?? localFileName));
                    Log.LogInfo(thID, $"文件存在，大小: {head.size} 字节");

                    long c = 0;
                    if (head.size > (1 << 20))
                    {
                        // 文件大小大于1MB则设置回调函数
                        Report.Total = head.size;
                        Report.Completed = 0;
                        Report.BigFileTraceEnabled = true;
                        var size = (head.size > 1 << 30) ?
                            string.Format("{0:##.#}GB", ((double)head.size) / (1 << 30)) :
                            string.Format("{0:##.#}MB", ((double)head.size) / (1 << 20));
                        Log.LogWarning($"Big file({size}) detected! Please keep network steady!");
                        COSXMLDownloadTask task = new COSXMLDownloadTask(bucket, remotePath ?? localFileName, localDir, localFileName);
                        task.progressCallback = (completed, total) =>
                        {
                            if (completed > 1 << 30 && completed - c > 100 << 20)
                            {
                                Log.LogDebug(string.Format("downloaded = {0:##.#}GB, progress = {1:##.##}%", ((double)completed) / (1 << 30), completed * 100.0 / total));
                                c = completed;
                            }
                            if (completed < 1 << 30 && completed - c > 10 << 20)
                            {
                                Log.LogDebug(string.Format("downloaded = {0:##.#}MB, progress = {1:##.##}%", ((double)completed) / (1 << 20), completed * 100.0 / total));
                                c = completed;
                            }
                            (Report.Completed, Report.Total) = (completed, total);
                        };
                        // 执行请求                
                        var result = manager.DownloadAsync(task).Result;
                        // 请求成功
                        if (result is not null && result.httpCode != 200 && result.httpCode != 206)
                        {
                            Log.LogError(thID, $"Download task: {{\"{remotePath}\"->\"{savePath}\"}} failed, HTTP Code: {result.httpCode}, Message: {result.httpMessage}");
                            throw new Exception($"Download task: {{\"{remotePath}\"->\"{savePath}\"}} failed, message: {result.httpCode} {result.httpMessage}");
                        }
                        Log.LogDebug(thID, $"Download task: {{\"{remotePath}\"->\"{savePath}\"}} finished.");
                    }
                    else
                    {
                        if (Report.Completed > 0 && Report.Total > 0 && Report.Completed == Report.Total)
                            Report.BigFileTraceEnabled = false;
                        var request = new GetObjectRequest(bucket, remotePath ?? localFileName, localDir, localFileName);
                        // 执行请求                
                        var result = cosXml.GetObject(request);
                        // 请求成功
                        if (result.httpCode != 200 && result.httpCode != 206)
                        {
                            Log.LogError(thID, $"Download task: {{\"{remotePath}\"->\"{savePath}\"}} failed, HTTP Code: {result.httpCode}, Message: {result.httpMessage}");
                            throw new Exception($"Download task: {{\"{remotePath}\"->\"{savePath}\"}} failed, message: {result.httpCode} {result.httpMessage}");
                        }
                        Log.LogDebug(thID, $"Download task: {{\"{remotePath}\"->\"{savePath}\"}} finished.");
                    }

                    if (Report.BigFileTraceEnabled)
                        Report.Completed = Report.Total;

                    return thID;
                }
                catch (COSXML.CosException.CosServerException serverEx)
                {
                    // 处理COS服务器返回的错误
                    Log.LogError(thID, $"COS服务器错误: 状态码={serverEx.statusCode}, 错误码={serverEx.errorCode}, 错误信息={serverEx.errorMessage}");

                    if (serverEx.statusCode == 403)
                    {
                        Log.LogError(thID, "权限错误(403 Forbidden)：可能是SecretID/SecretKey无效，或没有访问权限");
                    }
                    else if (serverEx.statusCode == 404)
                    {
                        Log.LogError(thID, $"文件不存在(404 Not Found)：路径 \"{remotePath ?? localFileName}\" 在存储桶 \"{bucket}\" 中不存在");
                    }

                    Log.LogDebug(thID, $"Download task: {{\"{remotePath}\"->\"{savePath}\"}} ended with server error.");
                    return -1;
                }
                catch (COSXML.CosException.CosClientException clientEx)
                {
                    // 处理客户端错误
                    Log.LogError(thID, $"COS客户端错误: {clientEx.Message}");
                    Log.LogDebug(thID, $"Download task: {{\"{remotePath}\"->\"{savePath}\"}} ended with client error.");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(thID, $"下载错误: {ex.Message}");
                Log.LogDebug(thID, $"Download task: {{\"{remotePath}\"->\"{savePath}\"}} ended unexpectedly.");
                return -1;
            }
        }

        public int DownloadQueue(string basePath, IEnumerable<string> queue)
        {
            int thID = Log.StartNew();
            Log.LogDebug(thID, "Batch download task started.");
            var array = queue.ToArray();
            var count = array.Length;
            if (count == 0)
                return 0;
            var comCount = 0;
            var comCountOld = Report.ComCount;
            Report.Count += count;

            var partitionar = Partitioner.Create(0, count, count / 4 > 0 ? count / 4 : count);
            Parallel.ForEach(partitionar, (range, loopState) =>
            {
                for (long i = range.Item1; i < range.Item2; i++)
                {
                    if (loopState.IsStopped)
                        break;
                    string local = Path.Combine(basePath, array[i]);
                    int subID = -1;
                    try
                    {
                        subID = DownloadFile(local, array[i]);
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex.Message + " on " + array[i]);
                    }
                    finally
                    {
                        Interlocked.Increment(ref comCount);
                        Report.ComCount = comCount + comCountOld;
                        Log.LogInfo(thID, $"Child process: {subID} finished.");
                    }
                }
            });
            Log.LogInfo(thID, "Batch download task finished.");
            Report.ComCount = comCount + comCountOld;
            return thID;
        }

        public void ArchieveUnzip(string zipPath, string targetDir)
        {
            Stream? inStream = null;
            Stream? gzipStream = null;
            int thID = Log.StartNew();
            Log.LogInfo(thID, $"Zip {zipPath} is being extracted...");
            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                using (inStream = File.OpenRead(zipPath))
                {
                    using (gzipStream = new GZipStream(inStream, CompressionMode.Decompress))
                    {
                        TarFile.ExtractToDirectory(gzipStream, targetDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError(thID, ex.Message);
            }
            finally
            {
                if (gzipStream != null) gzipStream.Close();
                if (inStream != null) inStream.Close();
                Log.LogInfo(thID, $"Zip has been extracted to {targetDir}");
            }
        }

        public void UploadFile(string localPath, string targetPath, IProgress<double>? progress = null)
        {
            int thID = Log.StartNew();
            Log.LogInfo(thID, $"Upload task: {{\"{localPath}\"->\"{targetPath}\"}} started.");
            string bucket = $"{BucketName}-{Appid}";
            targetPath = targetPath.TrimStart('.').TrimStart('/');
            try
            {
                if (!File.Exists(localPath))
                {
                    Log.LogError($"File \"{localPath}\" doesn't exist!");
                    return;
                }
                FileInfo fi = new FileInfo(localPath);
                long c = 0;
                //初始化TransferConfig
                TransferConfig transferConfig = new TransferConfig();

                COSXMLUploadTask uploadTask = new COSXMLUploadTask(bucket, targetPath);
                uploadTask.SetSrcPath(localPath);

                // 检查文件大小，大于1MB时设置进度回调
                if (fi.Length > (1 << 20))
                {
                    var size = (fi.Length > 1 << 30) ?
                        string.Format("{0:##.#}GB", ((double)fi.Length) / (1 << 30)) :
                        string.Format("{0:##.#}MB", ((double)fi.Length) / (1 << 20));
                    Log.LogWarning($"上传大文件({size})，请保持网络稳定!");

                    UploadReport.Total = fi.Length;
                    UploadReport.Completed = 0;
                    UploadReport.BigFileTraceEnabled = true;

                    uploadTask.progressCallback = (completed, total) =>
                    {
                        double progressValue = (double)completed / total;
                        progress?.Report(progressValue);
                        UploadReport.Completed = completed;
                        UploadReport.Total = total;
                        UploadReport.OnProgressChanged(progressValue);

                        if (completed > 1 << 30 && completed - c > 100 << 20)
                        {
                            Log.LogDebug(string.Format("uploaded = {0:##.#}GB, progress = {1:##.##}%",
                                ((double)completed) / (1 << 30),
                                progressValue * 100.0));
                            c = completed;
                        }
                        if (completed < 1 << 30 && completed - c > 10 << 20)
                        {
                            Log.LogDebug(string.Format("uploaded = {0:##.#}MB, progress = {1:##.##}%",
                                ((double)completed) / (1 << 20),
                                progressValue * 100.0));
                            c = completed;
                        }
                    };
                }

                COSXMLUploadTask.UploadTaskResult r = manager.UploadAsync(uploadTask).Result;
                if (r.httpCode != 200)
                    Log.LogError(thID, $"Upload task: {{\"{localPath}\"->\"{targetPath}\"}} failed, message: {r.httpMessage}");
                string eTag = r.eTag;
                //到这里应该是成功了，但是因为我没有试过，也不知道具体情况，可能还要根据result的内容判断
                Log.LogInfo(thID, $"Upload task: {{\"{localPath}\"->\"{targetPath}\"}} finished.");

                // 清理上传进度
                if (UploadReport.BigFileTraceEnabled)
                {
                    UploadReport.Completed = UploadReport.Total;
                    UploadReport.BigFileTraceEnabled = false;
                    progress?.Report(1.0); // 确保进度到达100%
                    UploadReport.OnProgressChanged(1.0);
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }
        }

        public bool DetectFile(string remotePath)
        {
            int thID = Log.StartNew();
            string bucket = $"{BucketName}-{Appid}";
            remotePath = remotePath.TrimStart('.').TrimStart('/');
            Log.LogInfo(thID, $"检查文件是否存在: Bucket={bucket}, RemotePath={remotePath}");

            //执行请求
            try
            {
                DoesObjectExistRequest requestd = new DoesObjectExistRequest(bucket, remotePath);
                bool exists = cosXml.DoesObjectExist(requestd);
                Log.LogInfo(thID, exists ?
                    $"文件存在: {remotePath}" :
                    $"文件不存在: {remotePath}");
                return exists;
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                Log.LogError(thID, $"COS客户端错误: {clientEx.Message}");
                return false;
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                Log.LogError(thID, $"COS服务器错误: 状态码={serverEx.statusCode}, 错误码={serverEx.errorCode}, 错误信息={serverEx.errorMessage}");

                if (serverEx.statusCode == 403)
                {
                    Log.LogError(thID, "权限错误(403 Forbidden)：可能是SecretID/SecretKey无效，或没有访问权限");
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.LogError(thID, $"检查文件时发生错误: {ex.Message}");
                return false;
            }
        }

        public void DeleteFile(string remotePath)
        {
            int thID = Log.StartNew();
            Log.LogInfo(thID, $"Delete task: \"{remotePath}\" started.");
            string bucket = $"{BucketName}-{Appid}";
            remotePath = remotePath.TrimStart('.').TrimStart('/');
            //执行请求
            try
            {
                if (!DetectFile(remotePath))
                {
                    Log.LogWarning($"{remotePath} doesn't exist!");
                    return;
                }
                DeleteObjectRequest request = new DeleteObjectRequest(bucket, remotePath);
                DeleteObjectResult result = cosXml.DeleteObject(request);
                Log.LogInfo(thID, $"Delete task: \"{remotePath}\" finished.");
            }
            catch (CosClientException clientEx)
            {
                //请求失败
                Log.LogError("CosClientException: " + clientEx);
            }
            catch (CosServerException serverEx)
            {
                //请求失败
                Log.LogError("CosServerException: " + serverEx.GetInfo());
            }
        }

        public List<string> EnumerateDir(string remotePath)
        {
            int thID = Log.StartNew();
            var result = new List<string>();
            string bucket = $"{BucketName}-{Appid}";
            remotePath = remotePath.TrimStart('.').TrimStart('/');
            Log.LogInfo(thID, $"Enumerate files in {remotePath}");

            bool truncated = false;
            string marker = string.Empty;
            do
            {
                GetBucketRequest request = new GetBucketRequest(bucket);
                request.SetPrefix(remotePath);
                request.SetDelimiter("/");
                if (!string.IsNullOrEmpty(marker))
                    request.SetMarker(marker);
                //执行请求
                GetBucketResult res = cosXml.GetBucket(request);
                ListBucket info = res.listBucket;
                result.AddRange(info.contentsList.Select(i => i.key).Where(i => i != remotePath));
                foreach (var dir in info.commonPrefixesList)
                {
                    result.AddRange(EnumerateDir(dir.prefix));
                }
                truncated = info.isTruncated;
                marker = info.nextMarker;
            } while (truncated);

            return result;
        }
        #region 异步方法包装
        public Task<int> DownloadFileAsync(string savePath, string? remotePath = null)
        {
            return Task.Run(() => DownloadFile(savePath, remotePath));
        }

        public Task<int> DownloadQueueAsync(string basePath, IEnumerable<string> queue)
        {
            return Task.Run(() => DownloadQueue(basePath, queue));
        }
        public Task ArchieveUnzipAsync(string zipPath, string targetDir)
        {
            return Task.Run(() => ArchieveUnzip(zipPath, targetDir));
        }

        public Task UploadFileAsync(string localPath, string targetPath, IProgress<double>? progress = null)
        {
            return Task.Run(() => UploadFile(localPath, targetPath, progress));
        }

        public Task DeleteFileAsync(string remotePath)
        {
            return Task.Run(() => DeleteFile(remotePath));
        }

        #endregion

        public bool ValidateCredentials()
        {
            int thID = Log.StartNew();
            string bucket = $"{BucketName}-{Appid}";
            Log.LogInfo(thID, "验证COS密钥有效性...");

            try
            {
                // 尝试列出存储桶，这个操作需要有效的密钥
                GetBucketRequest request = new GetBucketRequest(bucket);
                // 设置最大返回数量为1，只需要验证连接成功即可
                request.SetMaxKeys("1");
                GetBucketResult result = cosXml.GetBucket(request);

                if (result.httpCode == 200)
                {
                    Log.LogInfo(thID, "COS密钥验证成功，连接正常");
                    return true;
                }
                else
                {
                    Log.LogError(thID, $"COS密钥验证失败，HTTP状态码: {result.httpCode}");
                    return false;
                }
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                if (serverEx.statusCode == 403)
                {
                    Log.LogError(thID, "COS密钥验证失败: 权限不足或密钥无效 (403 Forbidden)");
                    Log.LogDebug(thID, $"详细错误: {serverEx.GetInfo()}");
                }
                else
                {
                    Log.LogError(thID, $"COS服务器错误: {serverEx.GetInfo()}");
                }
                return false;
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                Log.LogError(thID, $"COS客户端错误: {clientEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Log.LogError(thID, $"验证COS密钥时发生异常: {ex.Message}");
                return false;
            }
        }
    }
}
