using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;

using SourceGit.Services;

[assembly: GitHub("sourcegit-scm", "SourceGit")]

namespace SourceGit.Services
{
    public interface IUpdaterService : IDisposable
    {
        event EventHandler<GitHubUpdaterService.Release> NewVersionAvailable;
        Task<Version> GetLatestReleaseAsync();
        Task CheckForUpdateAsync();
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class GitHubAttribute : Attribute
    {
        public GitHubAttribute()
        {
        }

        public GitHubAttribute(string owner, string repo, string assetName = "") : base()
        {
            Owner = owner;
            Repo = repo;
            AssetName = assetName;
        }

        public string Owner { get; private set; }
        public string Repo { get; private set; }
        public string AssetName { get; private set; }

        public override string ToString()
        {
            return $"https://github.com/{Owner}/{Repo}";
        }
    }

    public partial class GitHubUpdaterService : IUpdaterService
    {
        private Assembly Assembly => Assembly.GetExecutingAssembly();

        public string Title => Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        public string Product => Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        public Version Version => Version.Parse(Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "1.0.0");
        public string Repo => Assembly.GetCustomAttribute<GitHubAttribute>()?.ToString();
        public string Owner => Assembly.GetCustomAttribute<GitHubAttribute>()?.Owner;
        public string Name => Assembly.GetCustomAttribute<GitHubAttribute>()?.Repo;
        public string AssetName => Assembly.GetCustomAttribute<GitHubAttribute>()?.AssetName;

        internal Release LatestRelease = null;

        public async Task<Version> GetLatestReleaseAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var url = new Uri($"https://api.github.com/repos/{Owner}/{Name}/releases/latest");
                    client.DefaultRequestHeaders.Add("User-Agent", Title);
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        LatestRelease = JsonSerializer.Deserialize<Release>(content);
                    }
                }

                return LatestRelease?.GetVersion();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public Version GetLatestRelease()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var url = new Uri($"https://api.github.com/repos/{Owner}/{Name}/releases/latest");
                    client.DefaultRequestHeaders.Add("User-Agent", Title);
                    var response = client.Send(new HttpRequestMessage(HttpMethod.Get, url));
                    if (response.IsSuccessStatusCode)
                    {
                        var stream = response.Content.ReadAsStream();
                        var reader = new StreamReader(stream);
                        LatestRelease = JsonSerializer.Deserialize<Release>(reader.ReadToEnd());
                    }
                }

                return LatestRelease?.GetVersion();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public event EventHandler<Release> NewVersionAvailable;

        public async Task CheckForUpdateAsync()
        {
            try
            {
                LatestVersion = await GetLatestReleaseAsync();
                if (Version < LatestVersion)
                {
                    OnNewVersionAvailable(LatestRelease);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void CheckForUpdate()
        {
            try
            {
                LatestVersion = GetLatestRelease();
                if (Version < LatestVersion)
                {
                    OnNewVersionAvailable(LatestRelease);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static Version LatestVersion { get; private set; }

        public void Dispose()
        {
            // TODO release managed resources here
        }

        public partial class Release
        {
            public Release()
            {
                Assets = new HashSet<Asset>();
            }

            [JsonPropertyName("tarball_url")]
            public string TarballUrl { get; set; }

            //[JsonPropertyName("author")]
            //public Author Author { get; set; }

            [JsonPropertyName("published_at")]
            public DateTimeOffset? PublishedAt { get; set; }

            [JsonPropertyName("created_at")]
            public DateTimeOffset CreatedAt { get; set; }

            [JsonPropertyName("prerelease")]
            public bool Prerelease { get; set; }

            [JsonPropertyName("draft")]
            public bool Draft { get; set; }

            [JsonPropertyName("body")]
            public string Body { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("target_commitish")]
            public string TargetCommitish { get; set; }

            [JsonPropertyName("tag_name")]
            public string TagName { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("upload_url")]
            public string UploadUrl { get; set; }

            [JsonPropertyName("assets_url")]
            public string AssetsUrl { get; set; }

            [JsonPropertyName("html_url")]
            public string HtmlUrl { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("zipball_url")]
            public string ZipballUrl { get; set; }

            [JsonPropertyName("assets")]
            public ICollection<Asset> Assets { get; set; }


            [GeneratedRegex(@"v?\=?((?:[0-9]{1,}\.{0,}){1,})\-?(.*)?")]
            private partial Regex SemVerRegEx();

            public Version GetVersion()
            {
                Version result = null;
                var versionMatch = SemVerRegEx().Match(TagName);
                if (versionMatch.Success)
                {
                    Version.TryParse(versionMatch.Groups[1].Value, out result);
                }

                return result;
            }
        }

        public class Asset
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("label")]
            public string Label { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("content_type")]
            public string ContentType { get; set; }

            [JsonPropertyName("size")]
            public int Size { get; set; }

            [JsonPropertyName("download_count")]
            public int DownloadCount { get; set; }

            [JsonPropertyName("created_at")]
            public DateTimeOffset CreatedAt { get; set; }

            [JsonPropertyName("updated_at")]
            public DateTimeOffset UpdatedAt { get; set; }

            [JsonPropertyName("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }

            //[JsonPropertyName("uploader")]
            //public Author Uploader { get; set; }
        }

        protected virtual void OnNewVersionAvailable(Release e)
        {
            NewVersionAvailable?.Invoke(this, e);
        }

    }
}