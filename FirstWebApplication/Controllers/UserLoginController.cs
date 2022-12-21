using FirstWebApplication.Models;
using NLog;
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
        #region private readonly variables
        private readonly RestaurantEntities restaurantEntities = new RestaurantEntities();
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        // GET: StudentLogin

        #region index method return login view
        public ActionResult Index()
        {
            SetSession();
            ViewBag.ReisterMessage = TempData["ReisterMessage"];
            ViewBag.ForgotPassword = TempData["ForgotPassword"];
            ViewBag.PasswordReset = TempData["PasswordReset"];

            return View();
        }
        #endregion

        #region logout method
        public ActionResult LogOut()
        {
            SetSession();
            return RedirectToAction("Index");
        }
        #endregion

        #region User login method
        [HttpPost]
        public ActionResult Login(UserLogin userLogin)
        {
            try
            {
                //check model state is valid
                if (ModelState.IsValid)
                {
                    //get user details from DB
                    UserRegistration user = restaurantEntities.UserRegistrations.Where(s => s.UserName == userLogin.UserName).FirstOrDefault();

                    //check user exist, password match, user is verified
                    if (user != null && DecryptPassword(user.Password) == userLogin.Password && user.IsVerified == true)
                    {
                        //set the session 
                        Session["UserID"] = user.UserID.ToString();
                        Session["UserName"] = user.UserName;

                        //redirect to dashboard
                        return RedirectToAction("DashBoard");
                    }
                    //if user is not verified
                    else if (user.IsVerified == false)
                    {
                        //redirect to login page with message
                        TempData["ErrorMessage"] = "Account is not Verified..";
                        return RedirectToAction("Index");
                    }
                    //if user doesn't exist 
                    else
                    {
                        //redirect to login page with message
                        TempData["ErrorMessage"] = "UserName or Password is incorrect";
                        return RedirectToAction("Index");
                    }
                }
                //if model is invalid redirect to login page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "UserLoginController Login[Post] Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region DashBoard method
        public ActionResult DashBoard(int? page)
        {
            //check user login or not
            if (Session["UserName"] == null)
            {
                //if user not login redirect to error page with message
                ViewBag.errorMessage = "Please Login to continue...";
                return View("Error");
            }
            try
            {
                //get meal type from database 
                ViewBag.mealTypes = GetMealTypesList();
                ViewBag.Action = "DashBoard";

                //add messages to viewbag

                ViewBag.deleteMessage = TempData["DeleteMessage"];

                ViewBag.addMessage = TempData["addMessage"];

                ViewBag.editMessage = TempData["editMessage"];


                //get menu details from DB and return dashboard view
                var menu = restaurantEntities.MenuDetails.OrderByDescending(item => item.CreatedOn).ToList().ToPagedList(page ?? 1, 6);
                return View(menu);
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "UserLoginController DashBoard Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region search method 
        public ActionResult SearchFunction(int? MealType, string searchString, int? page)
        {
            //check user login or not
            if (Session["UserName"] == null)
            {
                ViewBag.errorMessage = "Please Login to continue...";
                return View("Error");
            }
            try
            {
                //get meal type from DB
                ViewBag.mealTypes = GetMealTypesList();
                ViewBag.Action = "SearchFunction";

                //return menu details list based on search input and return dashboard view

                //without search anything
                if (MealType == null && (searchString == null || searchString.IsEmpty()))
                {
                    return RedirectToAction("DashBoard");
                }
                //search by name
                else if (MealType == null)
                {
                    var menu = restaurantEntities.MenuDetails.Where(s => s.Name.Contains(searchString)).OrderByDescending(item => item.CreatedOn).ToList().ToPagedList(page ?? 1, 6);
                    return View("DashBoard", menu);
                }
                //search by meal type
                else if (searchString == null || searchString.IsEmpty())
                {
                    var menu = restaurantEntities.MenuDetails.Where(s => s.TypeID == MealType).OrderByDescending(item => item.CreatedOn).ToList().ToPagedList(page ?? 1, 6);
                    return View("DashBoard", menu);
                }
                //search by meal type and name both
                else
                {
                    var menu = restaurantEntities.MenuDetails.Where(s => s.TypeID == MealType).Where(s => s.Name.Contains(searchString)).OrderByDescending(item => item.CreatedOn).ToList().ToPagedList(page ?? 1, 6);
                    return View("DashBoard", menu);
                }
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "UserLoginController SearchFunction Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region error page method
        public ActionResult Error()
        {
            return View();
        }
        #endregion

        #region clear user session
        private void SetSession()
        {
            Session["UserName"] = null;
            Session["UserID"] = null;
        }
        #endregion

        #region select list of meal type from DB
        private SelectListItem[] GetMealTypesList()
        {
            SelectListItem[] mealTypes = null;

            //get meal type from DB
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
        #endregion

        #region password decryption algo
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
        #endregion
    }
}