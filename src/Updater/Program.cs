using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Updater.Attributes;
using Updater.Models;

[assembly: GitHub("sourcegit-scm", "SourceGit")]

namespace Updater
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("SourceGit Updater");
            Console.WriteLine("");
            var instances = Process.GetProcessesByName(Name);
            if (instances.Any())
                foreach (var instance in instances)
                    instance.Kill();
            if (!await Update(args) || !File.Exists("SourceGit.exe"))
                return;
            Console.WriteLine("Restarting SourceGit...");
            await Task.Delay(5000);
            Start();
        }

        private static Assembly Assembly => Assembly.GetExecutingAssembly();
        private static string Repo => Assembly.GetCustomAttribute<GitHubAttribute>()?.ToString();
        private static string Owner => Assembly.GetCustomAttribute<GitHubAttribute>()?.Owner;
        private static string Name => Assembly.GetCustomAttribute<GitHubAttribute>()?.Repo;
        private static string AssetName => Assembly.GetCustomAttribute<GitHubAttribute>()?.AssetName;

        private static Release s_latestRelease = null;

        private static Version s_latestVersion = null;

        private static async Task<bool> Update(string[] args)
        {
            try
            {
                s_latestVersion = await GetLatestReleaseAsync();

                var assetName = AssetName;
                var os = string.Empty;
                if (OperatingSystem.IsWindows())
                {
                    os = ".win";
                }
                else if (OperatingSystem.IsMacOS())
                {
                    os = ".macOS";
                }
                else if (OperatingSystem.IsLinux())
                {
                    os = ".linux-x64";
                }

                if (string.IsNullOrEmpty(assetName)) assetName = $"{Name}{os}.zip";

                var assetUrl = s_latestRelease.Assets.FirstOrDefault(m => m.Name == assetName);
                var url = s_latestRelease.AssetsUrl;
                if (assetUrl != null) url = assetUrl.BrowserDownloadUrl;
                if (string.IsNullOrEmpty(url)) url = Repo;

                using (var client = new HttpClient())
                {
                    var bytes = await client.GetByteArrayAsync(url);
                    if (bytes.Length > 0)
                    {
                        var tempPath = Path.Combine(Path.GetTempPath(), Name);
                        if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                        var tempFileName = Path.Combine(tempPath, $"{s_latestRelease.Id}.bin");
                        if (File.Exists(tempFileName)) File.Delete(tempFileName);
                        await File.WriteAllBytesAsync(tempFileName, bytes);
                        if (File.Exists(tempFileName))
                        {
                            using (var fs = new FileStream(tempFileName, FileMode.Open))
                            {
                                ZipFile.ExtractToDirectory(fs, tempPath, overwriteFiles: true);
                                var cd = new DirectoryInfo(Directory.GetCurrentDirectory());
                                var td = new DirectoryInfo(Path.Combine(tempPath, Name));
                                var _ = td.GetFiles().All(m =>
                                {
                                    m.MoveTo(Path.Combine(cd.FullName, m.Name), true);
                                    return true;
                                });
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private static async Task<Version> GetLatestReleaseAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var url = new Uri($"https://api.github.com/repos/{Owner}/{Name}/releases/latest");
                    client.DefaultRequestHeaders.Add("User-Agent", Name);
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        s_latestRelease = JsonSerializer.Deserialize<Release>(content);
                    }
                }

                return s_latestRelease?.GetVersion();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        private static void Start(string workingDir = "")
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("SourceGit.exe") { UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true, WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory() });
            }
            else if (OperatingSystem.IsMacOS())
            {
                //TODO: macOS Launcher
            }
            else if (OperatingSystem.IsLinux())
            {
                //TODO: Linux Launcher
            }
        }
    }
}