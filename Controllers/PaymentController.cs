using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    [AdminOnly]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext db;

        public PaymentController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            var list = db.Payments.Include(p => p.Orders).ToList();
            return View(list);
        }

        public IActionResult Create()
        {
            PopulateOrders();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                PopulateOrders(payment.OrdersId);
                return View(payment);
            }

            db.Payments.Add(payment);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var payment = db.Payments.Find(id);
            if (payment == null)
            {
                return NotFound();
            }

            PopulateOrders(payment.OrdersId);
            return View(payment);
        }

        [HttpPost]
        public IActionResult Edit(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                PopulateOrders(payment.OrdersId);
                return View(payment);
            }

            db.Payments.Update(payment);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var payment = db.Payments.Find(id);
            if (payment == null)
            {
                return NotFound();
            }

            db.Payments.Remove(payment);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult OnlinePayment(int orderId)
        {
            var order = db.Orders.Include(o => o.User).Include(o => o.Tour).FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult ConfirmPayment(int orderId, string email)
        {
            var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Đã thanh toán";
            db.SaveChanges();

            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction("MyOrders", "Orders");
        }

        private void PopulateOrders(int? selectedOrderId = null)
        {
            ViewBag.Orders = new SelectList(db.Orders, "Id", "Id", selectedOrderId);
        }
    }
}
