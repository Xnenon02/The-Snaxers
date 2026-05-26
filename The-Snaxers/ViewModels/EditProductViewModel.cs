using System.ComponentModel.DataAnnotations;

namespace TheSnaxers.Models;

public class EditProductViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste ange ett produktnamn.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste ange ett varumärke.")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste ange ett pris.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Du måste ange kakaohalt.")]
    public int CocoaPercentage { get; set; }

    [Required(ErrorMessage = "Du måste ange en vikt.")]
    public int Weight { get; set; }

    [Required(ErrorMessage = "Du måste ange lagersaldo.")]
    public int StockLevel { get; set; }

    [Required(ErrorMessage = "Du måste ange ursprungsland.")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste välja en kategori.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du måste skriva en beskrivning.")]
    public string Description { get; set; } = string.Empty;

    // Denna håller reda på bildens URL i Blob Storage under uppdateringen
    public string? ImageUrl { get; set; }
}