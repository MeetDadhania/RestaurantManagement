using Antlr.Runtime;
using FirstWebApplication;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FirstWebApplication.Controllers
{
    public class UserController : Controller
    {
        private readonly RestaurantEntities restaurantEntities = new RestaurantEntities();

        // GET: Student
        public ActionResult Index()
        {
            SetSession();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(UserRegistration user)
        {
            SetSession();
            if (ModelState.IsValid)
            {
                try
                {
                    user.Password = EncryptPassword(user.Password);
                    user.ConfirmPassword = EncryptPassword(user.ConfirmPassword);
                    restaurantEntities.UserRegistrations.Add(user);
                    await restaurantEntities.SaveChangesAsync();
                    return View(user);
                }
                catch (DbEntityValidationException dbEx)
                {
                    throw dbEx;
                }
            }
            return RedirectToAction("Index");
        }

        private void SetSession()
        {
            Session["UserName"] = null;
            Session["UserID"] = null;
        }

        public JsonResult doesUserNameExist(string UserName)
        {
            return Json(!restaurantEntities.UserRegistrations.Any(x => x.UserName == UserName), JsonRequestBehavior.AllowGet);
        }

        private string EncryptPassword(string password)
        {
            StringBuilder encryptedPassword = new StringBuilder(password);
            for (int i = 0; i < password.Length; i++)
            {
                if (i % 2 == 0)
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