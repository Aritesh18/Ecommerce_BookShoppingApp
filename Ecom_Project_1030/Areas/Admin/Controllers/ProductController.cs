using Ecom_Project_1030.DataAccess.Repository.iRepository;
using Ecom_Project_1030.Models;
using Ecom_Project_1030.Models.ViewModels;
using Ecom_Project_1030.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ecom_Project_1030.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
        
    {
        private readonly iUnitofWork _unitofwork;
        private readonly IWebHostEnvironment _webHostEnviornment;

        public ProductController(iUnitofWork unitofWork,IWebHostEnvironment webHostEnvironment)

        {
            _unitofwork = unitofWork;
            _webHostEnviornment = webHostEnvironment;
        }
         
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofwork.product.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var productInDb = _unitofwork.product.Get(id);
            if (productInDb == null)
                return Json(new { success = false, message = "Something went wrong while delete data!!!" });
            //Delete File
            var WebRootPath = _webHostEnviornment.WebRootPath;
            var imagePath = Path.Combine(WebRootPath, productInDb.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _unitofwork.product.Remove(productInDb);
            _unitofwork.Save();
            return Json(new { success = true, message = "data deleted successfully!!!" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()              //VM=viewmodel
            {
                Product = new Product(),
                CategoryList = _unitofwork.category.GetAll().Select(cl => new SelectListItem()                 //cl=categorylist
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()

                }),
                CoverTypeList = _unitofwork.covertype.GetAll().Select(ct => new SelectListItem()              //ct=covertypelist
                {
                    Text = ct.Name,
                    Value = ct.Id.ToString()
                })
              };
            if (id == null) return View(productVM);
            productVM.Product = _unitofwork.product.Get(id.GetValueOrDefault());
            return View(productVM);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _webHostEnviornment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    var fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);
                    var Uploads = Path.Combine(webRootPath, @"Images\Products");
                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitofwork.product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }
                    if (productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.Trim('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var FileStream = new FileStream(Path.Combine(Uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(FileStream);
                    }
                    productVM.Product.ImageUrl = @"\Images\Products\" + fileName + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitofwork.product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }
                }
                if (productVM.Product.Id == 0)
                    _unitofwork.product.Add(productVM.Product);
                else
                    _unitofwork.product.Update(productVM.Product);
                _unitofwork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                productVM = new ProductVM()
                {
                    Product = new Product(),
                    CategoryList = _unitofwork.category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()

                    }),
                    CoverTypeList = _unitofwork.covertype.GetAll().Select(ct => new SelectListItem()
                    {
                        Text = ct.Name,
                        Value = ct.Id.ToString()

                    })
                };
                if(productVM.Product.Id!=0)
                {
                    productVM.Product = _unitofwork.product.Get(productVM.Product.Id);
                }
                return View(productVM);

                }
                
            }
        }
    }

