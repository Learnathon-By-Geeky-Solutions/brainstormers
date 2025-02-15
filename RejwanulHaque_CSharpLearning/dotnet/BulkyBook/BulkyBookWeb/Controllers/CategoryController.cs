﻿using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;

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


        //GET
        public IActionResult Edit(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == null || id == 0) return NotFound();
            var categoryFromDb = _db.Categories.Find(id);

            if(categoryFromDb == null)
            { 
                return NotFound();
            }
            return View(categoryFromDb);
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


        //GET
        public IActionResult Delete(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Edit(id);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var obj = _db.Categories.Find(id);
            if (obj == null) return NotFound();
            _db.Categories.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Category deleted successfully";

            return RedirectToAction("Index");
        }
    }
}
