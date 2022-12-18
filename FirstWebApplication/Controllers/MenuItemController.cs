using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace FirstWebApplication.Controllers
{
    public class MenuItemController : Controller
    {
        private readonly RestaurantEntities restaurantEntities = new RestaurantEntities();

        // GET: MenuItem
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            if (!CheckLogin())
            {
                return RedirectToAction("Error");
            }
            ViewBag.mealTypes = GetMealTypesList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Add(MenuDetail menuDetail)
        {
            if (!CheckLogin())
            {
                return RedirectToAction("Error");
            }
            if (ModelState.IsValid)
            {
                HttpPostedFileBase imageFile = Request.Files["ImageData"];
                if (imageFile != null)
                {
                    menuDetail.Image = ConvertToBytes(imageFile);
                }
                menuDetail.CreatedBy = Session["UserName"].ToString();
                menuDetail.CreatedOn = DateTime.Now;
                restaurantEntities.MenuDetails.Add(menuDetail);
                await restaurantEntities.SaveChangesAsync();
                TempData["addMessage"] = "Added";
                return RedirectToAction("DashBoard", "UserLogin");
            }
            ViewBag.mealTypes = GetMealTypesList();
            TempData["addMessage"] = "Error";
            return RedirectToAction("DashBoard", "UserLogin");
        }

        public ActionResult Edit(int Id,int? page)
        {
            if (!CheckLogin())
            {
                return RedirectToAction("Error");
            }
            MenuDetail item = restaurantEntities.MenuDetails.Where(i => i.ItemID.Equals(Id)).FirstOrDefault();
            ViewBag.mealTypes = GetMealTypesList();
            ViewBag.ActionMethod = "Edit";
            ViewBag.page = page;
            return PartialView("AddEditMenuItem",item);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(MenuDetail item,int pageNum)
        {
            if (!CheckLogin())
            {
                return RedirectToAction("Error");
            }
            if (ModelState.IsValid)
            {
                HttpPostedFileBase imageFile = Request.Files["ImageData"];
                MenuDetail menuDetail = restaurantEntities.MenuDetails.Where(i => i.ItemID.Equals(item.ItemID)).FirstOrDefault();
                menuDetail.Name = item.Name;
                menuDetail.Price = item.Price;
                if (imageFile.ContentLength != 0)
                {
                    menuDetail.Image = ConvertToBytes(imageFile);
                }
                else
                {
                    menuDetail.Image = item.Image;
                }
                menuDetail.Veg = item.Veg;
                menuDetail.ModifiedBy = Session["UserName"].ToString();
                menuDetail.ModifiedOn = DateTime.Now;
                await restaurantEntities.SaveChangesAsync();
                TempData["editMessage"] = "Edited";
                return RedirectToAction("DashBoard", "UserLogin", new { page = pageNum });
            }
            TempData["editMessage"] = "Error";
            return RedirectToAction("DashBoard", "UserLogin");
        }

        public async Task<ActionResult> Delete(int Id,int? page)
        {
            if (!CheckLogin())
            {
                return RedirectToAction("Error");
            }
            MenuDetail removeItem = restaurantEntities.MenuDetails.Find(Id);
            restaurantEntities.MenuDetails.Remove(removeItem);
            await restaurantEntities.SaveChangesAsync();
            TempData["DeleteMessage"] = "Deleted";
            return RedirectToAction("DashBoard", "UserLogin",new { page = page});
        }

        private SelectListItem[] GetMealTypesList()
        {
            SelectListItem[] mealTypes = null;

            var items = restaurantEntities.MealTypes.Select(a => new SelectListItem()
            {
                Text = a.MealType1,
                Value = a.TypeID.ToString()
            }).ToList();

            mealTypes = items.ToArray();


            // Have to create new instances via projection
            // to avoid ModelBinding updates to affect this
            // globally
            return mealTypes
                .Select(d => new SelectListItem()
                {
                    Value = d.Value,
                    Text = d.Text
                })
             .ToArray();
        }

        private byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(image.InputStream);
            imageBytes = reader.ReadBytes((int)image.ContentLength);
            return imageBytes;
        }

        public ActionResult Error()
        {
            return View();
        }

        private Boolean CheckLogin()
        {
            if (Session["UserName"] == null)
            {
                return false;
            }
            return true;
        }

        private void SetSession()
        {
            Session["UserName"] = null;
            Session["UserID"] = null;
        }
    }
}