using Microsoft.AspNetCore.Mvc;
using RazorThree.Protos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using System.Net.Http;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace RecipeRazor.Pages
{
    public class CategoriesPageModel : PageModel
    {
        [BindProperty]
        public List<CategoryItem> categories { get; set; } = new();
        private readonly Recipe.RecipeClient _recipeServiceClient;
        private readonly Category.CategoryClient _categoryServiceClient;

        public CategoriesPageModel(Recipe.RecipeClient recipeServiceClient, Category.CategoryClient categoryServiceClient)
        {
            _recipeServiceClient = recipeServiceClient;
            _categoryServiceClient = categoryServiceClient;
        }

        // Listing categories.
        public async Task<IActionResult> OnGet()
        {
            var categoriesGroup = await _categoryServiceClient.ListCategoriesAsync(new());
            categories = categoriesGroup.SingleCategory.ToList();
            return Page();
        }

        // Deleting a category.
        public async Task<IActionResult> OnPostDelete(string categoryToDelete)
        {
            if (categoryToDelete == null)
            {
                TempData["details"] = "Failed to delete.";
                TempData["confirmation"] = "fail";
            }
            else
            {
                try
                {
                    var categoriesGroup = await _categoryServiceClient.ListCategoriesAsync(new());
                    categories = categoriesGroup.SingleCategory.ToList();
                    var index = categories.FindIndex(c => c.Name == categoryToDelete);
                    var c = await _categoryServiceClient.RemoveCategoriesAsync(categories[index]);
                    TempData["details"] = categoryToDelete + " is deleted!";
                    TempData["confirmation"] = "success";

                }
                catch
                {
                    TempData["details"] = "Failed to delete.";
                    TempData["confirmation"] = "fail";
                }
            }
            return RedirectToPage("./CategoriesPage");
        }

        // Renaming a category.
        public async Task<IActionResult> OnPostUpdate(string newCategory, string oldValue)
        {
            if (newCategory == null || oldValue == null)
            {
                TempData["details"] = "Empty parameters were given.";
                TempData["confirmation"] = "fail";
            }
            else
            {
                var categoriesGroup = await _categoryServiceClient.ListCategoriesAsync(new());
                categories = categoriesGroup.SingleCategory.ToList();
                EditedCategory editedCategory = new();
                editedCategory.OldName = categories[Int32.Parse(oldValue)].Name;
                editedCategory.NewName = newCategory;
                try
                {
                    var r = await _categoryServiceClient.EditCategoriesAsync(editedCategory);
                    TempData["confirmation"] = "success";
                    TempData["details"] = editedCategory.OldName + " is renamed to " + newCategory + "!";
                }
                catch (Exception ex)
                {
                    TempData["confirmation"] = "fail";
                    if (ex.Message.Contains("The category name entered already exists."))
                    {
                        TempData["details"] = "The category name entered already exists";

                    }
                    else if (ex.Message.Contains("There is no change in the category value."))
                    {
                        TempData["details"] = "There is no change in the category value.";
                    }
                    else if (ex.Message.Contains("Empty parameters were given."))
                    {
                        TempData["details"] = "Empty parameters were given.";
                    }
                    else
                    {
                        TempData["details"] = "Failed to edit.";
                    }
                }
            }
            return RedirectToPage("./CategoriesPage");
        }

        // Adding a category.
        public async Task<IActionResult> OnPostAdd(string addCategoryName)
        {
            if (addCategoryName == null)
            {
                TempData["details"] = "Empty parameters were given.";
                TempData["confirmation"] = "fail";
            }
            else
            {
                CategoryItem categoryItem = new();
                categoryItem.Name = addCategoryName;
                categoryItem.Id = System.Guid.NewGuid().ToString();
                try
                {
                    var r = await _categoryServiceClient.AddCategoriesAsync(categoryItem);
                    TempData["confirmation"] = "success";
                    TempData["details"] = addCategoryName + " is added!";
                }
                catch (Exception ex)
                {
                    TempData["confirmation"] = "fail";
                    if (ex.Message.Contains("This category already exists."))
                    {
                        TempData["details"] = "The category name entered already exists";
                    }
                    else
                    {
                        TempData["details"] = "Failed to add.";
                    }
                }
            }
            return RedirectToPage("./CategoriesPage");
        }
    }
}