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
using Updater.Native;

[assembly: GitHub("sourcegit-scm", "SourceGit")]

namespace Updater
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            OS.IBackend backend = null;
            if (OperatingSystem.IsWindows())
            {
                backend = new Windows();
            }
            else if (OperatingSystem.IsMacOS())
            {
                backend = new MacOS();
            }
            else if (OperatingSystem.IsLinux())
            {
                backend = new Linux();
            }

            //TODO: Not OS supported
            if (backend != null)
            {
                await backend.Start(args);
            }
        }

        private static Assembly Assembly => Assembly.GetExecutingAssembly();
        internal static string Repo => Assembly.GetCustomAttribute<GitHubAttribute>()?.ToString();
        private static string Owner => Assembly.GetCustomAttribute<GitHubAttribute>()?.Owner;
        internal static string Name => Assembly.GetCustomAttribute<GitHubAttribute>()?.Repo;
        internal static string AssetName => Assembly.GetCustomAttribute<GitHubAttribute>()?.AssetName;

        internal static Release LatestRelease;

        internal static async Task GetLatestReleaseAsync()
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
                        LatestRelease = JsonSerializer.Deserialize<Release>(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}