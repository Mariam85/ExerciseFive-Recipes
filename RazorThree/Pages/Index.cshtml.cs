using Microsoft.AspNetCore.Mvc;
using RazorThree.Protos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;

namespace RazorThree.Pages
{
    public class IndexModel : PageModel
    {
       
        private readonly Recipe.RecipeClient _recipeServiceClient;
        public IndexModel(Recipe.RecipeClient recipeServiceClient)
        {
            _recipeServiceClient = recipeServiceClient;
        }

        public void  OnGet()
        {
            var channel = GrpcChannel.ForAddress("https://localhost:7051");
            var client = new Recipe.RecipeClient(channel);
            RecipeItem newR = new();
            //var r = await client.AddRe
            newR.Title = "Vegan NEEEWWWW icecream";
            newR.Ingredients.Add("60ml rapeseed oil");
            newR.Ingredients.Add("done");
            newR.Instructions.Add("Combine");
            newR.Instructions.Add("doneinst");
            newR.Categories.Add("Appetizer");
            newR.Id = "291e236a-b6ad-4ae9-a37e-434383e16e8c";
            //var r=await _recipeServiceClient.ListRecipesAsync(new());
           // return RedirectToPage("./Index");
        }

    }
}
