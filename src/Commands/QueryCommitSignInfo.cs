﻿namespace SourceGit.Commands
{
    public class QueryCommitSignInfo : Command
    {
        public QueryCommitSignInfo(string repo, string sha, bool useFakeSignersFile)
        {
            WorkingDirectory = repo;
            Context = repo;

            if (useFakeSignersFile)
                Args = $"-c gpg.ssh.allowedSignersFile=/dev/null show --no-show-signature --pretty=format:\"%G? %GK\" -s {sha}";
            else
                Args = $"show --no-show-signature --pretty=format:\"%G? %GK\" -s {sha}";
        }

        public Models.CommitSignInfo Result()
        {
            var rs = ReadToEnd();
            if (!rs.IsSuccess)
                return null;

            var raw = rs.StdOut.Trim();
            if (raw.Length > 1)
                return new Models.CommitSignInfo() { VerifyResult = raw[0], Key = raw.Substring(2) };

            return null;
        }
    }
}