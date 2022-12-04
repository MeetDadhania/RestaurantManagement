using FirstWebApplication.Models;
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
using System.Web.UI.WebControls;

namespace FirstWebApplication.Controllers
{
    public class UserLoginController : Controller
    {
        private readonly RestaurantEntities restaurantEntities = new RestaurantEntities();


        // GET: StudentLogin
        public ActionResult Index()
        {
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
                UserRegistration user = restaurantEntities.UserRegistrations.ToList()   .Where(s => s.UserName == userLogin.UserName && DecryptPassword(s.Password) == userLogin.Password).FirstOrDefault();
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



        public ActionResult DashBoard(string searchString = "")
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Error");
            }
            ViewBag.mealTypes = GetMealTypesList();
            List<MenuDetail> menu = restaurantEntities.MenuDetails.Where(s => s.Name.Contains(searchString)).ToList();
            TempData["MenuItems"] = menu;
            return View(menu);

        }

        [HttpPost]
        public ActionResult DashBoard(int? MealType)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Error");
            }
            List<MenuDetail> menu = new List<MenuDetail>();
            ViewBag.mealTypes = GetMealTypesList();
            if (MealType == null)
            {
                menu = restaurantEntities.MenuDetails.ToList();
                return View(menu);
            }
            menu = restaurantEntities.MenuDetails.Where(s => s.TypeID == MealType).ToList();
            return View(menu);
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