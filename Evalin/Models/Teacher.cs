using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Evalin.Models
{
    public class Teacher :
        User
    {

        //[Display(Name = "ID")]
        //public String Id { get; set; }

        //[Display(Name = "Full Name")]
        //public String Fullname { get; set; }

        //[Display(Name = "Username")]
        //public String Username { get; set; }

        //[Display(Name = "Password")]
        //public String Password { get; set; }

        //[Display(Name = "Type")]
        //public String Type { get; set; }

        //public String Age { get; set; }

        //public string Image { get; set; }


       
    }

    public class Course
    {

        [Display(Name = "Course Id")]
        public int Course_Id { get; set; }

        [Display(Name = "Course Name")]
        public String Name { get; set; }

        [Display(Name = "Date Created")]
        public String Date { get; set; }

        [Display(Name = "Exam Details")]
        public Exam[] exam { get; set; }

        [Display(Name = "Enroll ID")]
        public String Enroll_id { get; set; }

        [Display(Name = "Enroll Password")]
        public String Enroll_Password { get; set; }

        public String Status { get; set; }

        public String Description { get; set; }
        
    }


    public class Exam
    {

        [Display(Name = "Exam ID")]
        public int Exam_Id { get; set; }

        [Display(Name = "Course ID")]
        public int Course_Id { get; set; }

        [Display(Name = "Exam Name")]
        public String Exam_Name { get; set; }
     
        [Display(Name = "Total Marks")]
        public double Total_Marks { get; set; }

        [Display(Name = "Passing Marks")]
        public double Passing_Marks { get; set; }

        public Section[] Sections { get; set; }

        public DateTime Exam_Date { get; set; }

        public DateTime Start_Time { get; set; }

        public DateTime End_Time { get; set; }

        public TimeSpan Duration { get; set; }

        public Boolean AutoSubmit { get; set; }

        public String Temp { get; set; }

    }


    public class Section {

        public int Section_Id { get; set; }

        public int Exam_Id { get; set; }

        [AllowHtml]
        [Display(Name = "Section Text")]
        public string Section_Text { get; set; }

        [Display(Name = "Marks")]
        public double Section_Marks { get; set; }

        [Display(Name = "Order")]
        public int Section_Order { get; set; }

        public Question[] Questions { get; set; }

    }

    public class Question{
    
        public int Question_Id { get; set; }
    
        public int Exam_Id { get; set; }
        
        [AllowHtml]
        [Display(Name = "Question")]
        public string QuestionText { get; set; }

        [AllowHtml]
        [Display(Name = "Sample Answer")]
        public string Sample_Answer { get; set; }
 
        [Display(Name = "Question Marks")]
        public double Question_Marks { get; set; }

        [Display(Name = "Obtained Marks")]
        public double Obtained_Marks { get; set; }

        [Display(Name = "Order")]
        public int Question_Order { get; set; }

        [Display(Name = "Options")]
        public string[] Options { get; set; }

        [Display(Name = "Type")]
        public string Type { get; set; }

     
    }


    public class CourseDetail {

      public Course Course { get; set; }
      public EnrolledStudent[] Students { get; set; }
      public String Teacher_Name { get; set; }
      public int Strength { get; set; }
    }


    public class Result {

        public String ResultId { get; set; }
        public String Exam_Name { get; set; }
        public String Course_Name { get; set; }
        public String Student_Name { get; set; }
        public String StudentId { get; set; }
        public double Obtain_Marks { get; set; }
        public double Percentage { get; set; }
        public double Total_Marks { get; set; }
        public String Grade { get; set; }
        public String Remarks { get; set; }
    
    }
   
}