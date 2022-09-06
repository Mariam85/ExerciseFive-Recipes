using Microsoft.AspNetCore.Mvc;
using RazorThree.Protos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using System.Net.Http;
using System.Threading.Tasks.Dataflow;

namespace RecipeRazor.Pages
{
    public class CategoriesPageModel : PageModel
    {
        [BindProperty]
        public List<CategoryItem> categories { get; set; } = new();
        public List<string> msg { get; set; } = new();
        private readonly Recipe.RecipeClient _recipeServiceClient;
        private readonly Category.CategoryClient _categoryServiceClient;

        public CategoriesPageModel(Recipe.RecipeClient recipeServiceClient, Category.CategoryClient categoryServiceClient)
        {
            _recipeServiceClient = recipeServiceClient;
            _categoryServiceClient = categoryServiceClient;
        }

        public async Task<IActionResult> OnGet(List<string> messages)
        {
            var categoriesGroup = await _categoryServiceClient.ListCategoriesAsync(new());
            categories = categoriesGroup.SingleCategory.ToList();
            msg = messages;
            return Page();
        }

        public async Task<IActionResult> OnPostDelete(string deletedValue)
        {
            int index = Int32.Parse(deletedValue);
            var categoriesGroup = await _categoryServiceClient.ListCategoriesAsync(new());
            categories = categoriesGroup.SingleCategory.ToList();
            var c = await _categoryServiceClient.RemoveCategoriesAsync(categories[index]);
            System.Threading.Thread.Sleep(1500);
            return RedirectToPage("./CategoriesPage");
        }

        public async Task<IActionResult> OnPostUpdate(string newCategory, string oldValue)
        {
            var categoriesGroup = await _categoryServiceClient.ListCategoriesAsync(new());
            categories = categoriesGroup.SingleCategory.ToList();
            EditedCategory editedCategory = new();
            editedCategory.OldName = categories[Int32.Parse(oldValue)].Name;
            Console.WriteLine(oldValue);
            editedCategory.NewName = newCategory;
            try
            {
                var r = await _categoryServiceClient.EditCategoriesAsync(editedCategory);
                msg.Add("no error1");
            }
            catch
            {
                msg.Add("error1");
            }
            return RedirectToPage("./CategoriesPage", new { messages = msg });
        }

        public async Task<IActionResult> OnPostAdd(string addCategoryName)
        {
            CategoryItem categoryItem = new();
            categoryItem.Name = addCategoryName;
            categoryItem.Id = System.Guid.NewGuid().ToString();
            try
            {
                var r = await _categoryServiceClient.AddCategoriesAsync(categoryItem);
                msg.Add("no error2");
            }
            catch
            {
                msg.Add("error2");
            }
            return RedirectToPage("./CategoriesPage", new { messages = msg });
        }
    }
}