using NLog;
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
        #region private readonly variables
        private readonly RestaurantEntities restaurantEntities = new RestaurantEntities();
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region add menu item method
        [HttpPost]
        public async Task<ActionResult> Add(MenuDetail menuDetail)
        {
            //check user login or not
            if (!CheckLogin())
            {
                //return error view with message
                ViewBag.errorMessage = "Please login to continue..";
                return View("Error");
            }
            try
            {
                //validate model 
                if (ModelState.IsValid)
                {
                    //get the image file from form
                    HttpPostedFileBase imageFile = Request.Files["ImageData"];

                    //if input is not null convert it to byte array
                    if (imageFile != null)
                    {
                        menuDetail.Image = ConvertToBytes(imageFile);
                    }

                    //add log details user name and time
                    menuDetail.CreatedBy = Session["UserName"].ToString();
                    menuDetail.CreatedOn = DateTime.Now;

                    //add the details to DB and save it
                    restaurantEntities.MenuDetails.Add(menuDetail);
                    await restaurantEntities.SaveChangesAsync();

                    //redirect to dashboard action
                    TempData["addMessage"] = "Added";
                    return RedirectToAction("DashBoard", "UserLogin");
                }
                //if model is invalid redirect to dashboard with error message

                ViewBag.mealTypes = GetMealTypesList();
                TempData["addMessage"] = "Error";
                return RedirectToAction("DashBoard", "UserLogin");
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "MenuItemController Add[Post] Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region edit method 
        //get menu item from DB by id
        public ActionResult Edit(int Id,int? page)
        {
            //check user login or not
            if (!CheckLogin())
            {
                ViewBag.errorMessage = "Please login to continue..";
                return View("Error");
            }
            try
            {
                //get item from DB
                MenuDetail item = restaurantEntities.MenuDetails.Where(i => i.ItemID.Equals(Id)).FirstOrDefault();
                if (item != null)
                {
                    //return partialview with form to edit item details
                    ViewBag.mealTypes = GetMealTypesList();
                    ViewBag.ActionMethod = "Edit";
                    ViewBag.page = page;
                    return PartialView("AddEditMenuItem", item);
                }
                //it item not found in DB return error
                else
                {
                    ViewBag.errorMessage = "No item found in Database";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "MenuItemController Edit Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region edit[Post] method
        [HttpPost]
        public async Task<ActionResult> Edit(MenuDetail item,int pageNum)
        {
            //check user login or not
            if (!CheckLogin())
            {
                //return error page with message
                ViewBag.errorMessage = "Please login to continue..";
                return View("Error");
            }
            try
            {
                //check model is valid or not
                if (ModelState.IsValid)
                {
                    //get the image file from form
                    HttpPostedFileBase imageFile = Request.Files["ImageData"];

                    //get item details from DB
                    MenuDetail menuDetail = restaurantEntities.MenuDetails.Where(i => i.ItemID.Equals(item.ItemID)).FirstOrDefault();

                    //update the fields
                    menuDetail.Name = item.Name;
                    menuDetail.Price = item.Price;

                    //if image data is not null convert it to byte array
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

                    //save the DB
                    await restaurantEntities.SaveChangesAsync();

                    //redirect to dashboard with success message
                    TempData["editMessage"] = "Edited";
                    return RedirectToAction("DashBoard", "UserLogin", new { page = pageNum });
                }
                //if model is not valid redirect to dashboard with error message 
                TempData["editMessage"] = "Error";
                return RedirectToAction("DashBoard", "UserLogin");
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "MenuItemController Edit[Post] Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region delete method
        public async Task<ActionResult> Delete(int Id,int? page)
        {
            //check user login or not
            if (!CheckLogin())
            {
                ViewBag.errorMessage = "Please login to continue..";
                return View("Error");
            }
            try
            {
                //get item from DB by ID
                MenuDetail removeItem = restaurantEntities.MenuDetails.Find(Id);
                if (removeItem != null)
                {

                    //remove item and save the DB
                    restaurantEntities.MenuDetails.Remove(removeItem);
                    await restaurantEntities.SaveChangesAsync();

                    //redirect to Dashboard with success message
                    TempData["DeleteMessage"] = "Deleted";
                    return RedirectToAction("DashBoard", "UserLogin", new { page = page });
                }
                //if item not found in DB return error
                else
                {
                    ViewBag.errorMessage = "No item found in Database";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "MenuItemController Delete Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region get meal type list from DB
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
        #endregion

        #region conver file to byte array
        private byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(image.InputStream);
            imageBytes = reader.ReadBytes((int)image.ContentLength);
            return imageBytes;
        }
        #endregion

        #region Error method
        public ActionResult Error()
        {
            return View();
        }
        #endregion

        #region user login check
        private Boolean CheckLogin()
        {
            if (Session["UserName"] == null)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region clear user session
        private void SetSession()
        {
            Session["UserName"] = null;
            Session["UserID"] = null;
        }
        #endregion
    }
}