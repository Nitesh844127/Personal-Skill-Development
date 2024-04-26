using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using Dapper;
using skillDev.Models;
using System.Data;
using ClosedXML.Excel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;

namespace skillDev.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private IConfiguration Configuration;
        private readonly ILogger<StudentController> logger;
        private readonly IWebHostEnvironment Environment;

        public StudentController(ILogger<StudentController> _logger, IConfiguration _Configuration, IWebHostEnvironment _environment)
        {
            logger = _logger;
            Configuration = _Configuration;
            Environment = _environment;
        }

        [HttpGet]
        public IActionResult Index(string stName,int cityId ,int classId,int stateId,int courseId,int h)
        {
            string where = " 1=1";
            if (stName != null)
            {
                where += " and Student.stName='" + stName + "'";
            }
            if (cityId != 0)
            {
                where += " and Student.cityId=" + cityId;
            }
            if (classId != 0)
            {
                where += " and Student.classId=" + classId;
            }
            if (stateId != 0)
            {
                where += " and Student.stateId=" + stateId;
            }
            if (courseId != 0)
            {
                where += " and Student.courseId=" + courseId;
            }

            ViewBag.stName = stName;
            ViewBag.cityId = cityId;
            ViewBag.classId = classId;
            ViewBag.stateId = stateId;
            ViewBag.courseId = courseId;

            List<Student> students = new List<Student>();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                var studentNames = db.Query<string>("SELECT stName FROM Student").ToList();

                ViewBag.StudentNames = studentNames;
                ViewBag.cl = db.Query<Class>("Select * from Class ").ToList();
                ViewBag.ct = db.Query<City>("Select * from City ").ToList();
                ViewBag.st = db.Query<State>("Select * from State ").ToList();
                ViewBag.cs = db.Query<Course>("Select * from Course ").ToList();
                students = db.Query<Student>("select Student.*,City.cityName as cityName ,Course.courseName as courseName ,State.stateName as stateName ,Class.className as className " +
                    "from Student left join City on Student.cityId=City.id left join Course on Student.courseId=Course.id left join State on Student.stateId=State.id left join Class on Student.classId=Class.id where " + where).ToList();
                decimal Total = students.Sum(i => i.fees);
                ViewBag.totalFees = Total;
            }
            return View(students);
        }

        [HttpPost]
        public IActionResult Index(string stName, int cityId, int classId, int stateId, int courseId)
        {
            return RedirectToAction("Index", new { stName = stName, cityId = cityId, classId = classId, stateId = stateId, courseId = courseId });
        }


        [HttpGet]
        public IActionResult Create(string url)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            { 
                ViewBag.url = url;
                ViewBag.cl = db.Query<Class>("Select * from Class ").ToList();
                ViewBag.ct = db.Query<City>("Select * from City ").ToList();
                ViewBag.st = db.Query<State>("Select * from State ").ToList();
                ViewBag.cs = db.Query<Course>("Select * from Course ").ToList();
            }
            return View();
        }


        [HttpPost]
    
        public IActionResult Create(Student students, IFormFile studentImage, string url)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                if (studentImage != null && studentImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Environment.WebRootPath, "images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + studentImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    studentImage.CopyTo(new FileStream(filePath, FileMode.Create));
                    students.ImagePath = "/images/" + uniqueFileName;
                }

                students.id = db.ExecuteScalar<int>("select Max(id) from Student") + 1;
                db.Execute("insert into  Student(id,stName,classId,cityId,stateId,mobNo,cast,courseId,gender,fees, ImagePath) values(@id,@stName,@classId,@cityId,@stateId,@mobNo,@cast,@courseId,@gender,@fees, @ImagePath)", students);
            }

            return Redirect(url);
        }

       

        [HttpGet]
        public IActionResult Edit(int id,string url)
        {
            Student ctt = new Student();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ViewBag.url = url;
                ViewBag.cl = db.Query<Class>("Select * from Class ").ToList();
                ViewBag.ct = db.Query<City>("Select * from City ").ToList();
                ViewBag.st = db.Query<State>("Select * from State ").ToList();
                ViewBag.cs = db.Query<Course>("Select * from Course ").ToList();
                ctt = db.Query<Student>("select * from Student where id=" + id).FirstOrDefault();
            }
            return View(ctt);
        }

        [HttpPost]
        public IActionResult Edit(Student students, IFormFile studentImage, string url)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                if (studentImage != null && studentImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Environment.WebRootPath, "images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + studentImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    studentImage.CopyTo(new FileStream(filePath, FileMode.Create));
                    students.ImagePath = "/images/" + uniqueFileName;
                }
                else
                {
                    students.ImagePath = db.ExecuteScalar<string>("select ImagePath from Student where id=@id", new { id = students.id });
                }
                db.Execute("update Student set stName=@stName,classId=@classId,cityId=@cityId,stateId=@stateId,mobNo=@mobNo,cast=@cast,courseId=@courseId,gender=@gender,fees=@fees, ImagePath=@ImagePath where id=@id", students);
            }

            return Redirect(url);
        }
      
        [HttpGet]
        public IActionResult Delete(int id, string url)
        {
            Student ct = new Student();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ViewBag.url = url;
                ct = db.Query<Student>("select * from Student where id=" + id).FirstOrDefault();
            }
            return View(ct);
        }

        [HttpPost]
        public IActionResult Delete(Student students, string url)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {

                db.Execute("delete from Student where id=" + students.id);
            }

            return Redirect(url);
        }


        public FileContentResult ExcelExport(string stName, int cityId, int classId, int stateId, int courseId)
        {
            string where = " 1=1";
            if (stName != null)
            {
                where += " and Student.stName='" + stName + "'";
            }
            if (cityId != 0)
            {
                where += " and Student.cityId=" + cityId;
            }
            if (classId != 0)
            {
                where += " and Student.classId=" + classId;
            }
            if (stateId != 0)
            {
                where += " and Student.stateId=" + stateId;
            }
            if (courseId != 0)
            {
                where += " and Student.courseId=" + courseId;
            }

            List<Student> students = new List<Student>();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ViewBag.stName = stName;
                ViewBag.cl = db.Query<Class>("Select * from Class ").ToList();
                ViewBag.ct = db.Query<City>("Select * from City ").ToList();
                ViewBag.st = db.Query<State>("Select * from State ").ToList();
                ViewBag.cs = db.Query<Course>("Select * from Course ").ToList();
                students = db.Query<Student>("select Student.*,City.cityName as cityName ,Course.courseName as courseName ,State.stateName as stateName ,Class.className as className " +
                    "from Student left join City on Student.cityId=City.id left join Course on Student.courseId=Course.id left join State on Student.stateId=State.id left join Class on Student.classId=Class.id where " + where).ToList();
            }
            decimal totalFees = students.Sum(s => s.fees);
            DataTable dt = new DataTable("Student");
            dt.Columns.AddRange(new DataColumn[9]
                {
                     new DataColumn("Name"),
                     new DataColumn("Class"),
                     new DataColumn("City"),
                     new DataColumn("State"),
                     new DataColumn("Course"),
                     new DataColumn("Caste"),
                     new DataColumn("Gender"),
                     new DataColumn("Fees"),
                     new DataColumn("Mob No"),
            });
            foreach (Student obj in students)
            {
                dt.Rows.Add(obj.stName,
                    obj.className,
                    obj.cityName,
                    obj.stateName,
                    obj.courseName,
                    obj.cast,
                    obj.gender == "1" ? "Male" : obj.gender == "2" ? "Female" : "",
                    obj.fees,
                    obj.mobNo

                    );
            }
            dt.Rows.Add("Total Fees", "", "", "", "", "", "", totalFees, "");
            using (XLWorkbook wb = new XLWorkbook())

            {
                IXLWorksheet ws = wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Student.xlsx");
                }

            }
        }

        //pdf download code 
        public FileContentResult PdfExport(string stName, int cityId, int classId, int stateId, int courseId)
        {
            string where = " 1=1";
            if (stName != null)
            {   
                where += " and Student.stName='" + stName + "'";
            }
            if (cityId != 0)
            {
                where += " and Student.cityId=" + cityId;
            }
            if (classId != 0)
            {
                where += " and Student.classId=" + classId;
            }
            if (stateId != 0)
            {
                where += " and Student.stateId=" + stateId;
            }
            if (courseId != 0)
            {
                where += " and Student.courseId=" + courseId;
            }

            List<Student> students = new List<Student>();
            string city ;
            string classes ;
            string state;
            string course;
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {

                ViewBag.stName = stName;
                ViewBag.cl = db.Query<Class>("Select * from Class ").ToList();
                ViewBag.ct = db.Query<City>("Select * from City ").ToList();
                ViewBag.st = db.Query<State>("Select * from State ").ToList();
                ViewBag.cs = db.Query<Course>("Select * from Course ").ToList();
                students = db.Query<Student>("select Student.*,City.cityName as cityName ,Course.courseName as courseName ,State.stateName as stateName ,Class.className as className " +
                    "from Student left join City on Student.cityId=City.id left join Course on Student.courseId=Course.id left join State on Student.stateId=State.id left join Class on Student.classId=Class.id where " + where).ToList();
               city     = db.ExecuteScalar<string>("select cityName from City where id="+ cityId);
               classes  = db.ExecuteScalar<string>("select className from Class where id=" + classId);
               state   =  db.ExecuteScalar<string>("select stateName from State where id=" + stateId);
               course = db.ExecuteScalar<string>("select courseName from Course where id=" + courseId);
            }
            decimal totalFees = students.Sum(s => s.fees);
            iTextSharp.text.Font fonta = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font fontb = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font fontc = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            iTextSharp.text.Font fontd = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

            MemoryStream mmStream = new MemoryStream();
            Document doc = new Document(PageSize.A4, 15, 15, 15, 15);

            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, mmStream);
            doc.Open();
            PdfContentByte cb = pdfWriter.DirectContent;

            iTextSharp.text.Paragraph report = new iTextSharp.text.Paragraph("Students List", fontd);
            report.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            report.Font = fontd;
            doc.Add(report);

            PdfPTable table1 = new PdfPTable(4);
            float[] widths1 = new float[] { 1.4f, 1.4f, 1.4f, 1.4f};
            table1.SetWidths(widths1);
            table1.SpacingBefore = 20;
            table1.TotalWidth = 560;
            table1.LockedWidth = true;
            PdfPCell cell1;

            if (cityId > 0)
            {
                cell1 = new PdfPCell(new Phrase("City : " + city, fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);
            }
            else
            {
                cell1 = new PdfPCell(new Phrase("City : ", fontd));
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
            if (stateId > 0)
            {
                cell1 = new PdfPCell(new Phrase("State : " + state, fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);
            }
            else
            {
                cell1 = new PdfPCell(new Phrase("State : ", fontd));
                cell1.HorizontalAlignment = 1;
                table1.AddCell(cell1);
            }

            
            doc.Add(table1);

            PdfPTable table = new PdfPTable(10);
            float[] widths = new float[] { .7f, .7f, .6f, .9f, .6f,.7f,.7f,.7f,.6f,.8f };
            table.SetWidths(widths);
            table.SpacingBefore = 20;
            table.TotalWidth = 560;
            table.LockedWidth = true;

            PdfPCell cell;
            cell = new PdfPCell(new Phrase("Student Image", fontd)); 
            cell.HorizontalAlignment = 1;
            table.AddCell(cell); 
            
            cell = new PdfPCell(new Phrase("Student Name", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Class", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("City", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("State", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Course", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Caste", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Gender", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Fees", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Mob No", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);


            foreach (Student obj in students)
            {

                string imagePath = Path.Combine(Environment.WebRootPath, obj.ImagePath.TrimStart('/'));
                byte[] imageData = System.IO.File.ReadAllBytes(imagePath);

                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageData);
                image.ScaleAbsolute(50f, 50f); 
                cell = new PdfPCell(image);
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(obj.stName, fontb));
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.className, fontb));
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.cityName, fontb));
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.stateName, fontb));
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.courseName, fontb));
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.cast, fontb));
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.gender == "1" ? "Male" : obj.gender == "2" ? "Female" : "", fontb));
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.fees.ToString(), fontb));
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj.mobNo, fontb));
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);


            }
            cell = new PdfPCell(new Phrase("Total Fees ", fontd));
            cell.Colspan = 8; 
            cell.HorizontalAlignment = 2;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(totalFees.ToString(), fontb));
            cell.HorizontalAlignment = 2;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("", fontb));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);

            doc.Add(table);
            pdfWriter.CloseStream = false;
            doc.Close();

            byte[] bytea = mmStream.ToArray();
            return File(bytea, "application/pdf", "Students.pdf");
        }

    }
}
