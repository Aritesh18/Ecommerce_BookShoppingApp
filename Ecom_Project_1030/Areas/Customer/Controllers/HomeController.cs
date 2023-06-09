using Ecom_Project_1030.DataAccess.Repository.iRepository;
using Ecom_Project_1030.Models;
using Ecom_Project_1030.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
//using Ecom_Project_1030.Models.ViewModels;
//using Ecom_Project_1030.Models.ViewModels.ErrorViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecom_Project_1030.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly iUnitofWork _unitofwork;

        public HomeController(ILogger<HomeController> logger,iUnitofWork unitofWork)
        {
            _logger = logger;
            _unitofwork = unitofWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _unitofwork.shoppingCart.GetAll(sp => sp.ApplicationUserId == claim.Value).ToList().Count;
                //HttpContext.Session.SetInt32("", count);      //isko cooment bna do SD Class ma session add krna
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            var productList = _unitofwork.product.GetAll(includeProperties: "category,coverType");
            return View(productList);
        }
       
        public IActionResult Details(int id)
        {
            //Session
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim!=null)
            {
                var count = _unitofwork.shoppingCart.GetAll(sp => sp.ApplicationUserId == claim.Value).ToList().Count;
            }
            var productInDb = _unitofwork.product.FirstorDefault(p => p.Id == id, includeProperties: "category,coverType");
            if (productInDb == null) return NotFound();
            var shoppingCart = new ShoppingCart()
            {
                Product = productInDb,
                ProductId = productInDb.Id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if(ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = claim.Value;
                var shoppingCartFromDb = _unitofwork.shoppingCart.FirstorDefault(sp => sp.ApplicationUserId == claim.Value && sp.ProductId == shoppingCart.ProductId);
                if(shoppingCartFromDb==null)
                {
                    //Add
                    _unitofwork.shoppingCart.Add(shoppingCart);
                }
                else
                {
                    //Update
                    shoppingCartFromDb.Count += shoppingCart.Count;
                }
                _unitofwork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productInDb = _unitofwork.product.FirstorDefault(p => p.Id == shoppingCart.ProductId, includeProperties: "category,coverType");
                if (productInDb == null) return NotFound();
                var shoppingCartEdit = new ShoppingCart()
                {
                    Product = productInDb,
                    ProductId = productInDb.Id
                };
                return View(shoppingCart);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
