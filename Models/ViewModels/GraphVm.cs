
using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class GraphVm
    {
        public List<ExpenceCategoryGraphVm> ExpenceGraph { get; set; }
        public List<TransactionGraphVm> TransactionGraph { get; set; }


    }
}
