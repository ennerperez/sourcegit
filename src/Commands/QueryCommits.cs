﻿using System;
using System.Collections.Generic;

namespace SourceGit.Commands
{
    public class QueryCommits : Command
    {
        public QueryCommits(string repo, string limits, bool needFindHead = true)
        {
            _endOfBodyToken = $"----- END OF BODY {Guid.NewGuid()} -----";

            WorkingDirectory = repo;
            Context = repo;
            Args = $"log --date-order --no-show-signature --decorate=full --pretty=format:\"%H%n%P%n%D%n%aN±%aE%n%at%n%cN±%cE%n%ct%n%B%n{_endOfBodyToken}\" " + limits;
            _findFirstMerged = needFindHead;
        }

        public List<Models.Commit> Result()
        {
            var rs = ReadToEnd();
            if (!rs.IsSuccess)
                return _commits;

            var nextPartIdx = 0;
            var start = 0;
            var end = rs.StdOut.IndexOf('\n', start);
            var max = rs.StdOut.Length;
            while (end > 0)
            {
                var line = rs.StdOut.Substring(start, end - start);
                switch (nextPartIdx)
                {
                    case 0:
                        _current = new Models.Commit() { SHA = line };
                        _commits.Add(_current);
                        break;
                    case 1:
                        ParseParent(line);
                        break;
                    case 2:
                        ParseDecorators(line);
                        break;
                    case 3:
                        _current.Author = Models.User.FindOrAdd(line);
                        break;
                    case 4:
                        _current.AuthorTime = ulong.Parse(line);
                        break;
                    case 5:
                        _current.Committer = Models.User.FindOrAdd(line);
                        break;
                    case 6:
                        _current.CommitterTime = ulong.Parse(line);
                        start = end + 1;
                        end = rs.StdOut.IndexOf(_endOfBodyToken, start, StringComparison.Ordinal);
                        if (end > 0)
                        {
                            if (end > start)
                                _current.Body = rs.StdOut.Substring(start, end - start).TrimEnd();

                            start = end + _endOfBodyToken.Length + 1;
                            end = start >= max ? -1 : rs.StdOut.IndexOf('\n', start);
                        }

                        nextPartIdx = 0;
                        continue;
                    default:
                        break;
                }

                nextPartIdx++;
                start = end + 1;
                end = rs.StdOut.IndexOf('\n', start);
            }

            if (_findFirstMerged && !_isHeadFounded && _commits.Count > 0)
                MarkFirstMerged();

            return _commits;
        }

        private void ParseParent(string data)
        {
            if (data.Length < 8)
                return;

            var idx = data.IndexOf(' ', StringComparison.Ordinal);
            if (idx == -1)
            {
                _current.Parents.Add(data);
                return;
            }

            _current.Parents.Add(data.Substring(0, idx));
            _current.Parents.Add(data.Substring(idx + 1));
        }

        private void ParseDecorators(string data)
        {
            if (data.Length < 3)
                return;

            var subs = data.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var sub in subs)
            {
                var d = sub.Trim();
                if (d.StartsWith("tag: refs/tags/", StringComparison.Ordinal))
                {
                    _current.Decorators.Add(new Models.Decorator()
                    {
                        Type = Models.DecoratorType.Tag,
                        Name = d.Substring(15),
                    });
                }
                else if (d.EndsWith("/HEAD", StringComparison.Ordinal))
                {
                    continue;
                }
                else if (d.StartsWith("HEAD -> refs/heads/", StringComparison.Ordinal))
                {
                    _current.IsMerged = true;
                    _current.Decorators.Add(new Models.Decorator()
                    {
                        Type = Models.DecoratorType.CurrentBranchHead,
                        Name = d.Substring(19),
                    });
                }
                else if (d.Equals("HEAD"))
                {
                    _current.IsMerged = true;
                    _current.Decorators.Add(new Models.Decorator()
                    {
                        Type = Models.DecoratorType.CurrentCommitHead,
                        Name = d,
                    });
                }
                else if (d.StartsWith("refs/heads/", StringComparison.Ordinal))
                {
                    _current.Decorators.Add(new Models.Decorator()
                    {
                        Type = Models.DecoratorType.LocalBranchHead,
                        Name = d.Substring(11),
                    });
                }
                else if (d.StartsWith("refs/remotes/", StringComparison.Ordinal))
                {
                    _current.Decorators.Add(new Models.Decorator()
                    {
                        Type = Models.DecoratorType.RemoteBranchHead,
                        Name = d.Substring(13),
                    });
                }
            }

            _current.Decorators.Sort((l, r) =>
            {
                if (l.Type != r.Type)
                    return (int)l.Type - (int)r.Type;
                else
                    return l.Name.CompareTo(r.Name);
            });

            if (_current.IsMerged && !_isHeadFounded)
                _isHeadFounded = true;
        }

        private void MarkFirstMerged()
        {
            Args = $"log --since=\"{_commits[_commits.Count - 1].CommitterTimeStr}\" --format=\"%H\"";

            var rs = ReadToEnd();
            var shas = rs.StdOut.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (shas.Length == 0)
                return;

            var set = new HashSet<string>();
            foreach (var sha in shas)
                set.Add(sha);

            foreach (var c in _commits)
            {
                if (set.Contains(c.SHA))
                {
                    c.IsMerged = true;
                    break;
                }
            }
        }

        private string _endOfBodyToken = string.Empty;
        private List<Models.Commit> _commits = new List<Models.Commit>();
        private Models.Commit _current = null;
        private bool _findFirstMerged = false;
        private bool _isHeadFounded = false;
    }
}
