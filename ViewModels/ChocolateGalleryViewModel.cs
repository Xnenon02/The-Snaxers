namespace TheSnaxers.ViewModels;

public class ChocolateGalleryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    // Dina extra fält för AC3
    public string CountryName { get; set; } = string.Empty;
    public string FlagUrl { get; set; } = string.Empty;
    // För att visa placeholder om bild saknas (AC2)
    public string DisplayImage => string.IsNullOrEmpty(ImageUrl) ? "/images/placeholder.png" : ImageUrl;
}