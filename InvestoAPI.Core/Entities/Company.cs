using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestoAPI.Core.Entities
{
    public class Company
    {
        [Key]
        public int StockId { get; set; }

        public string Name { get; set; }

        [Required]
        public string Symbol {get; set; }

        public DateTime IpoDate { get; set; }

        public string Image { get; set; }

        public string Url { get; set; }

        public string Exchange { get; set; }

        public string Industry { get; set; }
    
        public string Currency { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MarketCap { get; set; }

        public string Description { get; set; }

        [StringLength(10)]
        public string MarketIndex { get; set; }

    }
}
