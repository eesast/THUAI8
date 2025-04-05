using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace installer.Page
{
    public partial class DeveloperPage : ContentPage
    {
        private bool _resourceKeyGenerated = false;
        private string _encryptedKeyFilePath = string.Empty;

        public bool ResourceKeyGenerated
        {
            get => _resourceKeyGenerated;
            set
            {
                _resourceKeyGenerated = value;
                OnPropertyChanged(nameof(ResourceKeyGenerated));
            }
        }

        public DeveloperPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private async void OnGenerateKeyClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SecretIDEntry.Text) ||
                    string.IsNullOrWhiteSpace(SecretKeyEntry.Text) ||
                    string.IsNullOrWhiteSpace(EncryptionPasswordEntry.Text))
                {
                    await DisplayAlert("输入错误", "请填写所有必填字段", "确定");
                    return;
                }

                // 显示进度指示
                StatusLabel.Text = "正在生成密钥...";
                StatusLabel.TextColor = Colors.Blue;
                GenerateKeyButton.IsEnabled = false;

                // 在后台线程中执行密钥生成
                await Task.Run(() =>
                {
                    try
                    {
                        // 创建安全随机密钥和IV
                        using (Aes aes = Aes.Create())
                        {
                            // 从密码生成密钥
                            using (var deriveBytes = new Rfc2898DeriveBytes(
                                EncryptionPasswordEntry.Text,
                                new byte[16], // 静态盐，在解密时需要相同
                                10000,
                                HashAlgorithmName.SHA256))
                            {
                                aes.Key = deriveBytes.GetBytes(32); // 256位密钥
                                aes.IV = deriveBytes.GetBytes(16);  // 128位IV
                            }

                            // 加密SecretID和SecretKey
                            var encryptedSecretID = EncryptString(SecretIDEntry.Text, aes.Key, aes.IV);
                            var encryptedSecretKey = EncryptString(SecretKeyEntry.Text, aes.Key, aes.IV);

                            // 创建密钥文件内容
                            string resourceContent = $"{Convert.ToBase64String(aes.Key)}\n{Convert.ToBase64String(aes.IV)}\n{encryptedSecretID}\n{encryptedSecretKey}";

                            try
                            {
                                // 为嵌入式资源创建目录
                                string resourceDirPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Raw");
                                Directory.CreateDirectory(resourceDirPath);

                                // 保存密钥文件
                                _encryptedKeyFilePath = Path.Combine(resourceDirPath, "secured_key.csv");
                                File.WriteAllText(_encryptedKeyFilePath, resourceContent);

                                // 更新UI必须在主线程中执行
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    KeyFilePathLabel.Text = _encryptedKeyFilePath;
                                    ResourceKeyGenerated = true;
                                    StatusLabel.Text = "密钥已生成，请将其添加为嵌入式资源";
                                    StatusLabel.TextColor = Colors.Green;
                                    GenerateKeyButton.IsEnabled = true;
                                });
                            }
                            catch (Exception ex)
                            {
                                DebugTool.LogException(ex, "保存加密密钥文件");
                                MainThread.BeginInvokeOnMainThread(async () =>
                                {
                                    await DisplayAlert("错误", $"保存密钥文件失败: {ex.Message}", "确定");
                                    StatusLabel.Text = "保存密钥文件失败，请检查日志";
                                    StatusLabel.TextColor = Colors.Red;
                                    GenerateKeyButton.IsEnabled = true;
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugTool.LogException(ex, "生成密钥加密过程");
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("错误", $"加密密钥失败: {ex.Message}", "确定");
                            StatusLabel.Text = "加密密钥失败，请重试";
                            StatusLabel.TextColor = Colors.Red;
                            GenerateKeyButton.IsEnabled = true;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                // 处理UI线程异常
                await DisplayAlert("错误", $"生成密钥时出错: {ex.Message}", "确定");
                StatusLabel.Text = "密钥生成失败，请重试";
                StatusLabel.TextColor = Colors.Red;
                GenerateKeyButton.IsEnabled = true;
                DebugTool.LogException(ex, "生成密钥");
            }
        }

        private string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(
                        memoryStream,
                        aes.CreateEncryptor(),
                        CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        private void OnOpenFolderClicked(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_encryptedKeyFilePath) && File.Exists(_encryptedKeyFilePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{_encryptedKeyFilePath}\"");
                }
                else
                {
                    StatusLabel.Text = "密钥文件不存在，请先生成密钥";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"无法打开文件位置: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
                DebugTool.LogException(ex, "打开文件位置");
            }
        }

        private async void OnCopyPathClicked(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_encryptedKeyFilePath))
                {
                    await Clipboard.SetTextAsync(_encryptedKeyFilePath);
                    StatusLabel.Text = "路径已复制到剪贴板";
                    StatusLabel.TextColor = Colors.Green;
                }
                else
                {
                    StatusLabel.Text = "密钥文件不存在，请先生成密钥";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"无法复制路径: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
                DebugTool.LogException(ex, "复制路径");
            }
        }

        private async void OnExitButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // 返回到主Shell
                await Shell.Current.GoToAsync("//InstallPage");
            }
            catch (Exception ex)
            {
                // 如果导航失败，记录错误并尝试使用其他导航方式
                DebugTool.LogException(ex, "退出开发者模式");
                try
                {
                    // 尝试返回
                    await Shell.Current.GoToAsync("..");
                }
                catch
                {
                    // 忽略错误
                }
            }
        }
    }
}