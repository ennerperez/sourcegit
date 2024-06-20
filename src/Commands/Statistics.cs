using System;

namespace SourceGit.Commands
{
    public class Statistics : Command
    {
        public Statistics(string repo, DateTimeOffset? since = null, DateTimeOffset? until = null)
        {
            _statistics = new Models.Statistics();

            var sinceArg = string.Empty;
            if (since != null)
            {
                sinceArg = $"--since=\"{since.Value.ToString("yyyy-MM-dd 00:00:00")}\"";
            }
            var untilArg = string.Empty;
            if (until != null)
            {
                untilArg = $"--until=\"{until.Value.ToString("yyyy-MM-dd 00:00:00")}\"";
            }

            WorkingDirectory = repo;
            Context = repo;
            Args = $"log --date-order --branches --remotes {sinceArg} {untilArg} --pretty=format:\"%ct$%cn\""; 
        }

        public Models.Statistics Result()
        {
            Exec();
            _statistics.Complete();
            return _statistics;
        }

        protected override void OnReadline(string line)
        {
            var dateEndIdx = line.IndexOf('$', StringComparison.Ordinal);
            if (dateEndIdx == -1)
                return;

            var dateStr = line.Substring(0, dateEndIdx);
            var date = 0.0;
            if (!double.TryParse(dateStr, out date))
                return;

            _statistics.AddCommit(line.Substring(dateEndIdx + 1), date);
        }

        private readonly Models.Statistics _statistics = null;
    }
}
