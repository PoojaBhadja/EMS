using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class TransactionDetailsVm
    {
        public List<DifferTransactionDetails> DifferTransactionDetails { get; set; } = new List<DifferTransactionDetails>();
        public string Id { get; set; }
        public string CardHolderName { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal UsedAmount { get; set; }
    }
    public class DifferTransactionDetails
    {
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }

    }
}
