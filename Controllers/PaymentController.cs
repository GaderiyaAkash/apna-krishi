using ApnaKrishi.Data;
using ApnaKrishi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

namespace ApnaKrishi.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public PaymentController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        public async Task<IActionResult> Payment(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null) return NotFound();

            // Create Razorpay order
            var razorpayKey = _config["Razorpay:KeyId"] ?? "rzp_test_key";
            var razorpaySecret = _config["Razorpay:KeySecret"] ?? "rzp_test_secret";

            RazorpayClient client = new(razorpayKey, razorpaySecret);
            Dictionary<string, object> options = new()
            {
                { "amount", (int)(order.GrandTotal * 100) }, // paise
                { "currency", "INR" },
                { "receipt", $"order_{orderId}" }
            };

            var razorpayOrder = client.Order.Create(options);
            string razorpayOrderId = razorpayOrder["id"].ToString() ?? string.Empty;

            // Update payment with razorpay order id
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (payment != null)
            {
                payment.RazorpayOrderId = razorpayOrderId;
                await _context.SaveChangesAsync();
            }

            ViewBag.RazorpayKey = razorpayKey;
            ViewBag.RazorpayOrderId = razorpayOrderId;
            ViewBag.Amount = (int)(order.GrandTotal * 100);
            ViewBag.OrderId = orderId;

            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserName = user?.FullName;
            ViewBag.UserEmail = user?.Email;
            ViewBag.UserPhone = user?.Mobile;

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPayment(string razorpayPaymentId,
            string razorpayOrderId, string razorpaySignature, int orderId)
        {
            var razorpaySecret = _config["Razorpay:KeySecret"] ?? "rzp_test_secret";

            // Verify signature
            var payload = $"{razorpayOrderId}|{razorpayPaymentId}";
            var key = Encoding.UTF8.GetBytes(razorpaySecret);
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            var userId = _userManager.GetUserId(User);
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null || payment == null) return NotFound();

            if (generatedSignature == razorpaySignature)
            {
                payment.RazorpayPaymentId = razorpayPaymentId;
                payment.TransactionId = razorpayPaymentId;
                payment.PaymentStatus = PaymentStatus.Success;
                payment.PaymentDate = DateTime.Now;
                order.Status = OrderStatus.Accepted;
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.Failed;
            }

            await _context.SaveChangesAsync();

            if (payment.PaymentStatus == PaymentStatus.Success)
                return RedirectToAction("Confirmation", "Order", new { orderId });

            TempData["Error"] = "Payment verification failed. Please contact support.";
            return RedirectToAction("MyOrders", "Order");
        }
    }
}
