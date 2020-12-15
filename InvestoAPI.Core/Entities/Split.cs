using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class Split
    {
        public int SplitId { get; set; }

        public DateTimeOffset Date { get; set; }

        public int StockId { get; set; }

        public float Numerator { get; set; }

        public float Denominator { get; set; }
    }
}
