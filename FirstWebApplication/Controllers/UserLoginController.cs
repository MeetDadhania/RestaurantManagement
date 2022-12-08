using FirstWebApplication.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.WebPages;

namespace FirstWebApplication.Controllers
{
    public class UserLoginController : Controller
    {
        private readonly RestaurantEntities restaurantEntities = new RestaurantEntities();


        // GET: StudentLogin
        public ActionResult Index()
        {
            SetSession();
            return View();
        }

        public ActionResult LogOut()
        {
            SetSession();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Login(UserLogin userLogin)
        {
            if (ModelState.IsValid)
            {
                UserRegistration user = restaurantEntities.UserRegistrations.Where(s => s.UserName == userLogin.UserName).FirstOrDefault();
                if (user != null && DecryptPassword(user.Password) == userLogin.Password)
                {
                    Session["UserID"] = user.UserID.ToString();
                    Session["UserName"] = user.UserName;

                    return RedirectToAction("DashBoard");
                }
                else
                {
                    TempData["ErrorMessage"] = "UserName or Password is incorrect";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult DashBoard(int? page)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Error");
            }
            ViewBag.mealTypes = GetMealTypesList();
            ViewBag.Action = "DashBoard";
            var menu = restaurantEntities.MenuDetails.ToList().ToPagedList(page ?? 1, 6);
            return View(menu);
        }

        public ActionResult NameSearch(string searchString ,int? page = 1)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Error");
            }
            if(searchString == null || searchString.IsEmpty())
            {
                return RedirectToAction("DashBoard");
            }
            ViewBag.mealTypes = GetMealTypesList();
            ViewBag.Action = "NameSearch";
            var menu = restaurantEntities.MenuDetails.Where(s => s.Name.Contains(searchString)).ToList().ToPagedList(page ?? 1, 6);
            return View("DashBoard",menu);

        }

        public ActionResult TypeSearch(int? MealType,int? page)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Error");
            }
            ViewBag.mealTypes = GetMealTypesList();
            ViewBag.Action = "TypeSearch";
            if (MealType == null)
            {
                return RedirectToAction("DashBoard");
            }
            else
            {
                var menu = restaurantEntities.MenuDetails.Where(s => s.TypeID == MealType).ToList().ToPagedList(page ?? 1, 6);
                return View("DashBoard", menu);
            }
        }

        public ActionResult Error()
        {
            return View();
        }

        private void SetSession()
        {
            Session["UserName"] = null;
            Session["UserID"] = null;
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

        private string DecryptPassword(string password)
        {
            StringBuilder encryptedPassword = new StringBuilder(password);
            for (int i = 0; i < password.Length; i++)
            {
                if (i % 2 != 0)
                {
                    encryptedPassword[i] = (char)(((int)password[i]) + 1);
                }
                else
                {
                    encryptedPassword[i] = (char)(((int)password[i]) - 1);
                }
            }
            return encryptedPassword.ToString();
        }
    }
}