using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Text.Json;
using GrpcService1.Protos;
using Google.Protobuf;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace GrpcService1.Services
{
    public class RecipeService : Recipe.RecipeBase
    {
        private List<RecipeItem> recipes = new();
        public async Task ReadRecipes()
        {
            string jsonContent = await File.ReadAllTextAsync("Recipes.json");
            if (jsonContent == null) { return; }
            recipes = JsonConvert.DeserializeObject<List<RecipeItem>>(jsonContent)!;
        }

        public async Task UpdateRecipes()
        {
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = System.IO.Path.Combine(Environment.CurrentDirectory, "Recipes.json");
            string sFilePath = Path.GetFullPath(sFile);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(sFilePath, System.Text.Json.JsonSerializer.Serialize(recipes, options));
        }

        // Listing recipes.
        public async override Task<RecipesList> ListRecipes(Empty request, ServerCallContext context)
        {
            await ReadRecipes();
            RecipesList response = new();
            response.SingleRecipe.AddRange(recipes);
            return response;
        }

        // Adding a recipe.
        public async override Task<RecipeItem> AddRecipe(RecipeItem request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Empty parameters."));
            }
            await ReadRecipes();
            recipes.Add(request);
            UpdateRecipes();
            return request;
        }

        // Deleting a recipe.
        public async override Task<RecipeItem> RemoveRecipe(RecipeItem request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Empty parameters."));
            }
            else
            {
                await ReadRecipes();
                RecipeItem response = request;
                bool isRemoved = recipes.Remove(recipes.Find(r => r.Id == request.Id));
                if (!isRemoved)
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "This recipe does not exist."));
                }
                else
                {
                    UpdateRecipes();
                    return response;
                }
            }
        }

        // Editing a recipe.
        public async override Task<RecipeItem> EditRecipe(RecipeItem request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Empty parameters."));
            }
            else
            {
                await ReadRecipes();
                RecipeItem response = request;
                int index = recipes.FindIndex(r => r.Id == request.Id);
                if (index != -1)
                {
                    recipes[index] = request;
                    recipes[index].Categories.OrderBy(c => c);
                    UpdateRecipes();
                    return response;
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "This recipe does not exist."));
                }
            }
        }
    }
}

