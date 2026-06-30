namespace ApnaKrishi.Models.ViewModels
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
