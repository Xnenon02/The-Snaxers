namespace TheSnaxers.ViewModels;

public class ChocolateGalleryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int CocoaPercentage { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string FlagUrl { get; set; } = string.Empty;
    public string DisplayImage => string.IsNullOrEmpty(ImageUrl) ? "/images/placeholder.png" : ImageUrl;
}