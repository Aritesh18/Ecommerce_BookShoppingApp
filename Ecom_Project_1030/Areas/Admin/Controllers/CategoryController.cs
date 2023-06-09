using Ecom_Project_1030.DataAccess.Repository.iRepository;
using Ecom_Project_1030.Models;
using Ecom_Project_1030.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecom_Project_1030.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        
        private readonly iUnitofWork _unitofwork;
        public CategoryController(iUnitofWork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var categoryList = _unitofwork.category.GetAll();
            return Json(new { Data = categoryList });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var categoryFromDb = _unitofwork.category.Get(id);
            if (categoryFromDb == null)
                return Json(new { Success = false, message = "Something went wrong while delete data!!!" });
            _unitofwork.category.Remove(categoryFromDb);
            _unitofwork.Save();
            return Json(new { Success = true, message = "data deleted successfully!!! " });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            //Create
            Category category = new Category();
            if (id == null) return View(category);
            //Edit
            category = _unitofwork.category.Get(id.GetValueOrDefault());
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (category == null) return NotFound();
            if (!ModelState.IsValid) return View(category);
            if (category.Id == 0)
                _unitofwork.category.Add(category);
            else
                _unitofwork.category.Update(category);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));

        }
    }
}
