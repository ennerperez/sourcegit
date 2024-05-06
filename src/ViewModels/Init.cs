﻿using System.IO;
using System.Threading.Tasks;

namespace SourceGit.ViewModels
{
    public class Init : Popup
    {
        public string TargetPath
        {
            get => _targetPath;
            set => SetProperty(ref _targetPath, value);
        }

        public Init(string path)
        {
            TargetPath = path;
            View = new Views.Init() { DataContext = this };
        }

        public override Task<bool> Sure()
        {
            ProgressDescription = $"Initialize git repository at: '{_targetPath}'";

            return Task.Run(() =>
            {
                var succ = new Commands.Init(HostPageId, _targetPath).Exec();
                if (!succ)
                    return false;

                var gitDir = Path.GetFullPath(Path.Combine(_targetPath, ".git"));

                CallUIThread(() =>
                {
                    var node = new RepositoryNode()
                    {
                        Id = _targetPath,
                        FullPath = _targetPath,
                        Name = Path.GetFileName(_targetPath),
                        GitDir = gitDir,
                        Bookmark = 0,
                        IsRepository = true,
                    };
                    Preference.AddNode(node);
                });

                return true;
            });
        }

        private string _targetPath;
    }
}
