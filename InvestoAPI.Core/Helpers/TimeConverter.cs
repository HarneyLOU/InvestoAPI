using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Helpers
{
    public static class TimeConverter
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp);
            return dtDateTime;
        }
    }
}
