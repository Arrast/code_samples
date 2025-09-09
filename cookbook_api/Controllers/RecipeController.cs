using cookbook_api.Data;
using cookbook_api.Data.ViewModels;
using cookbook_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cookbook_api.Controllers;

[ApiController]
[Route("[controller]")]
public class RecipeController : ControllerBase
{
    private AppDbContext _dbContext;

    public RecipeController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET
    [HttpGet("get_all_recipes")]
    public async Task<IActionResult> GetAllRecipes()
    {
        var recipes = await _dbContext.Recipes.ToListAsync();
        if (recipes.Count == 0)
        {
            return BadRequest("There are no available recipes right now.");
        }

        return Ok(recipes);
    }

    [HttpGet("get_recipe")]
    public async Task<IActionResult> GetRecipeById([FromQuery] int recipeId)
    {
        var recipe = await _dbContext.Recipes.FirstOrDefaultAsync(x => x.Id == recipeId);
        if (recipe == null)
        {
            return NoContent();
        }

        return Ok(recipe);
    }

    [HttpPut("update_recipe")]
    public async Task<IActionResult> UpdateRecipe([FromQuery] int recipeId, [FromBody] AccessRecipeViewModel viewModel)
    {
        // Check if the model is valid.
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var recipe = await _dbContext.Recipes.FirstOrDefaultAsync(x => x.Id == recipeId);
        if (recipe == null)
        {
            return NoContent();
        }

        // I only want to modify the values that have actual values.
        // I don't want to allow clearing fields (Since they are all required).
        if (!string.IsNullOrEmpty(viewModel.Author))
        {
            recipe.Author = viewModel.Author;
        }

        if (!string.IsNullOrEmpty(viewModel.Title))
        {
            recipe.Title = viewModel.Title;
        }

        if (!string.IsNullOrEmpty(viewModel.Description))
        {
            recipe.Description = viewModel.Description;
        }

        // Now we add the ingredients.
        if (viewModel.Ingredients is { Count: > 0 })
        {
            recipe.Ingredients.Clear();
            recipe.Ingredients.AddRange(viewModel.Ingredients);
        }

        // Now we save.
        _dbContext.Recipes.Update(recipe);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("delete_recipe")]
    public async Task<IActionResult> DeleteRecipe([FromQuery] int recipeId)
    {
        var recipe = await _dbContext.Recipes.FirstOrDefaultAsync(x => x.Id == recipeId);
        if (recipe == null)
        {
            return NoContent();
        }

        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("add_recipe")]
    public async Task<IActionResult> AddRecipe([FromBody] RecipeViewModel viewModel)
    {
        // Check if the model is valid.
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        // Create the recipe.
        var recipe = new Recipe
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            Author = viewModel.Author,
            Ingredients = new List<IngredientEntry>(),
            Created = DateTime.UtcNow
        };

        // Add the Ingredients.
        recipe.Ingredients.AddRange(viewModel.Ingredients);

        // Save to the database.
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}