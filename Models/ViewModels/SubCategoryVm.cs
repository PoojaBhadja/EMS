
namespace Models.ViewModels
{
 
    public class SubCategoryInput
    {
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public int SelectedTransactionType { get; set; }

    }
}
