using Microsoft.AspNetCore.Mvc;
using RazorThree.Protos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using System.Net.Http;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Hosting.Server;

namespace RazorThree.Pages
{
    public class ListRecipesModel : PageModel
    {
        [BindProperty]
        public List<RecipeItem> recipesList { get; set; } = new List<RecipeItem>();
        public RecipeItem singleRecipe { get; set; } = new();
        public RecipeItem newRecipe { get; set; } 
        public List<CategoryItem> categ { get; set; }
        private readonly Recipe.RecipeClient _recipeServiceClient;
        private readonly Category.CategoryClient _categoryServiceClient;

        public ListRecipesModel(Recipe.RecipeClient recipeServiceClient, Category.CategoryClient categoryServiceClient)
        {
            _recipeServiceClient = recipeServiceClient;
            _categoryServiceClient = categoryServiceClient;
        }

        public async Task OnGet()
        {
            var recipesGroup = await _recipeServiceClient.ListRecipesAsync(new());
            recipesList = recipesGroup.SingleRecipe.ToList();
            singleRecipe = recipesList[0];
            recipesList.RemoveAt(0);
            var categoriesGroup = await _categoryServiceClient.ListCategoriesAsync(new());
            categ = categoriesGroup.SingleCategory.ToList();
        }
    }
}
