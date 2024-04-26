using Microsoft.AspNetCore.Mvc;
using skillDev.Models;
using System.Data.SQLite;
using Dapper;

namespace skillDev.Controllers
{
    public class CourseController : Controller
    {

        private IConfiguration Configuration;
        private readonly ILogger<CourseController> logger;
        private readonly IWebHostEnvironment Environment;

        public CourseController(ILogger<CourseController> _logger, IConfiguration _Configuration, IWebHostEnvironment _environment)
        {
            logger = _logger;
            Configuration = _Configuration;
            Environment = _environment;
        }

        [HttpGet]
        public IActionResult Index()
        {


            List<Course> courses = new List<Course>();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                courses = db.Query<Course>("select * from Course").ToList();
            }
            return View(courses);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Course courses)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                courses.id = db.ExecuteScalar<int>("select Max(id) from Course") + 1;
                db.Execute("insert into  Course(id,courseName)values(@id,@courseName)", courses);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Course ct = new Course();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ct = db.Query<Course>("select * from Course where id=" + id).FirstOrDefault();
            }
            return View(ct);
        }

        [HttpPost]
        public IActionResult Edit(Course courses)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                db.Execute("update Course set courseName=@courseName where id=" + courses.id, courses);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            Course ct = new Course();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ct = db.Query<Course>("select * from Course where id=" + id).FirstOrDefault();
            }
            return View(ct);
        }

        [HttpPost]
        public IActionResult Delete(Course courses)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {

                db.Execute("delete from Course where id=" + courses.id);
            }

            return RedirectToAction("Index");
        }

    }
}
