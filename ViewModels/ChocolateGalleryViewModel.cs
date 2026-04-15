namespace TheSnaxers.ViewModels;



public class ChocolateGalleryViewModel

{

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public int CocoaPercentage { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string CountryName { get; set; } = "Okänt";
    
    public string FlagUrl { get; set; } = string.Empty;



    // Generates a dynamic image path based on the product name if no ImageUrl is provided.

    // Includes a fallback to a placeholder image.

    // TODO (Azure/Blob Storage): Once Azure Blob Storage is fully integrated and 

    // all products have a valid 'ImageUrl' saved in the database, this dynamic fallback 

    // logic can be removed. The View should then just use the regular ImageUrl property.

    public string DynamicImageUrl

    {

        get

        {

            if (!string.IsNullOrWhiteSpace(ImageUrl)) return ImageUrl;

            if (string.IsNullOrWhiteSpace(Name)) return "/images/placeholder-choco.png";



            var safeName = Name.ToLower()

                               .Replace(" ", "-")

                               .Replace("å", "a")

                               .Replace("ä", "a")

                               .Replace("ö", "o");



            return $"/images/products/{safeName}.jpg";

        }

    }

}