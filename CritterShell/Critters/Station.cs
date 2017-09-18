using System;
using System.Diagnostics;

namespace CritterShell.Critters
{
    public class Station
    {
        private static readonly DateTime DateNotSpecified = DateTime.MinValue.ToUniversalTime();

        public DateTime DateRemoved { get; set; }
        public DateTime DateSet { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }

        public Station(Station other)
        {
            this.DateRemoved = other.DateRemoved;
            this.DateSet = other.DateSet;
            this.ID = other.ID;
            this.Name = other.Name;
        }

        public Station(string id, string name)
            : this(id, name, Station.DateNotSpecified, Station.DateNotSpecified)
        {
        }

        public Station(string id, string name, DateTime dateSet, DateTime dateRemoved)
        {
            this.DateRemoved = dateRemoved;
            this.DateSet = dateSet;
            this.ID = id;
            this.Name = name;
        }

        public int GetUptime(int month)
        {
            int startYear = this.DateSet.Year;
            int startMonth = this.DateSet.Month;
            int startDay = this.DateSet.Day;

            DateTime latestOperationalDate = this.DateRemoved;
            if (this.DateRemoved == DateTime.MaxValue.ToUniversalTime())
            {
                latestOperationalDate = DateTime.UtcNow;
            }
            Debug.Assert(latestOperationalDate >= this.DateSet, "Operation ended before station was set.");
            int endYear = latestOperationalDate.Year;
            int endMonth = latestOperationalDate.Month;
            int endDay = latestOperationalDate.Day;

            int uptime = 0;
            for (int currentYear = startYear; currentYear <= endYear; ++currentYear)
            {
                DateTime firstOfMonth = new DateTime(currentYear, month, 1);
                int daysInMonth = DateTime.DaysInMonth(currentYear, month);
                DateTime endOfMonth = firstOfMonth.AddDays(daysInMonth - 1);

                if ((firstOfMonth >= this.DateSet) && (endOfMonth <= latestOperationalDate))
                {
                    // the station operated for the entire month
                    uptime += daysInMonth;
                }
                else if ((startMonth == month) && (currentYear == endYear))
                {
                    if ((startMonth == endMonth) && (startYear == endYear))
                    {
                        // the station began and ended operation in this month
                        uptime += endDay - startDay;
                    }
                    else
                    {
                        // the station began operation in this month
                        uptime += daysInMonth - startDay;
                    }
                }
                else if ((endMonth == month) && (currentYear == startYear))
                {
                    // the station ended operation in this month
                    uptime += endDay;
                }
            }

            return uptime;
        }

        public void MergeUptime(Station other)
        {
            this.DateRemoved = this.DateRemoved > other.DateRemoved ? this.DateRemoved : other.DateRemoved;
            this.DateSet = this.DateSet < other.DateSet ? this.DateSet : other.DateSet;
        }
    }
}
