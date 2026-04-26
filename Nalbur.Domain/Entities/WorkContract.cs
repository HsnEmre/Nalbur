using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nalbur.Domain.Entities
{
    public class WorkContract : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        public string? CustomerName { get; set; }

        public string? CustomerPhone { get; set; }

        public string WorkDescription { get; set; } = string.Empty;

        public string Materials { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public DateTime ContractDate { get; set; } = DateTime.Today;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
