using ApnaKrishi.Data;
using ApnaKrishi.Models;
using ApnaKrishi.Models.ViewModels;
using ApnaKrishi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApnaKrishi.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public OrderController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var subTotal = cartItems.Sum(c => c.TotalPrice);
            var shipping = subTotal >= 500 ? 0 : 60;

            var vm = new CheckoutViewModel
            {
                CartItems = cartItems,
                SubTotal = subTotal,
                ShippingCharges = shipping,
                GrandTotal = subTotal + shipping,
                DeliveryAddress = user?.Address ?? string.Empty,
                City = user?.City ?? string.Empty,
                State = user?.State ?? string.Empty,
                PinCode = user?.PinCode ?? string.Empty
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var subTotal = cartItems.Sum(c => c.TotalPrice);
            var shipping = subTotal >= 500 ? 0 : 60;
            var grandTotal = subTotal + shipping;

            // Create Order
            var order = new Order
            {
                UserId = userId!,
                OrderDate = DateTime.Now,
                TotalAmount = subTotal,
                ShippingCharges = shipping,
                GrandTotal = grandTotal,
                PaymentMethod = model.PaymentMethod,
                Status = OrderStatus.Pending,
                DeliveryAddress = model.DeliveryAddress,
                City = model.City,
                State = model.State,
                PinCode = model.PinCode,
                Notes = model.Notes
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add Order Details & reduce stock
            foreach (var item in cartItems)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product!.DiscountedPrice
                });

                item.Product.Stock -= item.Quantity;
            }

            // Create Payment record
            _context.Payments.Add(new Payment
            {
                OrderId = order.OrderId,
                PaymentDate = DateTime.Now,
                Amount = grandTotal,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentMethod == PaymentMethod.CashOnDelivery
                    ? PaymentStatus.Pending
                    : PaymentStatus.Pending
            });

            // Clear Cart
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Send confirmation email (non-blocking – don't crash if SMTP not configured)
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.Email != null)
                {
                    await _emailService.SendOrderConfirmationAsync(user.Email, user.FullName, order.OrderId, grandTotal);
                }
            }
            catch (Exception)
            {
                // Email failed – order is still placed successfully
            }

            if (model.PaymentMethod != PaymentMethod.CashOnDelivery)
                return RedirectToAction("Payment", "Payment", new { orderId = order.OrderId });

            return RedirectToAction("Confirmation", new { orderId = order.OrderId });
        }

        public async Task<IActionResult> Confirmation(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null) return NotFound();

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Accepted)
            {
                TempData["Error"] = "Order cannot be cancelled at this stage.";
                return RedirectToAction("MyOrders");
            }

            order.Status = OrderStatus.Cancelled;

            // Restore stock
            foreach (var detail in order.OrderDetails)
            {
                if (detail.Product != null)
                    detail.Product.Stock += detail.Quantity;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Order cancelled successfully.";
            return RedirectToAction("MyOrders");
        }

        public async Task<IActionResult> DownloadInvoice(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null) return NotFound();

            var pdfBytes = InvoiceGenerator.GeneratePdf(order);
            return File(pdfBytes, "application/pdf", $"Invoice_Order_{orderId}.pdf");
        }
    }
}
