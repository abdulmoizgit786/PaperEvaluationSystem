using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Evalin.Models
{
    public class Student :
        User

    {
    //    [Display(Name = "user ID")]
    //    public String Id { get; set; }

    //    [Display(Name = "Full Name")]
    //    public String Fullname { get; set; }

    //    [Required(ErrorMessage="Please Enter Username" )]
    //    [Display(Name = "Username")]
    //    public String Username { get; set; }

    //    [Required(ErrorMessage = "Please Enter Password")]
    //    [Display(Name = "Password")]
    //    public String Password { get; set; }

    //    [Display(Name = "Type")]
    //    public String Type { get; set; }

    //    [Display(Name = "Image")]
    //    public String Image { get; set; }

    }


    public class EnrolledStudent : Student {

        public DateTime Enrollement_Date { get; set; }
        public String Status { get; set; }
        public int Course_Id { get; set; }
    }

}