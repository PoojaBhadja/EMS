using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.Input
{
    public class AccountAmountTransferInput
    {
        public int FromAccountDetailId { get; set; }
        public int ToAccountDetailId { get; set; }

        public decimal Amount { get; set; }
    }
}
