using Dapper;
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
    [Authorize(Roles = SD.Role_Admin)]

    public class CoverTypeController : Controller
    {
        private readonly iUnitofWork _unitofwork;
        public CoverTypeController(iUnitofWork unitofWork)
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
            return Json(new { Data = _unitofwork.SP_CALL.List<CoverType>(SD.Proc_GetCoverTypes) });
            //var categoryList = _unitofwork.covertype.GetAll();
            //return Json(new {Data=_unitofwork.covertype.GetAll()});
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            //  var coverTypeInDb = _unitofwork.covertype.Get(id);
            var param = new DynamicParameters();
            param.Add("@id", id);
            var coverTypeInDb = _unitofwork.SP_CALL.OneRecord<CoverType>(SD.Proc_GetCoverType, param);
            if (coverTypeInDb == null)
                return Json(new { success = false, message = "something went wrong while delete data!!!" });
            _unitofwork.SP_CALL.Execute(SD.Proc_DeleteCoverTypes, param);
          //  _unitofwork.covertype.Remove(coverTypeInDb);
          //  _unitofwork.Save();
            return Json(new { success = true, message = "Data Deleted Successfully!!!" });
        }
        #endregion
        public IActionResult Upsert (int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null) return View(coverType);
            //procedure method used
            var param=new DynamicParameters();
            param.Add("@id", id.GetValueOrDefault());
            coverType = _unitofwork.SP_CALL.OneRecord<CoverType>(SD.Proc_GetCoverType, param);
            // coverType = _unitofwork.covertype.Get(id.GetValueOrDefault());
            if (coverType == null) return NotFound();
                       return View(coverType);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null) return NotFound();
            if (!ModelState.IsValid) return View();
            var param = new DynamicParameters();
            param.Add("@name", coverType.Name);
            if (coverType.Id == 0)
                // _unitofwork.covertype.Add(coverType);
                _unitofwork.SP_CALL.Execute(SD.Proc_CreateCoverTypes, param);
            else
            {
                param.Add("@id", coverType.Id);
                _unitofwork.SP_CALL.Execute(SD.Proc_UpdateCoverTypes, param);
            }
              //  _unitofwork.covertype.Update(coverType);
                //_unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
