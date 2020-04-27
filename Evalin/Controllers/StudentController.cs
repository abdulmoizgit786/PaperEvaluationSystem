using Evalin.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Evalin.Controllers
{
    public class StudentController : Controller
    {

        [MyCustomFilter]
        [HttpGet]
        public ActionResult Profile()
        {
            return View("~/Views/Shared/Profile.cshtml");
        }

        [MyCustomFilter]
        [HttpGet]
        public ActionResult Dashboard()
        {
            //Dashboard d = new Dashboard() { user = (Student)Session["CurrentUser"], Classes = DataAccess.GetClasses(((Student)Session["CurrentUser"]).Id, ((Student)Session["CurrentUser"]).type) };
            //return View(d);
            return View();
        }

        [MyCustomFilter]
        [HttpGet]
        public ActionResult Exam(int Eid)
        {

            try
            {
                String Sid = ((Student)Session["CurrentUser"]).Id;
                Exam exam = new Exam();
                DataTable ValidateStudent = DataAccess.daobj("select Top(1) * from Courses as C join Exam as E on C.Course_Id = E.Course_Id join Enrollement as En on C.Course_Id = En.Course_Id  join Student as S on En.Student_Id = S.id  where E.Exam_Id = " + Eid.ToString() + " and S.id = '" + Sid + "' ");
                DataTable Attempt = DataAccess.daobj("select End_Time from Exam_Log where Student_Id = '" + Sid + "' and Exam_Id = " + Eid + "");
                DataTable dtexam = DataAccess.daobj("select Top(1) *  ,Cast(Concat(Exam_Date,' ',End_Time) as datetime) as End_DateTime , Cast(Concat(Exam_Date,' ',Start_Time) as datetime) as Start_DateTime from Exam where Exam_Id = " + Eid.ToString() + "");
                if (dtexam.Rows.Count > 0 && ValidateStudent.Rows.Count > 0)
                {
                    if (Attempt.Rows.Count > 0)
                    {
                        if (Attempt.Rows[0]["End_Time"] != DBNull.Value)
                        {
                            ViewBag.ErrorHeading = "You Have Already Attempt this Exam";
                            return View("~/Views/Shared/Error.cshtml");
                        }
                    }
                    if (DataAccess.GetDate() <= (DateTime)dtexam.Rows[0]["Start_DateTime"])
                    {
                        ViewBag.ErrorHeading = "Exam will be Held on " + ((DateTime)dtexam.Rows[0]["Start_DateTime"]).ToString("dd-MMMM-yyyy hh:mm tt");
                        return View("~/Views/Shared/Error.cshtml");
                    }
                    else if (DataAccess.GetDate() >= (DateTime)dtexam.Rows[0]["End_DateTime"])
                    {
                        ViewBag.ErrorHeading = "Exam Has Been Expired";
                        return View("~/Views/Shared/Error.cshtml");
                    }
                    else
                    {
                        exam.Exam_Id = (int)dtexam.Rows[0]["Exam_Id"];
                        exam.Course_Id = (int)dtexam.Rows[0]["Course_Id"];
                        exam.Exam_Name = dtexam.Rows[0]["Exam_Name"].ToString();
                        exam.Exam_Date = (DateTime)dtexam.Rows[0]["Exam_Date"];
                        exam.Start_Time = exam.Exam_Date.Date.Add((TimeSpan)dtexam.Rows[0]["Start_Time"]);
                        exam.End_Time = exam.Exam_Date.Date.Add((TimeSpan)dtexam.Rows[0]["End_Time"]);
                        exam.Total_Marks = (Double)dtexam.Rows[0]["Total_Marks"];
                        exam.Passing_Marks = (Double)dtexam.Rows[0]["Passing_Marks"];
                        exam.Duration = TimeSpan.FromMinutes(Int32.Parse(dtexam.Rows[0]["Duration"].ToString()));
                        exam.AutoSubmit = (Boolean)dtexam.Rows[0]["AutoSubmit"];
                        DataTable dtparts = DataAccess.daobj("select * from Section where Exam_Id = " + Eid.ToString() + " order by Section_Order Asc");
                        if (dtparts.Rows.Count > 0)
                        {
                            exam.Sections = new Section[dtparts.Rows.Count];
                            for (int i = 0; i < dtparts.Rows.Count; i++)
                            {
                                Section section = new Section();
                                section.Section_Id = (int)dtparts.Rows[i]["Section_Id"];
                                section.Exam_Id = (int)dtparts.Rows[i]["Exam_Id"];
                                section.Section_Text = dtparts.Rows[i]["Section_Text"].ToString();
                                section.Section_Marks = (double)dtparts.Rows[i]["Section_Marks"];
                                section.Section_Order = (int)dtparts.Rows[i]["Section_Order"];
                                exam.Sections[i] = section;

                                DataTable dtquestion = DataAccess.daobj("select Q.*,isnull(A.Answer,'') as StudentAnswer  from Question as Q left join (select * from Answer where Student_Id = '" + Sid + "') as A on Q.Question_Id = A.Question_Id where Section_Id = " + section.Section_Id.ToString() + " order by Q.Question_Order Asc");

                                if (dtquestion.Rows.Count > 0)
                                {
                                    exam.Sections[i].Questions = new Question[dtquestion.Rows.Count];
                                    for (int j = 0; j < dtquestion.Rows.Count; j++)
                                    {
                                        Question question = new Question();
                                        question.Question_Id = (int)dtquestion.Rows[j]["Question_Id"];
                                        if (dtquestion.Rows[j]["Question_Type"].ToString().Equals("FB"))
                                        { question.QuestionText = ConvertintoFB(dtquestion.Rows[j]["Question"].ToString()); }
                                        else
                                        { question.QuestionText = dtquestion.Rows[j]["Question"].ToString(); }
                                        question.Type = dtquestion.Rows[j]["Question_Type"].ToString();
                                        question.Question_Marks = (double)dtquestion.Rows[j]["Question_Marks"];
                                        question.Question_Order = (int)dtquestion.Rows[j]["Question_Order"];
                                        question.Sample_Answer = (String)dtquestion.Rows[j]["StudentAnswer"];
                                        exam.Sections[i].Questions[j] = question;

                                        if (question.Type.Equals("MCQ"))
                                        {
                                            DataTable dtchoices = DataAccess.daobj("select * from Choice where Question_Id = " + question.Question_Id.ToString() + " order by Choice_Id Asc");
                                            if (dtchoices.Rows.Count > 0)
                                            {
                                                exam.Sections[i].Questions[j].Options = new String[dtchoices.Rows.Count];
                                                for (int k = 0; k < dtchoices.Rows.Count; k++)
                                                {
                                                    String Choice = dtchoices.Rows[k]["Choice"].ToString();
                                                    exam.Sections[i].Questions[j].Options[k] = Choice;
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        return View(exam);
                    }
                }
                else
                {
                    ViewBag.ErrorHeading = "Invalid Exam Access";
                    return View("~/Views/Shared/Error.cshtml");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorHeading = "Invalid Exam Access";
                return View("~/Views/Shared/Error.cshtml");
            }

        }

        [MyCustomFilter]
        [HttpGet]
        public ActionResult CourseEnroll(String CourseEnrollId, String Password)
        {
            try
            {
                String Sid = ((Student)Session["CurrentUser"]).Id;
                DataAccess.clearparam();
                DataAccess.addParam("Sid", Sid);
                DataAccess.addParam("CourseEnrollId", CourseEnrollId);
                DataAccess.addParam("Password", Password);
                String output = (String)DataAccess.Reader("dbo.EnrollStudent @Sid , @CourseEnrollId , @Password", true, false);
                String result = output.Split('-')[0];
                switch (result)
                {
                    case "Enrolled Sucessfull":
                        return new HttpStatusCodeResult(200, output);
                        break;

                    case "Authentication Failed":
                        return new HttpStatusCodeResult(400, result);
                        break;

                    case "Already Exsist":
                        return new HttpStatusCodeResult(400, "You are Already Enrolled in this Course");
                        break;

                    default:
                        return new HttpStatusCodeResult(400, "An Error Occured Try Again Later");
                        break;
                }
            }
            catch (Exception ex)
            { return new HttpStatusCodeResult(400, "An Error Occured Try Again Later"); }
        }



        [MyCustomFilter]
        [HttpGet]
        public ActionResult PreExamValidate(int Eid)
        {

            try
            {
                String Sid = ((Student)Session["CurrentUser"]).Id;
                Exam exam = new Exam();
                DataTable ValidateStudent = DataAccess.daobj("select Top(1) * from Courses as C join Exam as E on C.Course_Id = E.Course_Id join Enrollement as En on C.Course_Id = En.Course_Id  join Student as S on En.Student_Id = S.id  where E.Exam_Id = " + Eid.ToString() + " and S.id = '" + Sid + "' ");
                DataTable Attempt = DataAccess.daobj("select End_Time from Exam_Log where Student_Id = '" + Sid + "' and Exam_Id = " + Eid + "");
                DataTable dtexam = DataAccess.daobj("select Top(1) *  ,Cast(Concat(Exam_Date,' ',End_Time) as datetime) as End_DateTime , Cast(Concat(Exam_Date,' ',Start_Time) as datetime) as Start_DateTime from Exam where Exam_Id = " + Eid.ToString() + "");
                if (dtexam.Rows.Count > 0 && ValidateStudent.Rows.Count > 0)
                {
                    if (Attempt.Rows.Count > 0)
                    {
                        if (Attempt.Rows[0]["End_Time"] != DBNull.Value)
                        {
                            return new HttpStatusCodeResult(400, "You Have Already Attempt this Exam");
                        }
                    }
                    if (DataAccess.GetDate() <= (DateTime)dtexam.Rows[0]["Start_DateTime"])
                    {
                        return new HttpStatusCodeResult(400, "Exam will be Held on " + ((DateTime)dtexam.Rows[0]["Start_DateTime"]).ToString("dd-MMMM-yyyy hh:mm tt"));
                    }
                    else if (DataAccess.GetDate() >= (DateTime)dtexam.Rows[0]["End_DateTime"])
                    {
                        return new HttpStatusCodeResult(400, "Exam Has Been Expired");
                    }
                    else
                    {}
                    return new HttpStatusCodeResult(200,"");
                }
                else
                {
                    return new HttpStatusCodeResult(400, "Invalid Exam Access");
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(400, "Invalid Exam Access");
            }

        }


        //This ConvertintoFB Method is first non static i have converted it into static in order to use in Teacher Controller//
        public static String ConvertintoFB(String QuestionText)
        {

            String FBQuestion = "";
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(QuestionText);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//text()");
            foreach (HtmlNode node in nodeCollection)
            {
                if (!node.ParentNode.Name.Equals("u"))
                { FBQuestion = FBQuestion + node.InnerHtml; }
                else{
                    FBQuestion = FBQuestion + "<input class=\"FBAnswer FBStyle\" onfocus=\"FBFocus(this)\" onblur=\"FBBlur(this)\" />";
                }
            }

            return FBQuestion;
        }


        [MyCustomFilter]
        [HttpGet]
        public async Task<String> GetRemainingTime(int eid)
        {
           return await Timer(eid);
        }

        public async Task<String> Timer(int eid)
        {
            String sid = ((Student)Session["CurrentUser"]).Id;
            TimeSpan span;
            DataTable dt = DataAccess.daobj("select * from Exam_Log where Student_Id = '" + sid + "' and Exam_Id = " + eid.ToString());
            if (!(dt.Rows.Count > 0))
            {
                DataAccess.clearparam();
                DataAccess.addParam("sid", sid);
                DataAccess.addParam("eid", eid);
                DataAccess.Execute("insert into Exam_log (Student_Id,Exam_Id,Start_Time) values (@sid,@eid,GETDATE())", true, false);
                dt = DataAccess.daobj("select * from Exam_Log where Student_Id = '" + sid + "' and Exam_Id = " + eid.ToString());
            }

            DataTable dtexam = DataAccess.daobj("select *,GETDATE() as CurrentTime from Exam where Exam_Id=" + eid.ToString());
            if (dtexam.Rows.Count > 0)
            {
                TimeSpan Exam_ET = ((TimeSpan)dtexam.Rows[0]["End_Time"]);
                TimeSpan Exam_ST = ((TimeSpan)dtexam.Rows[0]["Start_Time"]);
                TimeSpan Student_ST = ((DateTime)dt.Rows[0]["Start_Time"]).TimeOfDay;
                TimeSpan CurrentTime = ((DateTime)dtexam.Rows[0]["CurrentTime"]).TimeOfDay;

                if ((Boolean)dtexam.Rows[0]["AutoSubmit"])
                {
                    span = TimeSpan.FromMinutes(Exam_ET.Subtract(CurrentTime).TotalMinutes);
                    if (span.TotalSeconds >= 0)
                    { return "<span style=\"font-size:22px ; font-weight:bold; font-family:Calibri\">Time Left : <span style=\"font-size: 22px; font-weight: lighter;\">" + span.Hours.ToString() + "<span style=\"font-size: 10px; font-weight: bold;\">h</span> </span><span style=\"font-size: 22px; font-weight: lighter;\">" + span.Minutes.ToString() + "<span style=\"font-size: 10px; font-weight: bold;\">m</span> </span> <span style=\"font-size: 22px; font-weight: lighter;\">" + span.Seconds.ToString() + "<span style=\"font-size: 10px; font-weight: bold; \">s</span></span></span>"; }
                    else
                    { return "<span style=\"font-size:22px ; font-weight:bold; font-family:Calibri\">Finished</span>"; }

                }
                else
                {
                    span = TimeSpan.FromMinutes(Exam_ET.Subtract(Exam_ST).Subtract(CurrentTime.Subtract(Student_ST)).TotalMinutes);
                    if (span.TotalSeconds >= 0)
                    { return "<span style=\"font-size:22px ; font-weight:bold; font-family:Calibri\">Time Left : <span style=\"font-size: 22px; font-weight: lighter;\">" + span.Hours.ToString() + "<span style=\"font-size: 10px; font-weight: bold;\">h</span> </span><span style=\"font-size: 22px; font-weight: lighter;\">" + span.Minutes.ToString() + "<span style=\"font-size: 10px; font-weight: bold;\">m</span> </span> <span style=\"font-size: 22px; font-weight: lighter;\">" + span.Seconds.ToString() + "<span style=\"font-size: 10px; font-weight: bold; \">s</span></span></span>"; }
                    else
                    { return "<span style=\"font-size:22px ; font-weight:bold; font-family:Calibri\">Finished</span>"; }
                }
            }
            else
            { span = TimeSpan.FromMinutes(0) ;
            return "<span style=\"font-size:22px ; font-weight:bold; font-family:Calibri\">Time Left : <span style=\"font-size: 22px; font-weight: lighter;\">" + span.Hours.ToString() + "<span style=\"font-size: 10px; font-weight: bold;\">h</span> </span><span style=\"font-size: 22px; font-weight: lighter;\">" + span.Minutes.ToString() + "<span style=\"font-size: 10px; font-weight: bold;\">m</span> </span> <span style=\"font-size: 22px; font-weight: lighter;\">" + span.Seconds.ToString() + "<span style=\"font-size: 10px; font-weight: bold; \">s</span></span></span>";
            }
       }



        [MyCustomFilter]
        [HttpPost]
        public void SaveExam(Exam exam)
        {
            Double ObtainedMarks = 0;
            Double TotalObtainedMarks = 0;
            try
            {
                String Sid = ((Student)Session["CurrentUser"]).Id;
                Double PassingMarks = (Double)DataAccess.Reader("select Passing_Marks from Exam where Exam_Id = " + exam.Exam_Id.ToString());
            
                for (int s = 0; s < exam.Sections.Count() ; s++)
                {
                    for (int q = 0; q < exam.Sections[s].Questions.Count(); q++)
                    {
                
                        if (((int)DataAccess.Reader("select Count(Answer_Id) from Answer where Question_Id = " + exam.Sections[s].Questions[q].Question_Id.ToString() + " and Student_Id = '" + Sid + "'")) > 0)
                        {
                            ObtainedMarks = CalculateMarks(exam.Sections[s].Questions[q].Sample_Answer != null ? exam.Sections[s].Questions[q].Sample_Answer : "", exam.Sections[s].Questions[q].Question_Id);
                            TotalObtainedMarks = TotalObtainedMarks + ObtainedMarks;
                            DataAccess.clearparam();
                            DataAccess.addParam("ans", exam.Sections[s].Questions[q].Sample_Answer != null ?  exam.Sections[s].Questions[q].Sample_Answer : "");
                            DataAccess.addParam("marks", ObtainedMarks);
                            DataAccess.Execute("Update Answer set Answer = @ans , Obtain_Marks = @marks where Question_Id = " + exam.Sections[s].Questions[q].Question_Id.ToString() + " and Student_Id = '" + Sid + "'", true, true);
                        }
                        else
                        {
                            ObtainedMarks = CalculateMarks(exam.Sections[s].Questions[q].Sample_Answer != null ? exam.Sections[s].Questions[q].Sample_Answer : "", exam.Sections[s].Questions[q].Question_Id);
                            TotalObtainedMarks = TotalObtainedMarks + ObtainedMarks;
                            DataAccess.clearparam();
                            DataAccess.addParam("qid", exam.Sections[s].Questions[q].Question_Id);
                            DataAccess.addParam("ans", exam.Sections[s].Questions[q].Sample_Answer != null ? exam.Sections[s].Questions[q].Sample_Answer : "");
                            DataAccess.addParam("marks", ObtainedMarks);
                            DataAccess.addParam("sid", Sid);
                            DataAccess.Execute("insert into Answer (Answer_Id,Question_Id,Student_Id,Answer,Obtain_Marks) values ((select isnull(Max(Answer_Id),0)+1 from Answer ),@qid,@sid,@ans,@marks)", true, true);
                        }
                    }
                }

                if (((int)DataAccess.Reader("select Count(Result_Id) from Result where Exam_Id = " + exam.Exam_Id.ToString() + " and Student_Id = '" + Sid + "'")) > 0)
                {
                    DataAccess.clearparam();
                    DataAccess.addParam("obtainedmarks", TotalObtainedMarks);
                    DataAccess.addParam("grade", CalculateGrade(((TotalObtainedMarks / exam.Total_Marks) * 100), exam.Exam_Id));
                    DataAccess.addParam("percentage", ((TotalObtainedMarks / exam.Total_Marks) * 100));
                    DataAccess.addParam("result", TotalObtainedMarks >= PassingMarks ? "Passed" : "Failed");
                    DataAccess.addParam("remarks", "");
                    DataAccess.Execute("update Result set Obtain_Marks = @obtainedmarks ,Grade = @grade , Remarks = @remarks ,Percentage = @percentage ,Result = @result where Exam_Id = "+exam.Exam_Id.ToString()+" and Student_Id = '"+Sid+"'", true, true);
                }
                else
                {
                    DataAccess.clearparam();
                    DataAccess.addParam("eid", exam.Exam_Id);
                    DataAccess.addParam("sid", Sid);
                    DataAccess.addParam("obtainedmarks", TotalObtainedMarks);
                    DataAccess.addParam("grade", CalculateGrade(((TotalObtainedMarks / exam.Total_Marks) * 100), exam.Exam_Id));
                    DataAccess.addParam("percentage", ((TotalObtainedMarks / exam.Total_Marks) * 100));
                    DataAccess.addParam("result", TotalObtainedMarks >= PassingMarks ? "Passed" : "Failed");
                    DataAccess.addParam("remarks", "");
                    DataAccess.Execute("insert into Result (Result_Id,Exam_Id,Student_Id,Obtain_Marks,Grade,Remarks,Percentage,Result) values ((select isnull(Max(Result_Id),0)+1 from Result),@eid,@sid,@obtainedmarks,@grade,@remarks,@percentage,@result)", true, true);
                }
                       
                
                DataAccess.Execute("update Exam_Log set End_Time = GETDATE() where Student_Id = '" + Sid + "' and Exam_Id = " + exam.Exam_Id.ToString() + "", false, true);
                DataAccess.TransactionCommit();
            }
            catch(Exception e)
            {
                DataAccess.TransactionRollback();
            }

        }

        [MyCustomFilter]
        [HttpPost]
        public async Task<HttpStatusCodeResult> AutoSave(Exam exam) 
        {
               
            Double ObtainedMarks = 0;
            Double TotalObtainedMarks = 0;
            try
            {
                String Sid = ((Student)Session["CurrentUser"]).Id;
                Double PassingMarks = (Double)DataAccess.Reader("select Passing_Marks from Exam where Exam_Id = " + exam.Exam_Id.ToString());
            
                for (int s = 0; s < exam.Sections.Count() ; s++)
                {
                    for (int q = 0; q < exam.Sections[s].Questions.Count(); q++)
                    {
                
                        if (((int)DataAccess.Reader("select Count(Answer_Id) from Answer where Question_Id = " + exam.Sections[s].Questions[q].Question_Id.ToString() + " and Student_Id = '" + Sid + "'")) > 0)
                        {
                            ObtainedMarks = CalculateMarks(exam.Sections[s].Questions[q].Sample_Answer != null ? exam.Sections[s].Questions[q].Sample_Answer : "", exam.Sections[s].Questions[q].Question_Id);
                            TotalObtainedMarks = TotalObtainedMarks + ObtainedMarks;
                            DataAccess.clearparam();
                            DataAccess.addParam("ans", exam.Sections[s].Questions[q].Sample_Answer != null ? exam.Sections[s].Questions[q].Sample_Answer : "");
                            DataAccess.addParam("marks", ObtainedMarks);
                            DataAccess.Execute("Update Answer set Answer = @ans , Obtain_Marks = @marks where Question_Id = " + exam.Sections[s].Questions[q].Question_Id.ToString() + " and Student_Id = '" + Sid + "'", true, true);
                        }
                        else
                        {
                            ObtainedMarks = CalculateMarks(exam.Sections[s].Questions[q].Sample_Answer != null ? exam.Sections[s].Questions[q].Sample_Answer : "", exam.Sections[s].Questions[q].Question_Id);
                            TotalObtainedMarks = TotalObtainedMarks + ObtainedMarks;
                            DataAccess.clearparam();
                            DataAccess.addParam("qid", exam.Sections[s].Questions[q].Question_Id);
                            DataAccess.addParam("ans", exam.Sections[s].Questions[q].Sample_Answer != null ?  exam.Sections[s].Questions[q].Sample_Answer : "");
                            DataAccess.addParam("marks", ObtainedMarks);
                            DataAccess.addParam("sid", Sid);
                            DataAccess.Execute("insert into Answer (Answer_Id,Question_Id,Student_Id,Answer,Obtain_Marks) values ((select isnull(Max(Answer_Id),0)+1 from Answer ),@qid,@sid,@ans,@marks)", true, true);
                        }
                    }
                }

                if (((int)DataAccess.Reader("select Count(Result_Id) from Result where Exam_Id = " + exam.Exam_Id.ToString() + " and Student_Id = '" + Sid + "'")) > 0)
                {
                    DataAccess.clearparam();
                    DataAccess.addParam("obtainedmarks", TotalObtainedMarks);
                    DataAccess.addParam("grade", CalculateGrade(((TotalObtainedMarks / exam.Total_Marks) * 100), exam.Exam_Id));
                    DataAccess.addParam("percentage", ((TotalObtainedMarks / exam.Total_Marks) * 100));
                    DataAccess.addParam("result", TotalObtainedMarks >= PassingMarks ? "Passed" : "Failed");
                    DataAccess.addParam("remarks", "");
                    DataAccess.Execute("update Result set Obtain_Marks = @obtainedmarks ,Grade = @grade , Remarks = @remarks ,Percentage = @percentage ,Result = @result where Exam_Id = "+exam.Exam_Id.ToString()+" and Student_Id = '"+Sid+"'", true, true);
                }
                else
                {
                    DataAccess.clearparam();
                    DataAccess.addParam("eid", exam.Exam_Id);
                    DataAccess.addParam("sid", Sid);
                    DataAccess.addParam("obtainedmarks", TotalObtainedMarks);
                    DataAccess.addParam("grade", CalculateGrade(((TotalObtainedMarks / exam.Total_Marks) * 100), exam.Exam_Id));
                    DataAccess.addParam("percentage", ((TotalObtainedMarks / exam.Total_Marks) * 100));
                    DataAccess.addParam("result", TotalObtainedMarks >= PassingMarks ? "Passed" : "Failed");
                    DataAccess.addParam("remarks", "");
                    DataAccess.Execute("insert into Result (Result_Id,Exam_Id,Student_Id,Obtain_Marks,Grade,Remarks,Percentage,Result) values ((select isnull(Max(Result_Id),0)+1 from Result),@eid,@sid,@obtainedmarks,@grade,@remarks,@percentage,@result)", true, true);
                }
                       

            //Double ObtainedMarks = 0;
            //try
            //{
            //    String Sid = ((Student)Session["CurrentUser"]).Id;
            //    for (int s = 0; s < exam.Sections.Count(); s++)
            //    {
            //        for (int q = 0; q < exam.Sections[s].Questions.Count(); q++)
            //        {
            //            if (((int)DataAccess.Reader("select Count(Answer_Id) from Answer where Question_Id = " + exam.Sections[s].Questions[q].Question_Id.ToString() + " and Student_Id = '" + Sid + "'")) > 0)
            //            {
            //                ObtainedMarks = CalculateMarks(exam.Sections[s].Questions[q].Sample_Answer, exam.Sections[s].Questions[q].Question_Id);
            //                DataAccess.clearparam();
            //                DataAccess.addParam("ans", exam.Sections[s].Questions[q].Sample_Answer);
            //                DataAccess.addParam("marks", ObtainedMarks);
            //                DataAccess.Execute("Update Answer set Answer = @ans , Obtain_Marks = @marks where Question_Id = " + exam.Sections[s].Questions[q].Question_Id.ToString() + " and Student_Id = '"+Sid+"'", true, true);
            //            }
            //            else
            //            {
            //                ObtainedMarks = CalculateMarks(exam.Sections[s].Questions[q].Sample_Answer, exam.Sections[s].Questions[q].Question_Id);
            //                DataAccess.clearparam();
            //                DataAccess.addParam("qid", exam.Sections[s].Questions[q].Question_Id);
            //                DataAccess.addParam("ans", exam.Sections[s].Questions[q].Sample_Answer);
            //                DataAccess.addParam("marks", ObtainedMarks);
            //                DataAccess.addParam("sid", Sid);
            //                DataAccess.Execute("insert into Answer (Answer_Id,Question_Id,Student_Id,Answer,Obtain_Marks) values ((select isnull(Max(Answer_Id),0)+1 from Answer ),@qid,@sid,@ans,@marks)", true, true);
            //            }

            //        }
            //    }

                DataAccess.TransactionCommit();
                return new HttpStatusCodeResult(200);
            }
            catch (Exception e)
            {
                DataAccess.TransactionRollback();
                return new HttpStatusCodeResult(400);
            }
        
        }

        public Double CalculateMarks(String Answer, int Question_id)
        {
            Double Marks = 0;
            DataTable dtquestion = DataAccess.daobj("select * from Question where Question_id = " + Question_id.ToString() + "");
            if (dtquestion.Rows[0]["Question_Type"].Equals("Descriptive"))
            {
                //Marks = CalculateDiscriptiveMarks(Answer,Question_id);
            }
            else if (dtquestion.Rows[0]["Question_Type"].Equals("FB"))
            { 
               
                String[] StudentAnswers = Answer.Split(new String[]{"~*%*~"},StringSplitOptions.None);
                String[] TeacherAnswers = dtquestion.Rows[0]["Sample_Answer"].ToString().Split(new String[]{"~*%*~"},StringSplitOptions.None);
                Double SingleFBMark = ((Double)dtquestion.Rows[0]["Question_Marks"])/(TeacherAnswers.Length);

                if (StudentAnswers.Length == TeacherAnswers.Length)
                {
                    for (int i = 0 ; i < StudentAnswers.Length ; i++)
                    {
                         if (StudentAnswers[i].Trim().Equals(TeacherAnswers[i].Trim()))
                         { Marks = Marks + SingleFBMark; }
                    }
                }
               
            }
            else
            {
                if (Answer.Trim().Equals(dtquestion.Rows[0]["Sample_Answer"].ToString().Trim()))
                {Marks = (Double)dtquestion.Rows[0]["Question_Marks"];}
            }

            return Marks;
        }

        public String CalculateGrade(Double MarksObtainedInPercentage, int Exam_id)
        {
            String Grade = "Not Specified";
            DataTable dtGrade = DataAccess.daobj("select * from GradingPolicy where Grade_Id = (select Top(1) Grade_Id from Exam where Exam_Id = " + Exam_id.ToString() + ")");
            if (dtGrade.Rows.Count > 0)
            {
                for (int i = 0; i < dtGrade.Rows.Count; i++)
                {
                    if (MarksObtainedInPercentage >= ((int)dtGrade.Rows[i]["From_Percentage"]) && MarksObtainedInPercentage <= ((int)dtGrade.Rows[i]["To_Percentage"]))
                    { return dtGrade.Rows[i]["Grade"].ToString(); }
                }
            }
            return Grade;
        }



        [HttpPost]
        [MyCustomFilter]
        public ActionResult LeaveCourse(String CourseId)
        {
            try
            {
                String Sid = ((Student)Session["CurrentUser"]).Id;
                DataAccess.addParam("cid", CourseId);
                DataAccess.addParam("sid", Sid);
                Object result = DataAccess.Reader("dbo.DeEnrollStudent @cid , @sid", true, false);
                return new HttpStatusCodeResult(200);
            }
            catch (Exception e)
            { return new HttpStatusCodeResult(400); }
        }



        [MyCustomFilter]
        [HttpGet]
        public ActionResult Course(String CourseId)
        {
            CourseDetail CourseDes = new CourseDetail();
            DataTable dtc = DataAccess.daobj("select Top(1) * , T.full_name  from Courses as C Join Enrollement as E on C.Course_Id = E.Course_Id Join Teacher as T on C.Teacher_Id = T.id where C.Course_Id = " + CourseId + " and E.Student_Id = '" + ((Student)Session["CurrentUser"]).Id.ToString() + "'");
            int Strength = (int)DataAccess.Reader("select  Count(Student_Id) from Enrollement where Course_Id = "+CourseId+" and Status = 'Active'");
            DataTable dte = DataAccess.daobj("select * from Exam  where Course_Id = " + CourseId + " order by Cast(Concat (cast (Exam_Date as nvarchar) , ' ' ,cast (Start_Time as nvarchar) ) as datetime) Asc");
            if (dtc.Rows.Count > 0)
            {
                CourseDes.Teacher_Name = dtc.Rows[0]["full_name"].ToString();
                CourseDes.Strength = Strength;
                CourseDes.Course = new Course
                {
                    Course_Id = Int32.Parse(dtc.Rows[0]["Course_Id"].ToString()),
                    Name = dtc.Rows[0]["Course_name"].ToString(),
                    Date = dtc.Rows[0]["Date_Created"].ToString(),
                    Status = dtc.Rows[0]["Status"].ToString(),
                    Enroll_id = dtc.Rows[0]["Enroll_Id"].ToString()
                };

                if (dte.Rows.Count > 0)
                {
                    Exam[] Exams = new Exam[dte.Rows.Count];
                    for (int i = 0; i < dte.Rows.Count; i++)
                    {
                        Exams[i] = new Exam
                        {
                            Exam_Id = Int32.Parse(dte.Rows[i]["Exam_Id"].ToString()),
                            Exam_Name = dte.Rows[i]["Exam_Name"].ToString(),
                            Exam_Date = DateTime.Parse(dte.Rows[i]["Exam_Date"].ToString()),
                            Start_Time = DateTime.Parse(dte.Rows[i]["Start_Time"].ToString()),
                            End_Time = DateTime.Parse(dte.Rows[i]["End_Time"].ToString()),
                            Total_Marks = Int32.Parse(dte.Rows[i]["Total_Marks"].ToString()),
                            Duration = DateTime.Parse(dte.Rows[i]["End_Time"].ToString()).Subtract(DateTime.Parse(dte.Rows[i]["Start_Time"].ToString())),
                            AutoSubmit = (Boolean)dte.Rows[i]["AutoSubmit"]
                        };
                    }
                    CourseDes.Course.exam = Exams;
                }

                TempData["Cid"] = CourseId;
            }
            else
            {
                return RedirectToAction("Dashboard", "Student");
            }
            return View(CourseDes);
        }



        [HttpGet]
        [MyCustomFilter]
        public JsonResult GetGrade(String ExamId)
        {
                String Sid = ((Student)Session["CurrentUser"]).Id;
                DataAccess.addParam("eid", ExamId);
                DataAccess.addParam("sid", Sid);
                DataTable result = DataAccess.daobj("dbo.GetGrade @eid , @sid", true, false);
                if (result.Rows.Count > 0)
                {
                    if (result.Columns.Count > 1)
                    {
                        var Data = new { Grade = result.Rows[0][3].ToString(), TotalMarks = result.Rows[0][0].ToString(), ObtainedMarks = result.Rows[0][1].ToString(), Percentage = result.Rows[0][2].ToString(), PassingMarks = result.Rows[0][4].ToString() };
                        return Json(Data, JsonRequestBehavior.AllowGet);
                    }
                    else
                    { return Json(new { Grade = result.Rows[0][0].ToString() }, JsonRequestBehavior.AllowGet); }
                }
                else
                { return Json(null, JsonRequestBehavior.AllowGet); }
                
        }


	}


}