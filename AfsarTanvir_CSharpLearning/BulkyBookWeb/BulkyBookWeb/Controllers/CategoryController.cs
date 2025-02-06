using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Controllers
{
    public class CategoryController(ApplicationDbContext db) : Controller
    {
        private readonly ApplicationDbContext _db = db;
        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = [.. _db.Categories];
            return View(objCategoryList);
        }
        // Get Action Result for Create
        public IActionResult Create()
        {
            return View();
        }
        // Post Action Result for Create Object in Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if(category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order can not exactly match the name.");
            }
            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["success"] = "Category Created Successfully.";
                return RedirectToAction("Index");
            }
            return View(category);
        }


        // Get Action Result for Edit
        public IActionResult Edit(int? id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Categories.Find(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        // Post Action Result for Editing Objects in Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order can not exactly match the name.");
            }
            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                _db.Entry(category).Property(c => c.CreateDateTime).IsModified = false;
                _db.SaveChanges();
                TempData["success"] = "Category Updated Successfully.";
                return RedirectToAction("Index");
            }
            return View(category);
        }


        // Get Action Result for Delete
        public IActionResult Delete(int? id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            return Edit(id);
        }
        // Post Action Result for Deleteing Objects in Database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Categories.Find(id);
            if(categoryFromDb == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(categoryFromDb);
            _db.SaveChanges();
            TempData["success"] = "Category Removed Successfully.";
            return RedirectToAction("Index");
        }
    }
}
