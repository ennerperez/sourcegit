using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceGit.Models
{
    public class StatisticsSample
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public double Percent { get; set; }
    }

    public class StatisticsReport
    {
        public int Total { get; set; } = 0;
        public List<StatisticsSample> Samples { get; set; } = new List<StatisticsSample>();
        public List<StatisticsSample> ByCommitter { get; set; } = new List<StatisticsSample>();

        public void AddCommit(int index, string committer)
        {
            Total++;
            Samples[index].Count++;

            if (_mapByCommitter.TryGetValue(committer, out var value))
            {
                value.Count++;
            }
            else
            {
                var sample = new StatisticsSample() { Name = committer, Count = 1 };
                _mapByCommitter.Add(committer, sample);
                ByCommitter.Add(sample);
            }
        }

        public void Complete()
        {
            ByCommitter.Sort((l, r) => r.Count - l.Count);
            
            var total = 0;
            foreach (var s in ByCommitter)
            {
                total += s.Count;
            }
            foreach (var s in ByCommitter)
            {
                s.Percent = (s.Count * 100.00) / total;
            }

            _mapByCommitter.Clear();
        }

        private readonly Dictionary<string, StatisticsSample> _mapByCommitter = new Dictionary<string, StatisticsSample>();
    }

    public class Statistics
    {
        public StatisticsReport Year { get; set; } = new StatisticsReport();
        public StatisticsReport Month { get; set; } = new StatisticsReport();
        public StatisticsReport Week { get; set; } = new StatisticsReport();

        public Statistics()
        {
            _utcStart = DateTime.UnixEpoch;
            _today = DateTime.Today;
            _thisWeekStart = _today.AddSeconds(-(int)_today.DayOfWeek * 3600 * 24 - _today.Hour * 3600 - _today.Minute * 60 - _today.Second);
            _thisWeekEnd = _thisWeekStart.AddDays(7);

            string[] monthNames = System.Globalization.DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames;

            for (int i = 0; i < monthNames.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(monthNames[i]))
                {
                    Year.Samples.Add(new StatisticsSample { Name = monthNames[i].ToUpperInvariant(), Count = 0, });
                }
            }

            var monthDays = DateTime.DaysInMonth(_today.Year, _today.Month);
            for (int i = 0; i < monthDays; i++)
            {
                Month.Samples.Add(new StatisticsSample
                {
                    Name = $"{i + 1}",
                    Count = 0,
                });
            }

            string[] weekDayNames = System.Globalization.DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames;

            for (int i = 0; i < weekDayNames.Length; i++)
            {
                Week.Samples.Add(new StatisticsSample
                {
                    Name = weekDayNames[i].ToUpperInvariant(),
                    Count = 0,
                });
            }
        }

        public string Since()
        {
            return _today.ToString("yyyy-01-01 00:00:00");
        }

        public void AddCommit(string committer, double timestamp)
        {
            var time = _utcStart.AddSeconds(timestamp).ToLocalTime();
            if (time.CompareTo(_thisWeekStart) >= 0 && time.CompareTo(_thisWeekEnd) < 0)
            {
                Week.AddCommit((int)time.DayOfWeek, committer);
            }

            if (time.Month == _today.Month)
            {
                Month.AddCommit(time.Day - 1, committer);
            }

            Year.AddCommit(time.Month - 1, committer);
        }

        public void Complete()
        {
            Year.Complete();
            Month.Complete();
            Week.Complete();
        }

        private readonly DateTime _utcStart;
        private readonly DateTime _today;
        private readonly DateTime _thisWeekStart;
        private readonly DateTime _thisWeekEnd;
    }
}
