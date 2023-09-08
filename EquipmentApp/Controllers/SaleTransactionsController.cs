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
    public class SaleTransactionsController : Controller
    {
        private EquipmentsDBEntities db = new EquipmentsDBEntities();

        [Authorize]
        public ActionResult Success()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public ActionResult AcceptOrder(int?id)
        {
            SaleTransaction saleTransaction = db.SaleTransactions.Find(id);
            saleTransaction.RequestStatus = true;
            db.Entry(saleTransaction).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("IndexForCustomer");
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public ActionResult RefuseOrder(int? id)
        {
            SaleTransaction saleTransaction = db.SaleTransactions.Find(id);
            saleTransaction.RequestStatus = false;
            db.Entry(saleTransaction).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("IndexForCustomer");
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public ActionResult SendReceipt(int? id, HttpPostedFileBase receipt)
        {
            SaleTransaction saleTransaction = db.SaleTransactions.Find(id);
            if (receipt != null)
            {
                string path = Path.Combine(Server.MapPath("~/Uploads"), receipt.FileName);
                receipt.SaveAs(path);
                saleTransaction.Receipt = receipt.FileName;
            }
            db.Entry(saleTransaction).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("IndexForCustomer");
        }
        //reports for customer
        [Authorize(Roles = "Customer")]
        public ActionResult Report()
        {
            var saleTransactions = db.SaleTransactions.Include(s => s.Equipment).Include(s => s.User);
            int count1 = 0;
            int count2 = 0;
            foreach (var i in saleTransactions)
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
            return View(saleTransactions.ToList());
        }

        //reports for admin
        [Authorize(Roles = "Admin")]
        public ActionResult AllReports()
        {
            var saleTransactions = db.SaleTransactions.Include(s => s.Equipment).Include(s => s.User);
                
            return View(saleTransactions.ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AllReports(string name)
        {
            var saleTransactions = db.SaleTransactions.Include(s => s.Equipment).Include(s => s.User).Where(s=>s.User.UserName.Contains(name));
           
            return View(saleTransactions.ToList());
        }
      
        [Authorize(Roles = "Customer")]
        public ActionResult IndexForCustomer()
        {
            var saleTransactions = db.SaleTransactions.Include(s => s.Equipment).Include(s => s.User);
            return View(saleTransactions.ToList());
        }

        // GET: SaleTransactions/Details/5
        [Authorize(Roles = "Customer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SaleTransaction saleTransaction = db.SaleTransactions.Find(id);
            if (saleTransaction == null)
            {
                return HttpNotFound();
            }
            return View(saleTransaction);
        }

        
       

        // POST: SaleTransactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        public ActionResult Create(int? id,int number)
        {
            int soldQuantity = 0;
            Equipment equipment = db.Equipments.Find(id);
            int equipmentQuantity= equipment.Quantity.Value ;
            foreach(SaleTransaction sale in db.SaleTransactions)
            {
                if (sale.Equipment_Id == id)
                {
                    soldQuantity = soldQuantity + sale.Quantity.Value;
                   
                }
            }
            if ((equipmentQuantity - soldQuantity) == 0)
            {
                TempData["message1"] = " المنتج غير متوفر !!!  جميع الكمية تم بيعها مسبقا...";
                return RedirectToAction("Details","Equipments",new { id = id });
            }
            
            else
            {
                if (((equipmentQuantity - soldQuantity)-number) < 0)
                {
                    TempData["message2"] = " الكمية المطلوبة غير متوفرة !!!";
                    return RedirectToAction("Details", "Equipments", new { id = id });
                }
                else
                {
                    SaleTransaction saleTransaction = new SaleTransaction();
                    foreach (User user in db.Users)
                    {
                        if (user.UserName == User.Identity.Name)
                        {
                            saleTransaction.User_Id = user.Id;
                        }
                    }
                    saleTransaction.Equipment_Id = id;
                    saleTransaction.TransactionDate = DateTime.Now;
                    saleTransaction.Quantity = number;
                    
                    saleTransaction.TotalPrice = number * equipment.Price;
                    saleTransaction.Receipt = null;
                    saleTransaction.RequestStatus = null;

                    db.SaleTransactions.Add(saleTransaction);
                    db.SaveChanges();
                    return RedirectToAction("Success");
                }
               
            }
            
        }
        [Authorize(Roles = "Customer")]
        public ActionResult DeleteConfirmed(int id)
        {
            SaleTransaction saleTransaction = db.SaleTransactions.Find(id);
            db.SaleTransactions.Remove(saleTransaction);
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
