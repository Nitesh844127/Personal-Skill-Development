using Microsoft.AspNetCore.Mvc;
using skillDev.Models;
using System.Data.SQLite;
using Dapper;

namespace skillDev.Controllers
{
    public class ClassController : Controller
    {
        private IConfiguration Configuration;
        private readonly ILogger<ClassController> logger;
        private readonly IWebHostEnvironment Environment;

        public ClassController(ILogger<ClassController> _logger, IConfiguration _Configuration, IWebHostEnvironment _environment)
        {
            logger = _logger;
            Configuration = _Configuration;
            Environment = _environment;
        }

        [HttpGet]
        public IActionResult Index()
        {


            List<Class> classes = new List<Class>();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                classes = db.Query<Class>("select * from Class").ToList();
            }
            return View(classes);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Class classes)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                classes.id = db.ExecuteScalar<int>("select Max(id) from Class") + 1;
                db.Execute("insert into  Class(id,className)values(@id,@className)", classes);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Class ct = new Class();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ct = db.Query<Class>("select * from Class where id=" + id).FirstOrDefault();
            }
            return View(ct);
        }

        [HttpPost]
        public IActionResult Edit(Class classes)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                db.Execute("update Class set className=@className where id=" + classes.id, classes);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            Class ct = new Class();
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {
                ct = db.Query<Class>("select * from Class where id=" + id).FirstOrDefault();
            }
            return View(ct);
        }

        [HttpPost]
        public IActionResult Delete(Class classes)
        {
            string Conn = this.Configuration.GetConnectionString("myconn");
            using (SQLiteConnection db = new SQLiteConnection(Conn))
            {

                db.Execute("delete from Class where id=" + classes.id);
            }

            return RedirectToAction("Index");
        }

    }
}
