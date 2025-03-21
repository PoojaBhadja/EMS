
using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class FilterDataVm 
    {
        public int Month { get; set; } = DateTime.UtcNow.Month;

    }
}
