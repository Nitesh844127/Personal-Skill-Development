using Microsoft.AspNetCore.Mvc;
using skillDev.Models;
using System.Data.SQLite;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using System.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text;


namespace skillDev.Controllers
{
    public class ReceiptController : Controller
    {
        private IConfiguration Configuration;
        private readonly ILogger<ReceiptController> logger;
        private readonly IWebHostEnvironment Environment;

        public ReceiptController(ILogger<ReceiptController> _logger, IConfiguration _Configuration, IWebHostEnvironment _environment)
        {
            logger = _logger;
            Configuration = _Configuration;
            Environment = _environment;
        }

        [HttpGet]
        public IActionResult Index(string stName, int classId, int courseId, string fromDate, string toDate, int h)
        {
            string where = "1=1";
            if (stName != null)
            {
                where += " and Student.stName='" + stName + "'";
            }

            if (classId != 0)
            {
                where += " and Student.classId=" + classId;
            }

            if (courseId != 0)
            {
                where += " and Student.courseId=" + courseId;
            }
            ViewBag.stName = stName;
            ViewBag.classId = classId;
            ViewBag.courseId = courseId;
            List<Receipt> receipts = new List<Receipt>();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                
                ViewBag.cl = db.Query<Class>("SELECT * FROM Class").ToList();
                ViewBag.cs = db.Query<Course>("SELECT * FROM Course").ToList();
                ViewBag.StudentNames = db.Query<Student>("SELECT id, stName, mobNo, courseId, classId FROM Student").ToList();

                receipts = db.Query<Receipt>("SELECT Receipt.*, Student.stName AS studName, Student.mobNo AS mob, Course.courseName AS courseName, Class.className AS className " +
                    "FROM Receipt " +
                    "LEFT JOIN Student ON Receipt.studId = Student.id " +
                    "LEFT JOIN Course ON Student.courseId = Course.id " +
                    "LEFT JOIN Class ON Student.classId = Class.id " +
                    "WHERE " + where).ToList();
                decimal totalAmount = receipts.Sum(r => r.amount);
                ViewBag.total = totalAmount;

            }

            return View(receipts);
        }

     
        [HttpPost]
        public IActionResult Index(string stName, int classId, int courseId, string fromDate, string toDate)
        {
            return RedirectToAction("Index", new { stName = stName, classId = classId, courseId = courseId, fromDate = fromDate, toDate = toDate });
        }

