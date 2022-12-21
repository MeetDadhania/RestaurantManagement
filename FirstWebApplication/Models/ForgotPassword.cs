using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace FirstWebApplication.Models
{
    public class ForgotPassword
    {
        [Display(Name = "User Name")]
        [Required(ErrorMessage = "UserName is Required..")]
        [Remote("checkUserExist", "User", ErrorMessage = "User name not exists..")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "length must between 5 to 20..")]
        [RegularExpression(("[^ ]+$"), ErrorMessage = "Space is not allowed")]
        public string UserName { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Confirm Password is Required..")]
        [Compare("Password", ErrorMessage = "Password Must match...")]
        [RegularExpression(("[^ ]+$"), ErrorMessage = "Space is not Allow")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Password is Required..")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "length must between 6 to 20..")]
        [RegularExpression(("[^ ]+$"), ErrorMessage = "Space is not allowed")]
        public string Password { get; set; }
    }
}