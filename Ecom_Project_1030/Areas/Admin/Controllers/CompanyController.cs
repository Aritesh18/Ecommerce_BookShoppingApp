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
    [Authorize(Roles = SD.Role_Admin+","+SD.Role_Employee)]

    public class CompanyController : Controller
    {
        private readonly iUnitofWork _unitofwork;
        public CompanyController(iUnitofWork unitofWork)
        {
            _unitofwork = unitofWork;
        }
        public IActionResult Index()
    


        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofwork.company.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companyInDb = _unitofwork.company.Get(id);
            if (companyInDb == null)
                return Json(new { success = false, message = "something went wrong while deleted data!!!" });
            _unitofwork.company.Remove(companyInDb);
            _unitofwork.Save();
            return Json(new { success = true, message = "data successfully deleted" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null) return View(company);
            company = _unitofwork.company.Get(id.GetValueOrDefault());
            if (company == null) return NotFound();
            return View(company);
        }
        [HttpPost]                      //save
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (company == null) return NotFound();
            if (!ModelState.IsValid) return View();
            if (company.Id == 0)
                _unitofwork.company.Add(company);
            else
                _unitofwork.company.Update(company);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
