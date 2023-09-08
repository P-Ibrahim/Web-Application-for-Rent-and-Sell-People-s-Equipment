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
    public class RentalRequestsController : Controller
    {
        private EquipmentsDBEntities db = new EquipmentsDBEntities();

        [Authorize]
        public ActionResult Success()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Customer")]
        public ActionResult AcceptOrder(int? id)
        {
            RentalRequest rentalRequest = db.RentalRequests.Find(id);
            rentalRequest.RequestStatus = true;
            db.Entry(rentalRequest).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("IndexForCustomer");
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public ActionResult RefuseOrder(int? id)
        {
            RentalRequest rentalRequest = db.RentalRequests.Find(id);
            rentalRequest.RequestStatus = false;
            db.Entry(rentalRequest).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("IndexForCustomer");
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public ActionResult SendReceipt(int? id, HttpPostedFileBase receipt)
        {
            RentalRequest rentalRequest = db.RentalRequests.Find(id);
            if (receipt != null)
            {
                string path = Path.Combine(Server.MapPath("~/Uploads"), receipt.FileName);
                receipt.SaveAs(path);
                rentalRequest.Receipt = receipt.FileName;
            }
            db.Entry(rentalRequest).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("IndexForCustomer");
        }

        //reports for costumer
        [Authorize(Roles = "Customer")]
        public ActionResult report()
        {
            var rentalRequests = db.RentalRequests.Include(r => r.Equipment).Include(r => r.User);
            int count1 = 0;
            int count2 = 0;
            foreach (var i in rentalRequests)
            {
                if (User.Identity.Name == i.User.UserName && i.Receipt != null)
                {
                    count1 += Convert.ToInt32(i.TotalPrice);
                }
                if (User.Identity.Name == i.Equipment.User.UserName && i.Receipt != null)
                {
                    count2 += Convert.ToInt32(i.TotalPrice);
                }
            }
            ViewData["count1"] = count1;
            ViewData["count2"] = count2;
            return View(rentalRequests.ToList());
        }
        //reports for Admin
        [Authorize(Roles = "Admin")]
        public ActionResult AllReports()
        {
            var rentalRequests = db.RentalRequests.Include(s => s.Equipment).Include(s => s.User);

            return View(rentalRequests.ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AllReports(string name)
        {
            var rentalRequests = db.RentalRequests.Include(s => s.Equipment).Include(s => s.User).Where(s => s.User.UserName.Contains(name));

            return View(rentalRequests.ToList());
        }

        [Authorize(Roles = "Customer")]
        public ActionResult IndexForCustomer()
        {
            var rentalRequests = db.RentalRequests.Include(r => r.Equipment).Include(r => r.User);
            return View(rentalRequests.ToList());
        }
        // GET: RentalRequests/Details/5
        [Authorize(Roles = "Customer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RentalRequest rentalRequest = db.RentalRequests.Find(id);
            if (rentalRequest == null)
            {
                return HttpNotFound();
            }
            return View(rentalRequest);
        }

       
        // POST: RentalRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        public ActionResult Create(int? id,DateTime startDate,DateTime endDate, int number)
        {
            int rentalQuantity = 0;
            Equipment equipment = db.Equipments.Find(id);
            int equipmentQuantity = equipment.Quantity.Value;
            foreach(RentalRequest rental in db.RentalRequests)
            {
                if (rental.Equipment_Id == id)
                {
                    if ((startDate<=rental.RentalStartDate&&endDate>=rental.RentalEndDate)||(startDate>=rental.RentalStartDate&&endDate>=rental.RentalEndDate)
                        || (startDate <= rental.RentalStartDate && endDate <= rental.RentalEndDate)|| (startDate >= rental.RentalStartDate && endDate <= rental.RentalEndDate))
                    {
                        rentalQuantity += rental.Quantity.Value;
                        
                    }
                }
            }
            if ((equipmentQuantity - rentalQuantity) == 0)
            {
                TempData["message1"] = " المنتج غير متوفر !!!  جميع الكمية مؤجره في هذا التاريخ...";
                return RedirectToAction("DetailsForRent", "Equipments", new { id = id });
            }
            else
            {
                if (((equipmentQuantity - rentalQuantity) - number) < 0)
                {
                    TempData["message2"] = " الكمية المطلوبة غير متوفرة في هذا التاريخ !!!";
                    return RedirectToAction("DetailsForRent", "Equipments", new { id = id });
                }
                else {
                    RentalRequest rentalRequest = new RentalRequest();
                    foreach (User user in db.Users)
                    {
                        if (user.UserName == User.Identity.Name)
                        {
                            rentalRequest.User_Id = user.Id;
                        }
                    }
                    rentalRequest.Equipment_Id = id;
                    rentalRequest.RequestDate = DateTime.Now;
                    rentalRequest.Quantity = number;
                    rentalRequest.RentalStartDate = startDate;
                    rentalRequest.RentalEndDate = endDate;
                    TimeSpan diff = endDate - startDate;
                    int days = (int)diff.TotalDays;
                    
                    rentalRequest.TotalPrice = number * equipment.Price * days;
                    rentalRequest.Receipt = null;
                    rentalRequest.RequestStatus = null;

                    db.RentalRequests.Add(rentalRequest);
                    db.SaveChanges();
                    return RedirectToAction("Success");
                }
            }
            
            

          
        }
        [Authorize(Roles = "Customer")]
        public ActionResult DeleteConfirmed(int id)
        {
            RentalRequest rentalRequest = db.RentalRequests.Find(id);
            db.RentalRequests.Remove(rentalRequest);
            db.SaveChanges();
            return RedirectToAction("IndexForCustomer");
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
