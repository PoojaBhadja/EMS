
using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class FixedCutOffVm : FixedCutOffInput
    {
        public int Id { get; set; }

        public Guid Guid { get; set; }
    }

    public class FixedCutOffInput
    {
        public string Name { get; set; } = null!;

    }
}
