namespace ApnaKrishi.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<Product> BestSellers { get; set; } = new();
        public List<Product> NewArrivals { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Product> Fertilizers { get; set; } = new();
        public List<Product> Seeds { get; set; } = new();
        public List<Product> Pesticides { get; set; } = new();
        public List<Product> FarmingTools { get; set; } = new();
    }
}
