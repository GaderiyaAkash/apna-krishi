# Apna Krishi – Setup Guide

## Prerequisites
- .NET 8 SDK  →  https://dotnet.microsoft.com/download
- SQL Server LocalDB (ships with Visual Studio) or full SQL Server
- Visual Studio 2022 or VS Code

---

## Step 1: Configure Connection String

`appsettings.json` is already set for LocalDB:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ApnaKrishiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```
For full SQL Server change to:
```
Server=YOUR_SERVER;Database=ApnaKrishiDb;User Id=sa;Password=yourpwd;TrustServerCertificate=True
```

---

## Step 2: Configure Email (Gmail App Password)

```json
"Email": {
  "Username": "your-gmail@gmail.com",
  "Password": "xxxx xxxx xxxx xxxx"   ← 16-char App Password
}
```
Generate at: https://myaccount.google.com/apppasswords

---

## Step 3: Configure Razorpay (Test Keys)

```json
"Razorpay": {
  "KeyId":    "rzp_test_XXXXXXXXXX",
  "KeySecret": "your_secret"
}
```
Dashboard: https://dashboard.razorpay.com/app/keys

---

## Step 4: Database Migration

**Option A – Package Manager Console (Visual Studio)**
```powershell
Add-Migration InitialCreate
Update-Database
```

**Option B – .NET CLI**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates all 9 tables and seeds:
- 4 Categories (Fertilizers, Seeds, Pesticides, Farming Tools)
- 8 Sample Products
- Roles: Admin, Customer
- Admin user: admin@apnakrishi.com / Admin@123
- Default WebsiteSettings row

---

## Step 5: Add Product Images

Place in `wwwroot/images/`:
- `no-image.png` — fallback product image (300×300 grey placeholder)
- `favicon.ico`  — wheat/leaf icon

Upload product images via Admin → Products → Edit.

---

## Step 6: Run

```bash
dotnet run
```
Or press **F5** in Visual Studio.

---

## Default Admin Credentials

| Email                  | Password  |
|------------------------|-----------|
| admin@apnakrishi.com   | Admin@123 |

---

## Project Structure

```
ApnaKrishi/
├── Controllers/
│   ├── AccountController.cs    Register · Login · Forgot/Reset Password
│   ├── HomeController.cs       Home page (featured · best sellers · new arrivals)
│   ├── ProductController.cs    Listing (search+filter+pagination) · Details · Reviews
│   ├── CartController.cs       Add · Update qty · Remove · Count API
│   ├── OrderController.cs      Checkout · Place order · My orders · Cancel · PDF invoice
│   ├── PaymentController.cs    Razorpay create order + HMAC verify
│   ├── UserController.cs       Profile · Change password
│   └── AdminController.cs      Full admin panel (9 sections)
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── Category.cs
│   ├── Product.cs
│   ├── Cart.cs
│   ├── Order.cs  (enums: OrderStatus, PaymentMethod)
│   ├── OrderDetail.cs
│   ├── Payment.cs  (enum: PaymentStatus)
│   ├── Review.cs
│   ├── WebsiteSettings.cs      ← singleton settings row
│   └── ViewModels/
│       ├── RegisterViewModel · LoginViewModel · ProfileViewModel
│       ├── HomeViewModel · ProductListViewModel · CheckoutViewModel
│       └── AdminViewModels (Dashboard · ProductForm · Reports · WebsiteSettings)
│
├── Data/
│   ├── ApplicationDbContext.cs  EF Core + seed data
│   └── DbSeeder.cs             Roles + admin user
│
├── Services/
│   ├── IEmailService / EmailService   MailKit SMTP
│   └── InvoiceGenerator               iTextSharp PDF
│
├── Views/
│   ├── Home/Index                      Hero · Categories · Featured · Best Sellers · New
│   ├── Product/Index · Details         Listing with filters · Product page + reviews
│   ├── Cart/Index                      Cart with qty controls
│   ├── Order/Checkout · Confirmation · MyOrders · Details
│   ├── Payment/Payment                 Razorpay checkout JS
│   ├── Account/Register · Login · ForgotPassword · ResetPassword
│   ├── User/Profile · ChangePassword
│   ├── Admin/
│   │   ├── Index          Dashboard (bar chart + donut + top products)
│   │   ├── Categories · AddCategory · EditCategory
│   │   ├── Products  · AddProduct  · EditProduct
│   │   ├── Users     · UserDetails
│   │   ├── Orders    · OrderDetails  (Accept/Reject/Dispatch/Deliver inline)
│   │   ├── Payments  (Refund modal)
│   │   ├── Reports   (Daily · Monthly · Product Sales · Customer Report)
│   │   └── Settings  (General · Shipping · Social · Access controls)
│   └── Shared/_Layout · _AdminLayout · _ProductCard · _ValidationScripts
│
└── wwwroot/
    ├── css/site.css · admin.css
    ├── js/site.js
    └── uploads/  (auto-created on first image upload)
```

---

## Admin Features Checklist

### Dashboard
- [x] Total Users · Products · Orders · Revenue cards
- [x] Today's orders + revenue
- [x] Monthly revenue bar chart (Chart.js)
- [x] Order status donut chart
- [x] Top 5 selling products
- [x] Recent orders table with quick view

### Category Management
- [x] View all categories
- [x] Add / Edit / Delete category
- [x] Image upload
- [x] Active/Inactive toggle
- [x] Delete guard (prevents delete if products exist)

### Product Management
- [x] View all products (search · category filter · stock filter)
- [x] Add / Edit / Delete product
- [x] Image upload with preview
- [x] Manage stock
- [x] Featured / Best Seller / New Arrival tags
- [x] Active/Inactive toggle

### User Management
- [x] View all customers (excludes admins)
- [x] Search by name / email / mobile
- [x] Filter: All / Blocked
- [x] View customer details + order history
- [x] Block / Unblock users
- [x] Delete users

### Order Management
- [x] View orders with status tabs + counts
- [x] Search + date range filter
- [x] Order detail page with status timeline
- [x] Change order status (dropdown)
- [x] Quick action buttons: Accept · Reject · Dispatch · Deliver
- [x] Customer info + delivery address on detail page

### Payment Management
- [x] View all transactions with status tabs
- [x] Search by customer / transaction ID / order ID
- [x] Refund with confirmation modal (marks payment Refunded + cancels order)
- [x] Total collected amount display

### Reports
- [x] Daily Sales Report — date range filter, summary cards, full order table
- [x] Monthly Sales Report — revenue trend line chart, breakdown table with progress bars
- [x] Product Sales Report — quantity, revenue, revenue share per product
- [x] Customer Report — total orders, total spent, last order per customer

### Settings
- [x] General: site name, tagline, contact email/phone, address, meta description
- [x] Shipping: free threshold, flat charge, GST %
- [x] Social media: Facebook, Instagram, Twitter links
- [x] Access controls: maintenance mode toggle, allow registrations toggle
- [x] Admin shortcuts: edit profile, change password
- [x] System info panel
