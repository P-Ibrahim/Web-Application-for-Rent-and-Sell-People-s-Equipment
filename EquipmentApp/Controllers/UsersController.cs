using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EquipmentApp.Models;
using System.Web.Security;
using System.IO;

namespace EquipmentApp.Controllers
{
    public class UsersController : Controller
    {
        private EquipmentsDBEntities db = new EquipmentsDBEntities();
        //profile
        [Authorize(Roles = "Customer")]
        public ActionResult Profile()
        {

            //var users = db.Users.Include(e => e.Role);
            //return View(users.ToList());
            foreach (User user in db.Users)
            {
                if (user.UserName == User.Identity.Name)
                {
                    return View(user);
                }
            }
            return RedirectToAction("Index");
        }

        //search for users

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string searchtext)
        {
            var users = db.Users.Include(e => e.Role).Where(x => x.UserName.Contains(searchtext));
            return View(users.ToList());

        }

        //blocked users
        [Authorize(Roles = "Admin")]
        public ActionResult blockedUsers()
        {

            var users = db.Users.Include(u => u.Role);
            return View(users.ToList());
        }
        //home
        public ActionResult home()
        {

            return View();
        }

        //LOGIN
        public ActionResult Login()
        {
            
            return View();
        }
        [HttpPost]
       
        public ActionResult Login(int phone,string password )
        {
           
            bool isValid = db.Users.Any(x => x.PhoneNumber == phone && x.Password == password);
            if (isValid)
            {
                //foreach (User user in db.Users)
                //{
                //    if (user.UserName == username && user.Password == password && user.Block == true)
                //    {
                //        ViewData["messag"] = "خطأ!! يرجى  ";
                //        return View();
                //    }
                //}
                bool isBlock = db.Users.Any(x => x.PhoneNumber == phone && x.Password == password && x.Block == true);
                if (isBlock)
                {
                    ViewData["message1"] = " هذا الحساب محظور";
                    return View();
                }
                foreach(var i in db.Users)
                {
                    if (i.PhoneNumber == phone) {
                        FormsAuthentication.SetAuthCookie(i.UserName, false);
                        return RedirectToAction("Index", "Equipments");
                    }
                }
                
            }
            ViewData["message2"] = "خطأ!! يرجى التحقق والمحاولة مرة أخرى ";
            return View();
        }
        //Singup
        public ActionResult Singup()
        {
            //ViewBag.Role_Id = new SelectList(db.Roles, "Id", "Name");
            return View();
        }
        [HttpPost]
        
        public ActionResult Singup( string username, int phone, string password, string address, HttpPostedFileBase IDcard)
        {
                User user = new User();
                user.UserName = username;
                user.PhoneNumber = phone;
                user.Password = password;
                user.Address = address;

                user.Block = false;
                user.Role_Id = 2;
                if (IDcard != null)
                {
                    string path = Path.Combine(Server.MapPath("~/Uploads"), IDcard.FileName);
                    IDcard.SaveAs(path);
                    user.IdCard = IDcard.FileName;
                }
            bool isValidName = db.Users.Any(x => x.UserName == user.UserName);
            if (isValidName)
            {
                ViewData["message"] = "خطأ!! هذا اسم المستخدم موجود " +
                    "حاول تغير الاسم ";
                return View(user);
            }
            bool isValidPhone = db.Users.Any(x => x.PhoneNumber == user.PhoneNumber);
            if (isValidPhone)
            {
                ViewData["mes"] = "خطأ!! هذا الرقم موجود " +
                    "حاول تغير الرقم ";
                return View(user);
            }
            //foreach(var i in db.Users)
            //{
            //    if (i.UserName == user.UserName)
            //    {
            //        ViewData["message"] = "خطأ!! هذا اسم المستخدم موجود " +
            //            "حاول تغير الاسم ";
            //        return View(user);
            //    }
            //}
            db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("home");
            
                
           
        }
        //LOGOUT
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("login");
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Role);
            return View(users.ToList());
        }

        // GET: Users/Details/5


        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(string username, int phone, string password, string address, HttpPostedFileBase IDcard)
        {
            User user = new User();
            user.UserName = username;
            user.PhoneNumber = phone;
            user.Password = password;
            user.Address = address;

            user.Block = false;
            user.Role_Id = 2;
            if (IDcard != null)
            {
                string path = Path.Combine(Server.MapPath("~/Uploads"), IDcard.FileName);
                IDcard.SaveAs(path);
                user.IdCard = IDcard.FileName;
            }
            db.Users.Add(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            user.Block = true;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        //unblock
        [Authorize(Roles = "Admin")]
        public ActionResult UnBlock(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UnBlock(int id)
        {
            User user = db.Users.Find(id);
            user.Block = false;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
