using ApnaKrishi.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ApnaKrishi.Services
{
    public static class InvoiceGenerator
    {
        public static byte[] GeneratePdf(Order order)
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 40, 40, 50, 50);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // Fonts
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(46, 125, 50));
            var headFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY);

            // Header
            var header = new Paragraph("🌾 Apna Krishi", titleFont) { Alignment = Element.ALIGN_CENTER };
            doc.Add(header);
            doc.Add(new Paragraph("Online Agriculture Product Store", smallFont) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new Paragraph("apnakrishi.com | support@apnakrishi.com", smallFont) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new Paragraph("\n"));

            // Invoice Title
            doc.Add(new Paragraph($"INVOICE – Order #{order.OrderId}", headFont));
            doc.Add(new Paragraph($"Date: {order.OrderDate:dd MMM yyyy}", normalFont));
            doc.Add(new Paragraph($"Status: {order.Status}", normalFont));
            doc.Add(new Paragraph("\n"));

            // Customer Info
            doc.Add(new Paragraph("Delivery To:", headFont));
            doc.Add(new Paragraph(order.User?.FullName ?? "Customer", normalFont));
            doc.Add(new Paragraph(order.DeliveryAddress ?? "", normalFont));
            doc.Add(new Paragraph($"{order.City}, {order.State} – {order.PinCode}", normalFont));
            doc.Add(new Paragraph($"Email: {order.User?.Email}", normalFont));
            doc.Add(new Paragraph($"Mobile: {order.User?.Mobile}", normalFont));
            doc.Add(new Paragraph("\n"));

            // Products Table
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 30, 10, 15, 15, 15 });

            string[] headers = { "Product", "Qty", "Unit Price", "Discount", "Subtotal" };
            foreach (var h in headers)
            {
                var cell = new PdfPCell(new Phrase(h, headFont))
                {
                    BackgroundColor = new BaseColor(232, 245, 233),
                    Padding = 6
                };
                table.AddCell(cell);
            }

            foreach (var detail in order.OrderDetails)
            {
                table.AddCell(new PdfPCell(new Phrase(detail.Product?.ProductName ?? "", normalFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(detail.Quantity.ToString(), normalFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase($"₹{detail.Product?.Price:F2}", normalFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase($"{detail.Product?.DiscountPercent}%", normalFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase($"₹{detail.SubTotal:F2}", normalFont)) { Padding = 5 });
            }

            doc.Add(table);
            doc.Add(new Paragraph("\n"));

            // Totals
            var totals = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
            totals.AddCell(new PdfPCell(new Phrase("Sub Total:", normalFont)) { Border = 0, Padding = 4 });
            totals.AddCell(new PdfPCell(new Phrase($"₹{order.TotalAmount:F2}", normalFont)) { Border = 0, Padding = 4 });
            totals.AddCell(new PdfPCell(new Phrase("Shipping:", normalFont)) { Border = 0, Padding = 4 });
            totals.AddCell(new PdfPCell(new Phrase($"₹{order.ShippingCharges:F2}", normalFont)) { Border = 0, Padding = 4 });
            totals.AddCell(new PdfPCell(new Phrase("Grand Total:", headFont)) { Border = 0, Padding = 4 });
            totals.AddCell(new PdfPCell(new Phrase($"₹{order.GrandTotal:F2}", headFont)) { Border = 0, Padding = 4 });
            doc.Add(totals);

            // Payment
            doc.Add(new Paragraph("\n"));
            doc.Add(new Paragraph($"Payment Method: {order.PaymentMethod}", normalFont));
            doc.Add(new Paragraph($"Payment Status: {order.Payment?.PaymentStatus}", normalFont));
            doc.Add(new Paragraph("\nThank you for shopping with Apna Krishi!", smallFont));

            doc.Close();
            return ms.ToArray();
        }
    }
}