        [HttpGet]
        public IActionResult Create(string url)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ViewBag.url = url;
                ViewBag.cl = db.Query<Class>("Select * from Class ").ToList();
                ViewBag.cs = db.Query<Course>("Select * from Course ").ToList();
                var studentNames = db.Query<Student>("SELECT id,stName FROM Student").ToList();
                ViewBag.StudentNames = studentNames;
            }
            return View();
        }

        [HttpPost]
        public IActionResult Create(Receipt receipts, DateTime date1, string url)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                receipts.date = int.Parse(date1.ToString("yyyyMMdd"));

                receipts.id = db.ExecuteScalar<int>("select Max(id) from Receipt") + 1;
                db.Execute("insert into  Receipt(id,studId,date,remark,amount)values(@id,@studId,@date,@remark,@amount)", receipts);
                ViewBag.SweetAlertTitle = "Success";
                ViewBag.SweetAlertMessage = "Receipt created successfully";
                ViewBag.SweetAlertIcon = "success";
            }
            TempData["ReceiptCreated"] = true;
            return Redirect(url);
            //return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Edit(int id, string url)
        {
            Receipt ctt = new Receipt();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ViewBag.url = url;
                var studentNames = db.Query<Student>("SELECT id,stName FROM Student").ToList();
                ViewBag.StudentNames = studentNames;
                ctt = db.Query<Receipt>("select * from Receipt where id=" + id).FirstOrDefault();
            }
            return View(ctt);
        }

        [HttpPost]
        public IActionResult Edit(Receipt receipts, DateTime date1, string url)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                receipts.date = int.Parse(date1.ToString("yyyyMMdd"));
                db.Execute("update Receipt set id=@id,studId=@studId,date=@date,remark=@remark,amount=@amount where id=" + receipts.id, receipts);
            }
            TempData["ReceiptUpdated"] = true;
            return Redirect(url);
            //return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Delete(int id, string url)
        {
            Receipt ct = new Receipt();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ViewBag.url = url;
                ct = db.Query<Receipt>("select * from Receipt where id=" + id).FirstOrDefault();
            }
            return View(ct);
        }

        [HttpPost]
        public IActionResult Delete(Receipt receipts, string url )
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {

                db.Execute("delete from Receipt where id=" + receipts.id);
            }
            TempData["ReceiptDeleted"] = true;
            return Redirect(url);
            //return RedirectToAction("Index");
        }

        //pdf download code 
        public FileContentResult PdfExport(string stName, int classId, int courseId, string fromDate, string toDate)
        {
            string where = "1=1";
            if (stName != null)
            {
                where += " and Student.stName='" + stName + "'";
            }

            if (classId != 0)
            {
                where += " and Student.classId=" + classId;
            }

            if (courseId != 0)
            {
                where += " and Student.courseId=" + courseId;
            }

            List<Receipt> receipts = new List<Receipt>();
            string classes;
            string course;
            string StudentNames;
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                classes = db.ExecuteScalar<string>("select className from Class where id=" + classId);
                course = db.ExecuteScalar<string>("select courseName from Course where id=" + courseId);

                //StudentNames = db.Query<string>("SELECT id, stName, mobNo, courseId, classId FROM Student where stName="+''" + stName + "');

                receipts = db.Query<Receipt>("SELECT Receipt.*, Student.stName AS studName, Student.mobNo AS mob, Course.courseName AS courseName, Class.className AS className " +
                    "FROM Receipt " +
                    "LEFT JOIN Student ON Receipt.studId = Student.id " +
                    "LEFT JOIN Course ON Student.courseId = Course.id " +
                    "LEFT JOIN Class ON Student.classId = Class.id " +
                    "WHERE " + where).ToList();


            }
            decimal totalFees = receipts.Sum(s => s.amount);

            iTextSharp.text.Font fonta = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font fontb = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font fontc = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font fontd = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

            MemoryStream mmStream = new MemoryStream();
            Document doc = new Document(PageSize.A4, 15, 15, 15, 15);

            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, mmStream);
            doc.Open();
            PdfContentByte cb = pdfWriter.DirectContent;

            iTextSharp.text.Paragraph report = new iTextSharp.text.Paragraph("Receipt", fontd);
            report.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            report.Font = fontd;
            doc.Add(report);

            PdfPTable table1 = new PdfPTable(3);
            float[] widths1 = new float[] { 1.4f, 1.4f, 1.4f };
            table1.SetWidths(widths1);
            table1.SpacingBefore = 20;
            table1.TotalWidth = 560;
            table1.LockedWidth = true;
            PdfPCell cell1;

            if (stName != "")
            {

                cell1 = new PdfPCell(new Phrase("Student Name : " + stName, fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);


            }
            else
            {
                cell1 = new PdfPCell(new Phrase("Student Name : ", fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);
            }
            if (courseId > 0)
            {

                cell1 = new PdfPCell(new Phrase("Course : " + course, fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);

            }
            else
            {
                cell1 = new PdfPCell(new Phrase("Course : ", fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);
            }
            if (classId > 0)
            {

                cell1 = new PdfPCell(new Phrase("Class : " + classes, fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);

            }
            else
            {
                cell1 = new PdfPCell(new Phrase("Class : ", fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);
            }



            doc.Add(table1);

            PdfPTable table = new PdfPTable(7);
            float[] widths = new float[] { .7f, .9f, .7f, .7f, .9f, .7f, 1.3f };
            table.SetWidths(widths);
            table.SpacingBefore = 20;
            table.TotalWidth = 560;
            table.LockedWidth = true;

            PdfPCell cell;
            cell = new PdfPCell(new Phrase("Student", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Receipt Date", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Class", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Course", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Mob No", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);


            cell = new PdfPCell(new Phrase("Amount", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Remark", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);



            foreach (Receipt obj in receipts)
            {

                cell = new PdfPCell(new Phrase(obj.studName, fontb));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.date.ToDate().ToString("yyyy-MM-dd"), fontb));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.className, fontb));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.courseName, fontb));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.mob, fontb));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.amount.ToString(), fontb));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.remark, fontb));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);


            }
            cell = new PdfPCell(new Phrase("Total Fees ", fontd));
            cell.Colspan = 5;
            cell.HorizontalAlignment = 2;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(totalFees.ToString(), fontb));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("", fontb));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
           

            doc.Add(table);
            pdfWriter.CloseStream = false;
            doc.Close();

            byte[] bytea = mmStream.ToArray();
            return File(bytea, "application/pdf", "Receipt.pdf");
        }




    }
}

