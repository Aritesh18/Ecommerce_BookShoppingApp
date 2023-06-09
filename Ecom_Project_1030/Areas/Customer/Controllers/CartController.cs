using Ecom_Project_1030.DataAccess.Repository.iRepository;
using Ecom_Project_1030.Models;
using Ecom_Project_1030.Models.ViewModels;
using Ecom_Project_1030.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Ecom_Project_1030.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class CartController : Controller
    {
        private readonly iUnitofWork _unitofwork;
        private static bool isEmailConfirm = false;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        public CartController(iUnitofWork unitofWork,IEmailSender emailSender,UserManager<IdentityUser>userManager)
        {
            _unitofwork = unitofWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim==null)
            {
                shoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<ShoppingCart>()
                };
                return View(shoppingCartVM);
            }
            //***
            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofwork.shoppingCart.GetAll(sp => sp.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            shoppingCartVM.OrderHeader.OrderTotal = 0;
            shoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.FirstorDefault(u => u.Id == claim.Value, includeProperties: "Company");
            foreach(var list in shoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (list.Count * list.Price);
                if (list.Product.Description.Length>100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
                string price = SD.NumberToWords(Convert.ToInt32(shoppingCartVM.OrderHeader.OrderTotal.ToString()));
                shoppingCartVM.OrderHeader.TotalInWords = price;
            }
            if (!isEmailConfirm)
            {
                ViewBag.EmailMessage = "Email has been sent kindly verify your email!";
                ViewBag.EmailCSS = "text-succes";
                isEmailConfirm = false;
            }
            else
            {
                ViewBag.EmailMessage = "Email must be confirm for authorize customer !";
                ViewBag.EmailCSS = "text-danger";

            }

                return View(shoppingCartVM);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult>IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitofwork.ApplicationUser.FirstorDefault(u => u.Id == claims.Value);
            if(user==null)
            {
                ModelState.AddModelError(string.Empty, "Email is Empty");
            }
            else
            {
                //email
                
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                isEmailConfirm = true;
            }
            return RedirectToAction(nameof(Index));
        }
        
             
        public IActionResult plus(int id)
        {
            var cart = _unitofwork.shoppingCart.FirstorDefault(sc => sc.Id == id);
            cart.Count += 1;
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id)
        {
            var cart = _unitofwork.shoppingCart.FirstorDefault(sc => sc.Id == id);
            if(cart.Count==1)
            {
                cart.Count = 1;

            }
            else
            {
                cart.Count -= 1;

            }
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult delete(int id)
        {
            var cart = _unitofwork.shoppingCart.FirstorDefault(sc => sc.Id == id);
            _unitofwork.shoppingCart.Remove(cart);
            _unitofwork.Save();
            //Session
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim!=null)
            {
                var count = _unitofwork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofwork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            shoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.FirstorDefault(u => u.Id == claim.Value, includeProperties: "Company");
            foreach (var list in shoppingCartVM.ListCart)
                 {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                list.Product.Description = SD.ConvertToRawHTML(list.Product.Description);
                string price = SD.NumberToWords(Convert.ToInt32(shoppingCartVM.OrderHeader.OrderTotal.ToString()));
                shoppingCartVM.OrderHeader.TotalInWords = price;
            
            }
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(shoppingCartVM);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string StripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //var user = _unitofwork.ApplicationUser.FirstorDefault(u => u.Id == claim.Value);
            //if(user==null)
            //{
            //    ModelState.AddModelError(string.Empty, "Email is empty!!");
            //}
            //else
            //{

            //}
            shoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.FirstorDefault(u => u.Id == claim.Value, includeProperties: "Company");
            shoppingCartVM.ListCart = _unitofwork.shoppingCart.GetAll(sc => sc.ApplicationUserId == claim.Value, includeProperties: "Product");
            shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            _unitofwork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitofwork.Save();
            foreach(var list in shoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = list.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = list.Price,
                    Count = list.Count
                };
                shoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                _unitofwork.OrderDetails.Add(orderDetails);
                _unitofwork.Save();
            }
            _unitofwork.shoppingCart.RemoveRange(shoppingCartVM.ListCart);
            _unitofwork.Save();
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
            #region Stripe
            if(StripeToken==null)
            {
                shoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                //payment process
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(shoppingCartVM.OrderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "OrderId:" + shoppingCartVM.OrderHeader.Id,
                    Source = StripeToken
                };
                //payment
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                    shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                else
                    shoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if(charge.Status.ToLower()=="succeeded")
                {
                    shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                }
                _unitofwork.Save();
            }

            #endregion
            return RedirectToAction("OrderConfirmation", "Cart", new { id = shoppingCartVM.OrderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
