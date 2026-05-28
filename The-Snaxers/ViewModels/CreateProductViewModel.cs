using System.ComponentModel.DataAnnotations;

namespace TheSnaxers.ViewModels;

public class CreateProductViewModel
{
    [Required(ErrorMessage = "Du måste ange ett produktnamn.")]
    [StringLength(100, ErrorMessage = "Namnet får inte vara längre än 100 tecken.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste ange ett varumärke.")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste ange ett pris.")]
    [Range(0.01, 10000, ErrorMessage = "Priset måste vara positivt.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Du måste ange kakaohalt.")]
    [Range(0, 100, ErrorMessage = "Kakaohalten måste vara mellan 0 och 100%.")]
    public int CocoaPercentage { get; set; }

    [Required(ErrorMessage = "Du måste ange en vikt.")]
    [Range(1, 5000, ErrorMessage = "Vikten måste vara mellan 1 och 5000 gram.")]
    public int Weight { get; set; }

    [Required(ErrorMessage = "Du måste ange lagersaldo.")]
    [Range(0, 10000, ErrorMessage = "Lagersaldo kan inte vara negativt.")]
    public int StockLevel { get; set; }

    [Required(ErrorMessage = "Du måste ange ursprungsland.")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste välja en kategori.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste skriva en beskrivning.")]
    [StringLength(1000, ErrorMessage = "Beskrivningen är för lång.")]
    public string Description { get; set; } = string.Empty;
}