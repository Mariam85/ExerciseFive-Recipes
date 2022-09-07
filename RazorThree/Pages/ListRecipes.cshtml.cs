using Microsoft.AspNetCore.Mvc;
using RazorThree.Protos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using System.Net.Http;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Hosting.Server;
using System.Numerics;
using System.ComponentModel.DataAnnotations;

namespace RazorThree.Pages
{
    public class ListRecipesModel : PageModel
    {
        [BindProperty]
        public RecipeItem newRecipe { get; set; } = new RecipeItem();
        public List<RecipeItem> recipesList { get; set; } = new List<RecipeItem>();
        public RecipeItem singleRecipe { get; set; } = new RecipeItem();

        public List<CategoryItem> categ { get; set; }
        private readonly Recipe.RecipeClient _recipeServiceClient;
        private readonly Category.CategoryClient _categoryServiceClient;

        public ListRecipesModel(Recipe.RecipeClient recipeServiceClient, Category.CategoryClient categoryServiceClient)
        {
            _recipeServiceClient = recipeServiceClient;
            _categoryServiceClient = categoryServiceClient;
        }

        // Listing recipes.
        public async Task OnGet()
        {
            var recipesGroup = await _recipeServiceClient.ListRecipesAsync(new());
            recipesList = recipesGroup.SingleRecipe.ToList();
            if (recipesList.Count != 0)
            {
                singleRecipe = recipesList[0];
                recipesList.RemoveAt(0);
            }
            var categoriesGroup = await _categoryServiceClient.ListCategoriesAsync(new());
            categ = categoriesGroup.SingleCategory.ToList();
        }

        // Adding a recipe.
        public async Task<IActionResult> OnPostAdd(List<string> data, string recipeIngredients, string recipeInstructions)
        {

            if (newRecipe.Title.Trim().Length == 0 || recipeInstructions == null || recipeIngredients == null || data.Count == 0)
            {
                TempData["confirmation"] = "fail";
                TempData["details"] = "Empty parameters.";
            }
            else
            {
                var ingredientesList = recipeIngredients.Replace("\n", "").Replace("\r", "").Split("• ").ToList();
                var instructionsList = recipeInstructions.Replace("\n", "").Replace("\r", "").Split("• ").ToList();
                ingredientesList.RemoveAt(0);
                instructionsList.RemoveAt(0);
                newRecipe.Ingredients.AddRange(ingredientesList);
                newRecipe.Instructions.AddRange(instructionsList);
                newRecipe.Categories.AddRange(data);
                newRecipe.Id = System.Guid.NewGuid().ToString();
                try
                {
                    var r = await _recipeServiceClient.AddRecipeAsync(newRecipe);
                    TempData["confirmation"] = "success";
                    TempData["details"] = newRecipe.Title + " is added!";
                }
                catch (Exception ex)
                {
                    TempData["confirmation"] = "fail";
                    if (ex.Message.Contains("Empty parameters."))
                    {
                        TempData["details"] = "Empty parameters.";
                    }
                    else
                    {
                        TempData["details"] = "Failed to add recipe.";
                    }
                }
            }
            return RedirectToPage("./ListRecipes");
        }

        // Deleting a recipe.
        public async Task<IActionResult> OnPostDelete(string recipeToDelete)
        {
            if (recipeToDelete == null)
            {
                TempData["details"] = "Failed to delete.";
                TempData["confirmation"] = "fail";
            }
            else
            {
                try
                {
                    var recipesGroup = await _recipeServiceClient.ListRecipesAsync(new());
                    recipesList = recipesGroup.SingleRecipe.ToList();
                    var recipeToRemove = recipesList.Find(r => r.Id == recipeToDelete);
                    var r = await _recipeServiceClient.RemoveRecipeAsync(recipeToRemove);
                    TempData["details"] = recipeToRemove.Title + " is deleted!";
                    TempData["confirmation"] = "success";
                }
                catch
                {
                    TempData["details"] = "Failed to delete.";
                    TempData["confirmation"] = "fail";
                }
            }
            return RedirectToPage("./ListRecipes");
        }

        // Editing a recipe.
        public async Task<IActionResult> OnPostUpdate(string id, string instructions, List<string> categories, string ingredients, string newTitle)
        {
            var ingredientesList = ingredients.Replace("\n", "").Replace("\r", "").Split("• ").ToList();
            ingredientesList.RemoveAt(0);
            var instructionsList = instructions.Replace("\n", "").Replace("\r", "").Split("• ").ToList();
            instructionsList.RemoveAt(0);
            RecipeItem recipeToEdit = new();
            recipeToEdit.Ingredients.AddRange(ingredientesList);
            recipeToEdit.Instructions.AddRange(instructionsList);
            recipeToEdit.Id = id;
            recipeToEdit.Title = newTitle;
            recipeToEdit.Categories.AddRange(categories);
            try
            {
                var r = await _recipeServiceClient.EditRecipeAsync(recipeToEdit);
                TempData["confirmation"] = "success";
                TempData["details"] = "recipe is edited!";
            }
            catch
            {
                TempData["confirmation"] = "fail";
                TempData["details"] = "Failed to edit recipe";
            }
            return RedirectToPage("./ListRecipes");
        }
    }
}
