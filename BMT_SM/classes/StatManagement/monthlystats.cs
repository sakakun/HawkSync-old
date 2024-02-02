using System.Collections.Generic;

namespace HawkSync_SM
{
    public class monthlystats
    {
        public Dictionary<int, daystat> monthstat { get; set; }
    }
    public class daystat
    {
        public List<day> daystats { get; set; }
    }
    public class day
    {
        public int Day { get; set; }
        public int Count { get; set; }
    }
}
