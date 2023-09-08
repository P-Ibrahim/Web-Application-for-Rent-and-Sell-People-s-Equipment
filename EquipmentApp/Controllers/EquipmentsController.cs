using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EquipmentApp.Models;

namespace EquipmentApp.Controllers
{
    public class EquipmentsController : Controller
    {
        private EquipmentsDBEntities db = new EquipmentsDBEntities();

        public ActionResult About()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string searchtext)
        {
            var equipments = db.Equipments.Include(e => e.Category).Include(e => e.OfferType)
                .Include(e => e.User).Where(x => x.Cancellation == false ).Where(x=>x.Name.Contains(searchtext)||x.Description.Contains(searchtext));
            return View(equipments.ToList());
        }
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public ActionResult CancelOffer(int? id)
        {
            Equipment equipment = db.Equipments.Find(id);
            equipment.Cancellation = true;
            db.Entry(equipment).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("IndexForCustomer");
        }
        // GET: Equipments
        public ActionResult Index(int?id)
        {
            if (id == null)
            {
                id = 2;
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            var equipments = db.Equipments.Include(e => e.Category).Include(e => e.OfferType).Include(e => e.User).Where(x => x.Cancellation == false && x.Category_Id == category.Id);
            

            return View(equipments.ToList());
        }

        // GET: Equipments for customer
        [Authorize(Roles = "Customer")]
        public ActionResult IndexForCustomer()
        {
            var equipments = db.Equipments.Include(e => e.Category).Include(e => e.OfferType).Include(e => e.User);
            
            foreach (User user in db.Users)
            {
                if (user.UserName == User.Identity.Name)
                {
                    var equip = equipments.Where(x => x.Cancellation == false && x.Owner_Id == user.Id);
                    return View(equip.ToList());
                }
            }

            return RedirectToAction("Login", "Users");

        }

        // GET: Equipments/Details/5
        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipment equipment = db.Equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            string message1 = TempData["message1"] as string;
            ViewBag.Message1 = message1;
            string message2 = TempData["message2"] as string;
            ViewBag.Message2 = message2;
            return View(equipment);
        }
        
        public ActionResult DetailsForRent(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipment equipment = db.Equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            string message1 = TempData["message1"] as string;
            ViewBag.Message1 = message1;
            string message2 = TempData["message2"] as string;
            ViewBag.Message2 = message2;
            return View(equipment);
        }

        // GET: Equipments/Create
        [Authorize(Roles = "Customer")]
        public ActionResult Create()
        {
            //ViewBag.Category_Id = new SelectList(db.Categories, "Id", "CategoryName");
            //ViewBag.OfferType_Id = new SelectList(db.OfferTypes, "Id", "OfferType1");
            //ViewBag.Owner_Id = new SelectList(db.Users, "Id", "UserName");
            return View();
        }

        // POST: Equipments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public ActionResult Create(string name,string description, int price, int quantity, int category_id, int offertype_id, HttpPostedFileBase img)
        {
           
                Equipment equipment = new Equipment();
                equipment.Name = name;
                equipment.Description = description;
                equipment.Price = price;
                equipment.Quantity = quantity;
                equipment.Category_Id = category_id;
                equipment.OfferType_Id = offertype_id;
                equipment.Cancellation = false;

                foreach (User user in db.Users)
                {
                    if (user.UserName == User.Identity.Name)
                    {
                        equipment.Owner_Id = user.Id;
                    }
                }
                if (img != null)
                {
                    string path = Path.Combine(Server.MapPath("~/Uploads"), img.FileName);
                    img.SaveAs(path);
                    equipment.Image = img.FileName;
                }

                
                db.Equipments.Add(equipment);
                db.SaveChanges();
                return RedirectToAction("IndexForCustomer");
            

            
        }

        // GET: Equipments/Edit/5
       

        // GET: Equipments/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipment equipment = db.Equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            return View(equipment);
        }

        // POST: Equipments/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Equipment equipment = db.Equipments.Find(id);
            equipment.Cancellation = true;
            db.Entry(equipment).State = EntityState.Modified;
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
