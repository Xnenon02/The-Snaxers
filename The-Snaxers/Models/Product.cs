using System.ComponentModel.DataAnnotations;

namespace TheSnaxers.Models;

public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "Namn är obligatoriskt.")]
    [Display(Name = "Namn")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Märke är obligatoriskt.")]
    [Display(Name = "Märke")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste ange kakaohalten.")]
    [Range(1, 100, ErrorMessage = "Kakaohalten måste vara mellan 1 och 100.")]
    [Display(Name = "Kakaoprocent")]
    public int CocoaPercentage { get; set; }
    public string Country { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Du måste ange en beskrivning.")]
    [Display(Name = "Beskrivning")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingen choklad är gratis (tyvärr).")]
    [Range(0.01, 10000, ErrorMessage = "Priset måste vara mellan 0.01 och 10000.")]
    [Display(Name = "Pris")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Du måste ange en kategori.")]
    [Display(Name = "Kategori")]
    public string Category { get; set; } = string.Empty;

    private string? _imageUrl;
    public string? ImageUrl 
    { 
        get => string.IsNullOrEmpty(_imageUrl) ? "/images/placeholder-choco.png" : _imageUrl;
        set => _imageUrl = value;
    }
}