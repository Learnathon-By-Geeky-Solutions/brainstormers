using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Controllers
{

    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _db.Categories; 
            return View(objCategoryList);
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if(obj.Name==obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("CustomError", "The DIsplayOrder cannot exactly match the Name. ");
            }
            if(ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Category created successfully";

                return RedirectToAction("Index");
            }
            return View(obj);
        }


        private async Task<Category?> GetCategoryById(int? id)
        {
            if (id == null || id <= 0) return null;
            return await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            var category = await GetCategoryById(id);
            if (category == null) return NotFound("Category not found");

            return View(category);  // Returns the default "Edit" view
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("CustomError", "The DIsplayOrder cannot exactly match the Name. ");
            }
            if (ModelState.IsValid)
            {
                _db.Categories.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Category updated successfully";

                return RedirectToAction("Index");
            }
            return View(obj);
        }


        

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            var category = await GetCategoryById(id);
            if (category == null) return NotFound("Category not found");

            return View("DeleteConfirmation", category);  // Returns a separate confirmation view
        }


        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.Categories.Find(id);
            if (obj == null) return NotFound();
            _db.Categories.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Category deleted successfully";

            return RedirectToAction("Index");
        }
    }
}
