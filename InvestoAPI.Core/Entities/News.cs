using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class News
    {
        public int NewsId { get; set; }

        public string Symbol { get; set; }

        public DateTimeOffset PublishedDate { get; set; }

        public string Title { get; set; }

        public string Image { get; set; }

        public string Site { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }
    }
}
