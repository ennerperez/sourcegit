using System;

namespace Updater.Attributes
{
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
}