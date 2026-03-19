using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext demo1;

        public PaymentController(ApplicationDbContext demo1)
        {
            this.demo1 = demo1;
        }
        public IActionResult Index()
        {
            var list = demo1.Payments.Include(p => p.Orders).ToList();
            return View(list);
        }
        public IActionResult Create()
        {
            ViewBag.Orders = new SelectList(demo1.Orders, "Id", "Name");
            return View();
        }
        [HttpPost]
        public IActionResult Create(Payment p)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Orders = new SelectList(demo1.Orders, "Id", "Name", p.OrdersId);
                return View(p);
            }
            demo1.Payments.Add(p);
            demo1.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var p = demo1.Payments.Find(id);
            if (p == null) return NotFound();
            ViewBag.Payments = new SelectList(demo1.Orders, "Id", "Name", p.OrdersId);
            return View(p);
        }
        [HttpPost]
        public IActionResult Edit(Payment p)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Orders = new SelectList(demo1.Orders, "Id", "Name", p.OrdersId);
                return View(p);
            }
            demo1.Payments.Update(p);
            demo1.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var p = demo1.Payments.Find(id);
                if(p== null) return NotFound();
                demo1.Payments.Remove(p);
            demo1.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult OnlinePayment(int orderId)
        {
            var order = demo1.Orders.Include(o => o.User).Include(o => o.Tour).FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public IActionResult ConfirmPayment(int orderId, string email)
        {
            var order = demo1.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound();

            order.Status = "Đã thanh toán";
            demo1.SaveChanges();


            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction("MyOrders", "Orders");
        }
    }
}
