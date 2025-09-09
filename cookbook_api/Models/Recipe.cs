using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace cookbook_api.Models;

[Owned]
public class IngredientEntry
{    
    [MaxLength(50)]
    [Required]
    public string Name { get; set; } = "";
    
    [Required]
    public double Amount { get; set; }
    
    [MaxLength(10)]
    [Required]
    public string AmountUnit { get; set; } = "";
    
    // Eventually, add another table with Ingredients so we can link them if I want to implement things like
    // "Find recipes with X, Y and Z ingredients"
}

public class Recipe
{
    public int Id { get; set; }
    
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
    
    public DateTime Created { get; set; }
}