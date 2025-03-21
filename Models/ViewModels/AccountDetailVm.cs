
using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class AccountDetailVm : AccountDetailInput
    {
        public int? Id { get; set; }
        public Guid? Guid { get; set; }
        public string? BankName { get; set; }
        public string? DisplayName { get; set; }

    }

    public class AccountDetailInput
    {
        [Required(ErrorMessage = "Field is required.")]
        public string? CardHolderName { get; set; }

        [Required(ErrorMessage = "Field is required.")]
        public int? BankId { get; set; }

    }
}
