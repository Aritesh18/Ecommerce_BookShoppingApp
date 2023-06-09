using Ecom_Project_1030.DataAccess.Repository;
using Ecom_Project_1030.DataAccess.Repository.iRepository;
using Ecom_Project_1030.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecom_Project_1030.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderStatusController : Controller
    {
        private readonly iUnitofWork _unitofWork;
        public OrderStatusController(iUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult AllOrdersIndex()
        {
            return View();
        }
        #region AllOrders
        [HttpGet]
        public IActionResult AllOrders()
        {
            return Json(new { data = _unitofWork.OrderHeader.GetAll() });
        }
        [HttpPost]
        public IActionResult AllOrders(DateTime fromDate, DateTime toDate)
        {
            var DateData = _unitofWork.OrderHeader.GetAll().Where(da => da.OrderDate >= fromDate && da.OrderDate <= toDate);
            return Json(new { data = DateData });
        }
        #endregion
        public IActionResult PendingOrdersIndex()
        {
            return View();
        }

        #region PendingOrders
        [HttpGet]
        public IActionResult PendingOrders()
        {
            var pendingOrders = _unitofWork.OrderHeader.GetAll(p => p.OrderStatus == SD.OrderStatusPending);
            return Json(new { data = pendingOrders });
        }
        #endregion
        public IActionResult ApprovedOrderIndex()
        {
            return View();
        }

        #region ApprovedOrders
        [HttpGet]
        public IActionResult ApprovedOrders()
        {
            var approvedOrders = _unitofWork.OrderHeader.GetAll(p => p.OrderStatus == SD.OrderStatusApproved);
            return Json(new { data = approvedOrders });
        }
        #endregion

        public IActionResult Details(int id)
        {
            var orderHeaderId = _unitofWork.OrderHeader.FirstorDefault(o => o.Id == id);
            var getId = orderHeaderId.Id;
            var orderDetails = _unitofWork.OrderDetails.GetAll(o => o.OrderHeaderId == getId, includeProperties: "Product,OrderHeader").FirstOrDefault();
            return View(orderDetails);
        }

    }
}
