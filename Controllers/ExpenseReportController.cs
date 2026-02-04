using ExpenseManager.Areas.Identity.Data;
using ExpenseManager.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin,User")]
public class ExpenseReportController : Controller
{
    private readonly ApplicationDBContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExpenseReportController(ApplicationDBContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    // 📊 Reports Page
    public async Task<IActionResult> ExpenseReports(DateTime? startDate, DateTime? endDate, int? categoryId, string? userId, string? userName)
    {
        try
        {
            var query = _dbContext.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .AsQueryable();

            // order by DateAndTime (descending) → latest first
            query = query.OrderByDescending(e => e.DateAndTime);

            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);
                query = query.Where(e => e.UserId == currentUserId);
            }

            if (startDate.HasValue)
                query = query.Where(e => e.DateAndTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.DateAndTime <= endDate.Value);

            if (categoryId.HasValue)
                query = query.Where(e => e.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(userName) && User.IsInRole("Admin"))
                query = query.Where(e => e.User.UserName.Contains(userName));

            var expenses = await query.ToListAsync();

            // populate dropdowns
            ViewBag.Categories = new SelectList(await _dbContext.Categories.ToListAsync(), "Id", "Name", categoryId);

            if (User.IsInRole("Admin"))
                ViewBag.Users = new SelectList(await _dbContext.Users.ToListAsync(), "Id", "UserName", userId);
            else
                ViewBag.Users = null;

            return View(expenses);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error while fetching expense records!!!");
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(DateTime? startDate, DateTime? endDate, string categoryId, string userName)
    {
        try
        {
            var query = _dbContext.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .AsQueryable();

            // order by DateAndTime (ascending)
            query = query.OrderBy(e => e.DateAndTime);

            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);
                query = query.Where(e => e.UserId == currentUserId);
            }

            if (startDate.HasValue)
                query = query.Where(e => e.DateAndTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.DateAndTime <= endDate.Value);

            if (!string.IsNullOrEmpty(categoryId))
                query = query.Where(e => e.CategoryId.ToString() == categoryId);

            if (!string.IsNullOrEmpty(userName) && User.IsInRole("Admin"))
                query = query.Where(e => e.User.UserName.Contains(userName));

            var expenses = await query.ToListAsync();
            var totalAmount = expenses.Sum(e => e.Amount);

            using (var ms = new MemoryStream())
            {
                var doc = new iTextSharp.text.Document(PageSize.A4, 36, 36, 54, 54);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ✅ Gray Theme Colors
                BaseColor darkGray = new BaseColor(66, 66, 66);
                BaseColor midGray = new BaseColor(128, 128, 128);
                BaseColor rowAltGray = new BaseColor(245, 245, 245);

                // ✅ Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.White);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.Black);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.White);

                // 📌 Title Header
                var titleTable = new PdfPTable(1) { WidthPercentage = 100 };
                var titleCell = new PdfPCell(new Phrase("📊 Expense Report", titleFont))
                {
                    BackgroundColor = darkGray,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 12,
                    Border = Rectangle.NO_BORDER
                };
                titleTable.AddCell(titleCell);
                doc.Add(titleTable);

                // ✅ Branding
                doc.Add(new Paragraph("ExpenseManager - Student Project (By Aniket Supekar)", normalFont));
                doc.Add(new Paragraph($"\nReport Date: {DateTime.Now:dd MMM yyyy}", normalFont));
                doc.Add(new Paragraph("\n"));

                // ✅ Watermark
                PdfContentByte canvas = writer.DirectContent;
                Font watermarkFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 58, BaseColor.LightGray);
                Phrase watermark = new Phrase("Expense Manager", watermarkFont);

                PdfGState gState = new PdfGState { FillOpacity = 0.30f };
                canvas.SaveState();
                canvas.SetGState(gState);
                ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, watermark, 300f, 400f, 45f);
                canvas.RestoreState();

                // 📌 Dynamic Table
                bool isAdmin = User.IsInRole("Admin");

                // ✅ If Admin → 6 columns (Sr.No, Date, Item, Category, Amount, UserName)
                // ✅ If User → 5 columns (Sr.No, Date, Item, Category, Amount)
                int columnCount = isAdmin ? 6 : 5;
                var table = new PdfPTable(columnCount) { WidthPercentage = 100, SpacingBefore = 10f };

                if (isAdmin)
                    table.SetWidths(new float[] { 8f, 15f, 25f, 18f, 15f, 19f });
                else
                    table.SetWidths(new float[] { 10f, 20f, 30f, 20f, 20f });

                // ✅ Table Headers
                string[] headers = isAdmin
                    ? new[] { "Sr.No", "Date", "User", "Item", "Category", "Amount" }
                    : new[] { "Sr.No", "Date", "Item", "Category", "Amount" };

                foreach (var h in headers)
                {
                    var cell = new PdfPCell(new Phrase(h, headerFont))
                    {
                        BackgroundColor = midGray,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 7
                    };
                    table.AddCell(cell);
                }

                // ✅ Table Rows
                bool alternate = false;
                var sn = 1;

                foreach (var exp in expenses)
                {
                    BaseColor rowColor = alternate ? rowAltGray : BaseColor.White;
                    alternate = !alternate;

                    table.AddCell(new PdfPCell(new Phrase(sn.ToString(), normalFont)) { Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = rowColor });
                    table.AddCell(new PdfPCell(new Phrase(exp.DateAndTime.ToShortDateString(), normalFont)) { Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = rowColor });

                    if (isAdmin) // 👈 Add username column only for Admin
                        table.AddCell(new PdfPCell(new Phrase(exp.User?.UserName ?? "-", normalFont)) { Padding = 6, BackgroundColor = rowColor });

                    table.AddCell(new PdfPCell(new Phrase(exp.ItemName, normalFont)) { Padding = 6, BackgroundColor = rowColor });
                    table.AddCell(new PdfPCell(new Phrase(exp.Category?.Name ?? "-", normalFont)) { Padding = 6, BackgroundColor = rowColor });
                    table.AddCell(new PdfPCell(new Phrase("₹" + exp.Amount, normalFont)) { Padding = 6, HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = rowColor });

                    sn++;
                }

                // 📌 Total Row
                var totalCell = new PdfPCell(new Phrase("Total", headerFont))
                {
                    Colspan = isAdmin ? 5 : 4, // 👈 shift colspan
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 8,
                    BackgroundColor = darkGray
                };
                table.AddCell(totalCell);

                table.AddCell(new PdfPCell(new Phrase("₹" + totalAmount, headerFont))
                {
                    Padding = 8,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    BackgroundColor = darkGray
                });

                doc.Add(table);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", "ExpenseReport.pdf");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error while generating pdf!!!");
            return View();
        }
    }


}
