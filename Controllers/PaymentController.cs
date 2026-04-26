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
        private readonly IPaymentService paymentService;
        private readonly ApplicationDbContext db;

        public PaymentController(IPaymentService paymentService, ApplicationDbContext db)
        {
            this.paymentService = paymentService;
            this.db = db;
        }

        public async Task<IActionResult> Index(string? keyword, string? paymentStatus,
            string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var result = await paymentService.GetPagedAsync(keyword, paymentStatus, sortBy, sortDir, page, pageSize);
            return View(result);
        }

        public IActionResult Create()
        {
            PopulateOrders();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                PopulateOrders(payment.OrdersId);
                return View(payment);
            }

            await paymentService.CreateAsync(payment);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var payment = await paymentService.GetByIdAsync(id);
            if (payment == null) return NotFound();

            PopulateOrders(payment.OrdersId);
            return View(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                PopulateOrders(payment.OrdersId);
                return View(payment);
            }

            await paymentService.UpdateAsync(payment);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await paymentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult OnlinePayment(int orderId)
        {
            var order = db.Orders.Include(o => o.User).Include(o => o.Tour).FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int orderId, string email)
        {
            var success = await paymentService.ConfirmPaymentAsync(orderId);
            if (!success) return NotFound();

            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction("MyOrders", "Orders");
        }

        private void PopulateOrders(int? selectedOrderId = null)
        {
            ViewBag.Orders = new SelectList(db.Orders, "Id", "Id", selectedOrderId);
        }
    }
}
