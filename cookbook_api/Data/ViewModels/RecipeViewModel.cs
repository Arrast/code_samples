using System.ComponentModel.DataAnnotations;
using cookbook_api.Models;

namespace cookbook_api.Data.ViewModels;

public class RecipeViewModel
{
    [MaxLength(50)]
    [Required]
    public string Title { get; set; } = "";
    
    [MaxLength(50)]
    [Required]
    public string Author { get; set; } = "";
    
    [MaxLength(1000)] 
    [Required]
    public string Description { get; set; } = "";

    public List<IngredientEntry> Ingredients { get; init; } = new();
}