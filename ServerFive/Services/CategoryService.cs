using Grpc.Core;
using ServerFive;
using Google.Protobuf;
using ServerFive.Protos;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;

namespace ServerFive.Services
{
    public class CategoryService : Protos.CategoryService.CategoryServiceBase
    {
        private List<Category> _categories = new();
        public override async Task<Categories> ListCategories(Empty request, ServerCallContext context)
        {
            await ReadCategories();
            Categories response = new();
            response.CategoryItem.AddRange(_categories);
            return response;
        }

        public override async Task<Category> AddCategories(Category request, ServerCallContext context)
        {
            await ReadCategories();
            Category response = new();
            if (_categories.Any())
            {
                if (_categories.FindIndex(c => c.Name == request.Name) == -1)
                {
                    _categories.Add(request);
                    _categories.Sort((x, y) => string.Compare(x.Name, y.Name));
                    await UpdateCategories();
                    response = request;
                }
            }
            return response;
        }

        public override async Task<Category> RemoveCategories(Category request, ServerCallContext context)
        {
            await ReadCategories();
            Category response = new();
            if (_categories.Any())
            {
                bool isRemoved = _categories.Remove(_categories.Find(c => c.Name == request.Name));
                if (!isRemoved)
                {
                    // Category does not exist.
                }
                else
                {
                    await UpdateCategories();
                    // Removing from the recipes file (TODO).
                    await ReadRecipes();
                    bool foundRecipe = false;
                    response = request;
                }
            }
            return response;
        }

        public override async Task<Category> EditCategories(EditedCategory request, ServerCallContext context)
        {
            Category response = new();
            if (request.OldName == request.NewName)
            {
                return response;
            }

            // Renaming category in the categories file.
            await ReadCategories();
            int index = _categories.FindIndex(c => c.Name == request.OldName);
            if (index != -1)
            {
                if (_categories.FindIndex(c => c.Name == request.NewName) == -1)
                {
                    _categories[index].Name = request.NewName;
                    _categories.Sort((x, y) => string.Compare(x.Name, y.Name));
                    await UpdateCategories();
                }

                // Renaming the category in the recipes file (TODO).
                response.Name = request.NewName;
                return response;
            }
            return response;
        }
        
        public async Task ReadCategories()
        {
            string jsonContent = await File.ReadAllTextAsync("Categories.json");
            if (jsonContent == null) { return; }
            _categories = JsonSerializer.Deserialize<List<Category>>(jsonContent);
        }

        public async Task UpdateCategories()
        {
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = System.IO.Path.Combine(Environment.CurrentDirectory, "Categories.json");
            string sFilePath = Path.GetFullPath(sFile);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(sFilePath, System.Text.Json.JsonSerializer.Serialize(_categories));
        }

        public async Task ReadRecipes()
        {
            string jsonContent = await File.ReadAllTextAsync("Recipes.json");
            if (jsonContent == null) { return; }
            Console.WriteLine(jsonContent);
            //recipes = JsonSerializer.Deserialize<List<>>(jsonContent);
        }

        public async Task UpdateRecipes()
        {
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = System.IO.Path.Combine(Environment.CurrentDirectory, "Recipes.json");
            string sFilePath = Path.GetFullPath(sFile);
            var options = new JsonSerializerOptions { WriteIndented = true };
            //File.WriteAllText(sFilePath, System.Text.Json.JsonSerializer.Serialize(recipes));
        }
    }
}
