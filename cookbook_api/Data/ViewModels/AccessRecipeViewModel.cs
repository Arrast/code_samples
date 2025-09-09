using System.ComponentModel.DataAnnotations;
using cookbook_api.Models;

namespace cookbook_api.Data.ViewModels;

public class AccessRecipeViewModel
{
    [MaxLength(50)]
    public string Title { get; set; } = "";

    [MaxLength(50)] public string Author { get; set; } = "";

    [MaxLength(1000)] public string Description { get; set; } = "";

    public List<IngredientEntry> Ingredients { get; init; } = new();
}