using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Updater.Native
{
    public class Linux : OS.IBackend
    {
        public void Launch(string workingDir = "")
        {
        }

        public async Task Start(string[] args)
        {
#if DEBUG
            //args = [Directory.GetCurrentDirectory()];
#endif
            if (args.Any())
            {
                Console.WriteLine($"{Program.Name} Updater");
                var instances = Process.GetProcessesByName(Program.Name);
                if (instances.Any())
                    foreach (var instance in instances)
                        instance.Kill();

                if (!await Update(args) || !File.Exists($"{Program.Name}.dll"))
                    return;


                Console.WriteLine($"Restarting {Program.Name}...");
                await Task.Delay(5000);
                Launch(args[0]);
            }
            else
            {
                var tempDir = String.Empty;
                tempDir = Path.GetTempPath();
                File.Copy("Updater.dll", Path.Combine(tempDir, "Updater.dll"), true);

#if DEBUG
                var files = new[] { "Updater.dll", "Updater.deps.json", "Updater.runtimeconfig.json" };
                var _ = files.Select(m => new FileInfo(m))
                    .All(m =>
                    {
                        m.CopyTo(Path.Combine(tempDir, m.Name), true);
                        return true;
                    });
#endif

                Process.Start(new ProcessStartInfo(Path.Combine(tempDir, "dotnet Updater.dll")) { Arguments = Directory.GetCurrentDirectory() });
            }
        }

        public async Task<bool> Update(string[] args)
        {
            try
            {
                await Program.GetLatestReleaseAsync();

                var assetName = Program.AssetName;
                var os = ".linux-x64";

                if (string.IsNullOrEmpty(assetName)) assetName = $"{Program.Name}{os}.zip";

                var assetUrl = Program.LatestRelease.Assets.FirstOrDefault(m => m.Name == assetName);
                var url = Program.LatestRelease.AssetsUrl;
                if (assetUrl != null) url = assetUrl.BrowserDownloadUrl;
                if (string.IsNullOrEmpty(url)) url = Program.Repo;

                using var client = new HttpClient();
                var bytes = await client.GetByteArrayAsync(url);
                if (bytes.Length <= 0)
                {
                    return true;
                }

                var tempPath = Path.Combine(Path.GetTempPath(), Program.Name);
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                var tempFileName = Path.Combine(tempPath, $"{Program.LatestRelease.Id}.bin");
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
                await File.WriteAllBytesAsync(tempFileName, bytes);
                if (!File.Exists(tempFileName))
                {
                    return true;
                }

                await using var fs = new FileStream(tempFileName, FileMode.Open);
                ZipFile.ExtractToDirectory(fs, tempPath, overwriteFiles: true);
                //var cd = new DirectoryInfo(Directory.GetCurrentDirectory());
                var cd = new DirectoryInfo(args[0]);
                var td = new DirectoryInfo(Path.Combine(tempPath, Program.Name));
                var result = td.GetFiles().All(m =>
                {
                    m.MoveTo(Path.Combine(cd.FullName, m.Name), true);
                    return true;
                });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }
    }
}