using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Updater.Models
{
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
        public partial Regex SemVerRegEx();

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
}