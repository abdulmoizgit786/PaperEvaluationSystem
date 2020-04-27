using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Evalin.Models
{
    public class User 
    {

        [Display(Name = "User ID")]
        public String Id { get; set; }

        [Required(ErrorMessage = "Name is Required")]
        [StringLength(50,ErrorMessage="Name Length Exceeds")]
        [Display(Name = "Full Name")]
        public String Fullname { get; set; }

        [Required(ErrorMessage="Username is Required")]
        [StringLength(40, ErrorMessage = "Username Length Exceeds")]
        [Display(Name = "Username")]
        public String Username { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [StringLength(40, ErrorMessage = "Password Length Exceeds")]
        [Display(Name = "Password")]
        public String Password { get; set; }

        [Display(Name = "Type")]
        public String type { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Date Of Birth")]
        public DateTime DOB { get; set; }

        [Required (ErrorMessage="Enter Age")]
        [Range(5,80,ErrorMessage="Age Between 5~80")]
        [Display(Name = "Age")]
        public int Age { get; set; }

        [Display(Name = "Remember Me")]
        public Boolean remember { get; set; }

        [Display(Name = "Image")]
        public HttpPostedFileBase Image { get; set; }

        [Display(Name = "ImageLocation")]
        public string ImageLocation { get; set; }

        [Required(ErrorMessage="Email Address is Required")]
        [EmailAddress(ErrorMessage = "Incorrect Email Format")]
        public string Emailaddress { get; set; }

       

    }
}