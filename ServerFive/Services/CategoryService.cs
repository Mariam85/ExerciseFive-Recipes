using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Text.Json;
using ServerFive.Protos;
using Google.Protobuf;
using Newtonsoft.Json;

namespace ServerFive.Services
{
    public class CategoryService : Category.CategoryBase
    {
        private List<CategoryItem> categories = new();
        private List<RecipeItem> recipes = new();
        public async Task ReadCategories()
        {
            string jsonContent = await File.ReadAllTextAsync("Categories.json");
            if (jsonContent == null) { return; }
            categories = JsonConvert.DeserializeObject<List<CategoryItem>>(jsonContent)!;
        }

        public async Task UpdateCategories()
        {
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = System.IO.Path.Combine(Environment.CurrentDirectory, "Categories.json");
            string sFilePath = Path.GetFullPath(sFile);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(sFilePath, System.Text.Json.JsonSerializer.Serialize(categories, options));
        }

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

        // Listing categories.
        public override async Task<Categories> ListCategories(Empty request, ServerCallContext context)
        {
            await ReadCategories();
            Categories response = new();
            response.SingleCategory.AddRange(categories);
            return response;
        }

        // Adding categories.
        public override async Task<CategoryItem> AddCategories(CategoryItem request, ServerCallContext context)
        {
            await ReadCategories();
            CategoryItem response = request;
            if (categories.Any())
            {
                if (categories.FindIndex(c => c.Name == request.Name) == -1)
                {
                    categories.Add(request);
                    categories.Sort((x, y) => string.Compare(x.Name, y.Name));
                    await UpdateCategories();
                    return response;
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "This category already exists."));
                }
            }
            else
            {
                categories.Add(request);
                await UpdateCategories();
                return response;
            }
        }

        // Remove categories.
        public override async Task<CategoryItem> RemoveCategories(CategoryItem request, ServerCallContext context)
        {
            await ReadCategories();
            CategoryItem response = request;
            if (categories.Any())
            {
                bool isRemoved = categories.Remove(categories.Find(c => c.Name == request.Name));
                if (!isRemoved)
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "This category does not exist."));
                }
                else
                {
                    await UpdateCategories();
                    // Removing from the recipes file.
                    await ReadRecipes();
                    bool foundRecipe = false;
                    foreach (RecipeItem r in recipes)
                    {
                        if (r.Categories[0] == request.Name && r.Categories.Count == 1)
                        {
                            foundRecipe = true;
                            recipes.Remove(r);
                        }
                        else
                        {
                            if (r.Categories.Contains(request.Name))
                            {
                                foundRecipe = true;
                                r.Categories.Remove(request.Name);
                            }
                        }
                    }
                    if (foundRecipe)
                    {
                        UpdateRecipes();
                    }
                    return request;
                }
            }
            else
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "There are no categories."));
            }
        }

        // Edit categories.
        public override async Task<CategoryItem> EditCategories(EditedCategory request, ServerCallContext context)
        {
            if (request == null || request.OldName.Length == 0 || request.NewName.Length == 0)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Empty parameters were given."));
            }
            else if (request.OldName == request.NewName)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "There is no change in the category value."));
            }
            else
            {
                // Renaming category in the categories file.
                await ReadCategories();
                int index = categories.FindIndex(c => c.Name == request.OldName);
                if (index != -1)
                {
                    if (categories.FindIndex(c => c.Name == request.NewName) == -1)
                    {
                        categories[index].Name = request.NewName;
                        categories.Sort((x, y) => string.Compare(x.Name, y.Name));
                        await UpdateCategories();

                        // Renaming the category in the recipes file.
                        await ReadRecipes();
                        List<RecipeItem> beforeRename = recipes.FindAll(r => r.Categories.Contains(request.OldName));
                        if (beforeRename.Count != 0)
                        {
                            foreach (RecipeItem r in beforeRename)
                            {
                                int i = r.Categories.IndexOf(request.OldName);
                                if (i != -1)
                                {
                                    r.Categories[i] = request.NewName;
                                    r.Categories.OrderBy(c => c);
                                }
                            }
                            UpdateRecipes();
                        }
                        CategoryItem response = new();
                        response.Name = request.NewName;
                        response.Id = categories[index].Id;
                        return response;
                    }
                    else
                    {
                        throw new RpcException(new Status(StatusCode.PermissionDenied, "The category name entered already exists."));
                    }
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "The old category name does not exist."));
                }

            }
        }

    }
}

