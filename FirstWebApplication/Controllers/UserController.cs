using Antlr.Runtime;
using FirstWebApplication;
using FirstWebApplication.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace FirstWebApplication.Controllers
{
    public class UserController : Controller
    {
        #region private readonly variables
        private readonly RestaurantEntities restaurantEntities = new RestaurantEntities();
        private readonly DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion 

        // GET: Student
        #region Index Method for return register form view
        public ActionResult Index()
        {
            //clear user session
            SetSession();

            ViewBag.RegisterMessage = TempData["RegisterMessage"];

            //return register page
            return View();
        }
        #endregion

        #region Register User method
        [HttpPost]
        public async Task<ActionResult> Create(UserRegistration user)
        {
            //clear user session
            SetSession();
            try
            {
                //validate the modal state
                if (ModelState.IsValid)
                {
                    //set user properties and encrypte the password
                    user.Password = EncryptPassword(user.Password);
                    user.ConfirmPassword = EncryptPassword(user.ConfirmPassword);
                    user.ActivationCode = Guid.NewGuid();

                    //send verification mail to user for account activation
                    SendVerificationEmail(user.Email, user.UserName, user.ActivationCode.ToString());

                    //add and save the user details to database
                    restaurantEntities.UserRegistrations.Add(user);
                    await restaurantEntities.SaveChangesAsync();

                    //return register page with message 
                    TempData["RegisterMessage"] = "Account Created Successfully. Please Active your account from link send to mail.";

                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "UserController Create Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region User account verification
        public ActionResult VarifyAccount(string activationCode)
        {
            try
            {
                // get user details from database based on activation code
                UserRegistration user = restaurantEntities.UserRegistrations.Where(s => s.ActivationCode.ToString() == activationCode).FirstOrDefault();
                if (user != null)
                {
                    //add success message
                    TempData["ReisterMessage"] = "Success";

                    //in user details make isverified field true
                    user.IsVerified = true;
                    user.ConfirmPassword = user.Password;

                    //save the changes in databse
                    restaurantEntities.SaveChanges();

                    //redirect to login page
                    return RedirectToAction("Index", "UserLogin");
                }
                else
                {
                    //if user not exist return error page with message
                    ViewBag.errorMessage = "Invalid URL";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "UserController VarifyAccount Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region send forgot password mail to user
        public ActionResult GetForgotPasswordUserName(ForgotPassword forgotUser)
        {
            try
            {
                //get user data from database 
                UserRegistration user = restaurantEntities.UserRegistrations.Where(s => s.UserName == forgotUser.UserName).FirstOrDefault();

                //send forgot password mail to user
                sendForgotPasswordEmail(user);

                //redirect to login page with message
                TempData["ForgotPassword"] = "EmailSend";
                return RedirectToAction("Index", "UserLogin");
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "UserController GetForgotPasswordUserName Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region get user and redirect to reset password page
        public ActionResult ForgotPassword(string activationCode, string token)
        {
            try
            {
                //get user details from database
                UserRegistration user = restaurantEntities.UserRegistrations.Where(s => s.ActivationCode.ToString() == activationCode).FirstOrDefault();
                
                //check is user exist
                if (user != null)
                {
                    //calculate the time when between URL send and use
                    TimeSpan linkTime = TimeSpan.Parse(token);
                    double timeDifference = (double)((DateTime.Now - baseDate).TotalMinutes - (linkTime.TotalMinutes));
                    
                    //check the time difference if time is > 5 min URL is expire
                    if (timeDifference < 5.0)
                    {
                        // get user name from user details
                        ForgotPassword forgotUser = new ForgotPassword();
                        forgotUser.UserName = user.UserName;

                        //return the reset password view
                        return View(forgotUser);
                    }
                    else
                    {
                        //if url expire redirect to error page with message
                        ViewBag.errorMessage = "Link is expired..";
                        return View("Error");
                    }
                }
                else
                {
                    //return error page with message
                    ViewBag.errorMessage = "Invalid Url...";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "UserController ForgotPassword Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region reset the password in DB
        [HttpPost]
        public ActionResult ForgotPassword(ForgotPassword forgotUser)
        {
            try
            {
                //get user details form DB
                UserRegistration user = restaurantEntities.UserRegistrations.Where(s => s.UserName == forgotUser.UserName).FirstOrDefault();
                
                //change the password field
                user.Password = EncryptPassword(forgotUser.Password);
                user.ConfirmPassword = EncryptPassword(forgotUser.Password);

                //save the changes to DB
                restaurantEntities.SaveChanges();

                //return to login page with message
                TempData["PasswordReset"] = "ResetSuccess";
                return RedirectToAction("Index", "UserLogin");
            }
            catch (Exception ex)
            {
                //log the error message in log file
                logger.Error(ex, "UserController ForgotPassword[Post] Action");

                //return the error view with message
                ViewBag.errorMessage = "Something went wrong please try again..";
                return View("Error");
            }
        }
        #endregion

        #region clear user session
        private void SetSession()
        {
            Session["UserName"] = null;
            Session["UserID"] = null;
        }
        #endregion

        //data field validation for user name
        public JsonResult doesUserNameExist(string UserName)
        {
            return Json(!restaurantEntities.UserRegistrations.Any(x => x.UserName == UserName), JsonRequestBehavior.AllowGet);
        }

        //data field validation for user name
        public JsonResult checkUserExist(string UserName)
        {
            return Json(restaurantEntities.UserRegistrations.Any(x => x.UserName == UserName), JsonRequestBehavior.AllowGet);
        }

        #region password encryption
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
        #endregion

        #region send account verification mail
        [NonAction]
        private void SendVerificationEmail(string email, string userName, string activationCode)
        {
            //declaration of variable and assign the value 
            var url = "/User/VarifyAccount/" + activationCode;
            var VerificationLink = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, url);

            var fromEmail = new MailAddress("fun.786.beast@gmail.com", "The Imperial Spice");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "*****************";
            string subject = "The Imperial Spice Account Varification..";
            string body = "<br/> Congratulation " + userName + "<br/> Your Imperial spice account is successfully created." +
                "Please click on the below link to active your account." +
                "<br/><a href='" + VerificationLink + "'>Active Account</a>";

            //smtp cliend configuration
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            //send the mail
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
        #endregion

        #region send forgot password mail
        [NonAction]
        private void sendForgotPasswordEmail(UserRegistration user)
        {
            //Declaration of variable and assign value
            TimeSpan token = DateTime.Now - baseDate;
            var url = "/User/ForgotPassword/" + user.ActivationCode + "?token=" + token;
            var VerificationLink = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, url);

            var fromEmail = new MailAddress("fun.786.beast@gmail.com", "The Imperial Spice");
            var toEmail = new MailAddress(user.Email);
            var fromEmailPassword = "*****************";
            string subject = "The Imperial Spice Forgot Password..";
            string body = "<br/> Hello " + user.UserName + "<br/> Click below link to reset your password. This link will expire in 5 Minutes." +
                "<br/><a href='" + VerificationLink + "'>Reset Password</a>";

            //smtp cliend configuration
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            //send the mail
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
        #endregion

    }
}