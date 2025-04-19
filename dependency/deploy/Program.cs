using installer.Data;
using installer.Model;
using installer.Services;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;

Logger Log = LoggerProvider.FromConsole();


Tencent_Cos Cloud = new Tencent_Cos("1352014406", "ap-beijing", "thuai8", Log);
Cloud.UpdateSecret(args[0], args[1]);

Downloader d = new Downloader();
d.Cloud.UpdateSecret(args[0], args[1]);
d.Data.Config.InstallPath = @"D:\a\mirror\";

d.Log.Partner.Add(Log);
// 每次更新需要更新默认值
d.CurrentVersion = new TVersion();

switch (args[2])
{
    case "check":

        if (d.CheckUpdate())
        {
            foreach (var r in d.Data.MD5Update)
            {
                Log.LogInfo($"{r.state}, {r.name}");
            }

            d.Data.SaveMD5Data();
            List<Task> l = new List<Task>();
            foreach (var r in d.Data.MD5Update)
            {
                var n = r.name.Replace('\\', '/');
                n = n.TrimStart('.').TrimStart('/');
                if (r.state == System.Data.DataRowState.Added || r.state == System.Data.DataRowState.Modified)
                {
                    l.Add(Cloud.UploadFileAsync(Path.Combine(d.Data.Config.InstallPath, r.name), n));
                }
                else if (r.state == System.Data.DataRowState.Deleted)
                {
                    l.Add(Cloud.DeleteFileAsync(n));
                }
            }
            Task.WaitAll(l.ToArray());
        }
        else
        {
            Log.LogInfo("Nothing to update");
        }

        d.Data.SaveMD5Data();
        Cloud.UploadFile(d.Data.MD5DataPath, "hash.json");

        Cloud.UploadFile(Path.Combine(d.Data.Config.InstallPath, "CAPI", "cpp", "API", "src", "AI.cpp"),
            $"Templates/t.{d.CurrentVersion.TemplateVersion}.cpp");
        Cloud.UploadFile(Path.Combine(d.Data.Config.InstallPath, "CAPI", "python", "PyAPI", "AI.py"),
            $"Templates/t.{d.CurrentVersion.TemplateVersion}.py");
        Log.LogInfo("User code uploaded.");

        var list = (from i in d.Data.MD5Data
                    select i.Key.Replace(Path.DirectorySeparatorChar, '/').TrimStart('.').TrimStart('/')).ToArray();
        Log.LogInfo(list[0]);
        using (FileStream s = new FileStream(Path.Combine(d.Data.Config.InstallPath, "compress.csv"), FileMode.Create, FileAccess.Write))
        using (StreamWriter w = new StreamWriter(s))
        {
            foreach (var item in list)
            {
                w.WriteLine("https://thuai8-1352014406.cos.ap-beijing.myqcloud.com/" + item);
            }
        }
        Cloud.UploadFile(Path.Combine(d.Data.Config.InstallPath, "compress.csv"), "compress.csv");
        Log.LogInfo("Compress csv generated.");
        break;
    case "upload":
        d.UpdateMD5();
        Cloud.DownloadFile(@"D:\a\publish\secured_key.csv", "secured_key.csv");
        ZipFile.CreateFromDirectory(@"D:\a\publish", @$"D:\a\Installer_v{d.CurrentVersion.InstallerVersion}.zip", CompressionLevel.SmallestSize, false);
        Cloud.UploadFile(@$"D:\a\Installer_v{d.CurrentVersion.InstallerVersion}.zip", $"Setup/Installer_v{d.CurrentVersion.InstallerVersion}.zip");
        break;
}