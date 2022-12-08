using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace FirstWebApplication.Models
{
    public class UserLogin
    {
        [Display(Name = "User Name")]
        [Required(ErrorMessage = "UserName is Required..")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is Required..")]
        public string Password { get; set; }
    }
}