using Evalin.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Evalin.Controllers
{
    public class TeacherController : Controller
    {
        [MyCustomFilter]
        [HttpGet]
        public ActionResult Dashboard()
        {
            return View();
        }


        [MyCustomFilter]
        [HttpGet]
        public ActionResult Profile()
        {
            return View("~/Views/Shared/Profile.cshtml");
        }

        [MyCustomFilter]
        [HttpGet]
        public ActionResult Class()
        {
            return View();
        }

        [MyCustomFilter]
        [HttpGet]
        public ActionResult AddExam(String CourseId = "")
        {
            ViewBag.Cid = CourseId;
            return View();
        }
        
        [MyCustomFilter]
        [HttpPost]
        public ActionResult AddExam(Exam exam)
        {
            int eid = 0;
            int sid = 0;
            int qid = 0;
            Boolean Of = false;
            Boolean Fbf = false;
            DateTime EndTime = new DateTime(exam.Exam_Date.Year,exam.Exam_Date.Month,exam.Exam_Date.Day,exam.End_Time.Hour,exam.End_Time.Minute,exam.End_Time.Second);
            DateTime StartTime = new DateTime(exam.Exam_Date.Year, exam.Exam_Date.Month, exam.Exam_Date.Day, exam.Start_Time.Hour, exam.Start_Time.Minute, exam.Start_Time.Second);
           
            try
            {
                double t_marks = 0;
                if (exam.Sections != null && DateTime.Compare(EndTime, StartTime) > 0)
                {
                    foreach (Section item in exam.Sections)
                    {
                        if (item.Questions != null)
                        {
                            foreach (Question Ques in item.Questions)
                            { t_marks = t_marks + Ques.Question_Marks; }
                        }
                    }
                }
                else
                { throw new Exception(""); }
                DataAccess.clearparam();
                DataAccess.addParam("title", exam.Exam_Name.ToString());
                DataAccess.addParam("date", exam.Exam_Date.ToString("yyyy-MMMM-dd"));
                DataAccess.addParam("stime", StartTime.ToString("hh:mm:ss"));
                DataAccess.addParam("etime", EndTime.ToString("hh:mm:ss"));
                DataAccess.addParam("tmarks", t_marks);
                DataAccess.addParam("pmarks", exam.Passing_Marks);
                DataAccess.addParam("duration", (EndTime - StartTime).TotalMinutes);
                DataAccess.addParam("autosubmit", exam.AutoSubmit);
                DataAccess.addParam("instruction", "");
                DataAccess.addParam("cid", exam.Course_Id);
                eid = (int)DataAccess.Reader("select (Isnull(Max(Exam_Id),0)+1) as Id from Exam");
                DataAccess.addParam("eid", eid);
                DataAccess.Execute("insert into Exam (Exam_id,Course_Id,Exam_Name,Exam_Date,Start_Time,End_Time,Total_Marks,Passing_Marks,Duration,AutoSubmit,Instructions) values (@eid,@cid,@title,@date,@stime,@etime,@tmarks,@pmarks,@duration,@autosubmit,@instruction)", true, true);
                if (exam.Sections != null)
                {
                    foreach (Section item in exam.Sections)
                    {
                        DataAccess.clearparam();
                        sid = (int)DataAccess.Reader("select (Isnull(Max(Section_Id),0)+1) as Id from Section");
                        DataAccess.addParam("sid", sid);
                        DataAccess.addParam("eid", eid);
                        DataAccess.addParam("stext", item.Section_Text);
                        DataAccess.addParam("smarks", item.Section_Marks);
                        DataAccess.addParam("sorder", item.Section_Order);
                        DataAccess.Execute("insert into Section (Section_Id,Exam_Id,Section_Text,Section_Marks,Section_Order) values (@sid,@eid,@stext,@smarks,@sorder)", true, true);
                        if (item.Questions != null)
                        {
                            foreach (Question Ques in item.Questions)
                            {
                                DataAccess.clearparam();
                                qid = (int)DataAccess.Reader("select (Isnull(Max(Question_Id),0)+1) as Id from Question");
                                DataAccess.addParam("qid", qid);
                                DataAccess.addParam("sid", sid);
                                DataAccess.addParam("question", Ques.QuestionText);
                                DataAccess.addParam("marks", Ques.Question_Marks);
                                DataAccess.addParam("order", Ques.Question_Order);
                                DataAccess.addParam("type", Ques.Type);
                                switch (Ques.Type)
                                {
                                    case "Descriptive":
                                        DataAccess.addParam("answer", Ques.Sample_Answer);
                                    break;

                                    case "FB":
                                    DataAccess.addParam("answer", ExtractAnswer(Ques.QuestionText));
                                    Fbf = true;
                                    break;

                                    case "MCQ":
                                    DataAccess.addParam("answer", Ques.Sample_Answer);
                                    Of = true;
                                    break;

                                    case "TF":
                                    DataAccess.addParam("answer", Ques.Sample_Answer);
                                    break;
                                }
                                DataAccess.Execute("insert into Question (Question_Id,Section_Id,Question,Question_Type,Question_Marks,Sample_Answer,Question_Order) values (@qid,@sid,@question,@type,@marks,@answer,@order)", true, true);
                                if (Of && Ques.Options != null && Ques.Options.Length > 0)
                                {
                                    foreach (String Options in Ques.Options)
                                    {
                                        DataAccess.clearparam();
                                        DataAccess.addParam("qid", qid);
                                        DataAccess.addParam("choice", Options);
                                        if (Ques.Sample_Answer.Equals(Options))
                                        { DataAccess.addParam("correct", true); }
                                        else
                                        { DataAccess.addParam("correct", false); }
                                        DataAccess.Execute("insert into Choice (Choice_Id,Question_Id,Choice,IsCorrect) values ((select (Isnull(Max(Choice_Id),0)+1) as Id from Choice),@qid,@choice,@correct)", true, true);
                                    }
                                }
                                else if (Fbf)
                                {
                                    List<String> answers = ExtractAnswerList(Ques.QuestionText);
                                    if (answers.Count > 0)
                                    {
                                        foreach (String ans in answers)
                                        {
                                            DataAccess.clearparam();
                                            DataAccess.addParam("qid", qid);
                                            DataAccess.addParam("answer", ans);
                                            DataAccess.Execute("insert into FB_Answers (FB_Answer_Id,Question_Id,Answer) values ((select (Isnull(Max(FB_Answer_Id),0)+1) as Id from FB_Answers),@qid,@answer)", true, true);
                                        }
                                    }
                                }

                                Fbf = false;
                                Of = false;



                            }
                        }
                    }
                }
                DataAccess.TransactionCommit();
                return new HttpStatusCodeResult(200,eid.ToString());
               
            }
            catch (Exception e)
            { 
            DataAccess.TransactionRollback();
            return new HttpStatusCodeResult(400);
            }
            
            
            
           
           
     
        }


        public String ExtractAnswer(string Question)
        {
            String ans = "";
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(Question);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//text()");
            foreach (HtmlNode node in nodeCollection)
            {
                if (node.ParentNode.Name.Equals("u"))
                {
                    if (ans.Equals(""))
                    {
                     ans = node.InnerText;
                    }
                    else
                    { ans = ans + "~*%*~" + node.InnerText; }
                }
            }
            return ans;
            }


        public List<String> ExtractAnswerList(string Question)
        {
            List<string> answers = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(Question);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//text()");
            foreach (HtmlNode node in nodeCollection)
            {
                if (node.ParentNode.Name.Equals("u"))
                {answers.Add(node.InnerText);}
            }
            return answers;
        }




        [MyCustomFilter]
        [HttpGet]
        public ActionResult Course(String CourseId)
        {
            CourseDetail CourseDes = new CourseDetail();
            DataTable dtc = DataAccess.daobj("select Top(1) * from Courses where Course_Id = "+CourseId+" and Teacher_Id = '"+((User)Session["CurrentUser"]).Id.ToString()+"'");
            DataTable dte = DataAccess.daobj("select * from Exam  where Course_Id = "+CourseId);
            DataTable dts = DataAccess.daobj("select * from Enrollement as En Join Student as St on  En.Student_Id = St.id where Course_Id = " + CourseId +" and St.Blocked = 0");
            if (dtc.Rows.Count > 0)
            {
                CourseDes.Course = new Course
                {
                    Course_Id = Int32.Parse(dtc.Rows[0]["Course_Id"].ToString()),
                    Name = dtc.Rows[0]["Course_name"].ToString(),
                    Date = dtc.Rows[0]["Date_Created"].ToString(),
                    Status = dtc.Rows[0]["Status"].ToString(),
                    Enroll_id = dtc.Rows[0]["Enroll_Id"].ToString(),
                    Enroll_Password = dtc.Rows[0]["Enroll_Password"].ToString()
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
                            Duration = DateTime.Parse(dte.Rows[i]["End_Time"].ToString()).Subtract(DateTime.Parse(dte.Rows[i]["Start_Time"].ToString()))
                        };
                    }
                    CourseDes.Course.exam = Exams;
                }

                if (dts.Rows.Count > 0)
                {
                    EnrolledStudent[] Students = new EnrolledStudent[dts.Rows.Count];
                    for (int i = 0; i < dts.Rows.Count; i++)
                    {
                        Students[i] = new EnrolledStudent
                        {
                            Id = dts.Rows[i]["Student_Id"].ToString(),
                            Fullname = dts.Rows[i]["full_name"].ToString(),
                            Username = dts.Rows[i]["username"].ToString(),
                            Gender = dts.Rows[i]["Gender"].ToString(),
                            Emailaddress = dts.Rows[i]["Email"].ToString(),
                            ImageLocation = dts.Rows[i]["image"].ToString(),
                            Status = dts.Rows[i]["Status"].ToString(),
                            Enrollement_Date = DateTime.Parse(dts.Rows[i]["Enrollement_Date"].ToString()),
                            Course_Id = Int32.Parse(CourseId)
                        };
                    }
                    CourseDes.Students = Students;
                }
                TempData["Cid"] = CourseId;
            }
            else {

                return RedirectToAction("Dashboard", "Teacher");
            }
            return View(CourseDes);
        }

        [HttpPost]
        [MyCustomFilter]
        public ActionResult DeEnrollStudent(string sid,String cid)
        {
            try
            {
                DataAccess.addParam("cid", cid);
                DataAccess.addParam("sid", sid);
                Object result = DataAccess.Reader("dbo.DeEnrollStudent @cid , @sid", true, false);
                return new HttpStatusCodeResult(200);
            }
            catch (Exception e)
            { return new HttpStatusCodeResult(400); }
        }


        [HttpPost]
        [MyCustomFilter]
        public ActionResult UpdateResult(Exam Exam)
        {

            try
            {

                String StudentId = DataAccess.Reader("select	Student_Id from Result where Result_Id = " + Exam.Temp).ToString();
                double ObtainedMarks = 0;
                String Grade = "F";
                Double Percentage; 
                if (Exam != null)
                {
                    if (Exam.Sections != null)
                    {
                        for (int s = 0; s < Exam.Sections.Count(); s++)
                        {

                            if (Exam.Sections[s].Questions != null)
                            {

                                for (int q = 0; q < Exam.Sections[s].Questions.Count(); q++)
                                {
                                    ObtainedMarks += Exam.Sections[s].Questions[q].Obtained_Marks;
                                    DataAccess.Execute("update Answer set Obtain_Marks = " + Exam.Sections[s].Questions[q].Obtained_Marks + " where Student_Id = '" + StudentId + "' and Question_Id = " + Exam.Sections[s].Questions[q].Question_Id, false, true);
                                }

                            }
                        }

                    }
                    Percentage = (ObtainedMarks/Exam.Total_Marks)*100;
                    Grade = CalculateGrade(Percentage, Exam.Exam_Id);
                    DataAccess.Execute("update Result set Obtain_Marks = " + ObtainedMarks.ToString() + ",Grade = '" + Grade + "', Percentage = '"+Percentage.ToString()+"' where Result_Id = " + Exam.Temp,false,true);
                    return RedirectToAction("Results", "Teacher");

                }
                else { return RedirectToAction("Error", "Account"); }

            }
            catch (Exception e)
            { return RedirectToAction("Error", "Account"); }
        
        
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
        public ActionResult DeleteExam(string eid, String cid)
        {
            try
            {
                DataAccess.addParam("cid", cid);
                DataAccess.addParam("eid", eid);
                Object result = DataAccess.Reader("dbo.DeleteExam @eid , @cid", true, false);
                return new HttpStatusCodeResult(200);
            }
            catch (Exception e)
            { return new HttpStatusCodeResult(400); }
        }

        [HttpPost]
        [MyCustomFilter]
        public ActionResult DeleteCourse(String CourseId)
        {
            try
            {
                DataAccess.addParam("cid", CourseId);
                DataAccess.addParam("tid", ((Teacher)(Session["CurrentUser"])).Id);
                Object result = DataAccess.Reader("dbo.DeleteCourse @cid , @tid", true, false);
                if ((int)result == 1)
                { return new HttpStatusCodeResult(200); }
                else
                { return new HttpStatusCodeResult(400); }
            }
            catch (Exception e)
            { return new HttpStatusCodeResult(400); }
        }




        [MyCustomFilter]
        [HttpGet]
        public ActionResult PreviewExam(String ExamId,String Type = "Solved")
        {
            String Tid = ((Teacher)Session["CurrentUser"]).Id;
            DataTable dtexam = DataAccess.daobj("select E.* from Exam as E Join Courses as C on E.Course_Id = C.Course_Id where C.Teacher_Id = '"+Tid+"' and E.Exam_Id = "+ExamId, false);
            Exam exam = new Exam();
            if (dtexam.Rows.Count > 0)
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
                    DataTable dtparts = DataAccess.daobj("select * from Section where Exam_Id = " + ExamId + " order by Section_Order Asc");
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

                            DataTable dtquestion = DataAccess.daobj("select *  from Question where Section_Id = "+section.Section_Id.ToString()+" order by Question_Order Asc");

                            if (dtquestion.Rows.Count > 0)
                            {
                                exam.Sections[i].Questions = new Question[dtquestion.Rows.Count];
                                for (int j = 0; j < dtquestion.Rows.Count; j++)
                                {
                                    Question question = new Question();
                                    question.Question_Id = (int)dtquestion.Rows[j]["Question_Id"];
                                    if (dtquestion.Rows[j]["Question_Type"].ToString().Equals("FB"))
                                    { question.QuestionText = StudentController.ConvertintoFB(dtquestion.Rows[j]["Question"].ToString()); }
                                    else
                                    { question.QuestionText = dtquestion.Rows[j]["Question"].ToString(); }
                                    question.Type = dtquestion.Rows[j]["Question_Type"].ToString();
                                    question.Question_Marks = (double)dtquestion.Rows[j]["Question_Marks"];
                                    question.Question_Order = (int)dtquestion.Rows[j]["Question_Order"];
                                    if (Type.Equals("Solved"))
                                    { question.Sample_Answer = (String)dtquestion.Rows[j]["Sample_Answer"]; }
                                    else
                                    {question.Sample_Answer = "";}
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
            else
            {
                ViewBag.ErrorHeading = "Invalid Exam Access";
                return View("~/Views/Shared/Error.cshtml");
            }

        }


        [HttpGet]
        [MyCustomFilter]
        public ActionResult PreviewResult(String ResultId)
        {

            String Tid = ((Teacher)Session["CurrentUser"]).Id;
            Int32 AutherizeAccess = (Int32)DataAccess.Reader("select Count(R.Result_Id) from Result as R Join Exam as E on E.Exam_Id = R.Exam_Id Join Courses as C on C.Course_Id = E.Course_Id where C.Teacher_Id = '"+Tid+"' and R.Result_Id = "+ResultId+"", false, false);
            if (AutherizeAccess > 0)
            {

                DataTable dtexam = DataAccess.daobj("select E.* from Result as R Join Exam as E on E.Exam_Id = R.Exam_Id where R.Result_Id = " + ResultId, false);
                Exam exam = new Exam();
                if (dtexam.Rows.Count > 0)
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
                    exam.Temp = ResultId;
                    DataTable dtparts = DataAccess.daobj("select * from Section where Exam_Id = " + exam.Exam_Id.ToString() + " order by Section_Order Asc");
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

                            DataTable dtquestion = DataAccess.daobj("select Q.* , A.* from Question  as Q Join Answer as A on Q.Question_Id = A.Question_Id Join Section as S on S.Section_Id = Q.Section_Id Join Exam as E on E.Exam_Id = S.Exam_Id  Join Result as R on R.Exam_Id = E.Exam_Id  where R.Result_Id = "+ResultId+" and S.Section_Id  = "+section.Section_Id+" Order by Q.Question_Order Asc");

                            if (dtquestion.Rows.Count > 0)
                            {
                                exam.Sections[i].Questions = new Question[dtquestion.Rows.Count];
                                for (int j = 0; j < dtquestion.Rows.Count; j++)
                                {
                                    Question question = new Question();
                                    question.Question_Id = (int)dtquestion.Rows[j]["Question_Id"];
                                    if (dtquestion.Rows[j]["Question_Type"].ToString().Equals("FB"))
                                    { question.QuestionText = StudentController.ConvertintoFB(dtquestion.Rows[j]["Question"].ToString()); }
                                    else
                                    { question.QuestionText = dtquestion.Rows[j]["Question"].ToString(); }
                                    question.Type = dtquestion.Rows[j]["Question_Type"].ToString();
                                    question.Question_Marks = (double)dtquestion.Rows[j]["Question_Marks"];
                                    question.Question_Order = (int)dtquestion.Rows[j]["Question_Order"];
                                    question.Sample_Answer = (String)dtquestion.Rows[j]["Answer"];
                                    question.Obtained_Marks = (double)dtquestion.Rows[j]["Obtain_Marks"];
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
                else
                {
                    ViewBag.ErrorHeading = "Invalid Exam Access";
                    return View("~/Views/Shared/Error.cshtml");
                }
            }
            else
            {
                ViewBag.ErrorHeading = "Invalid Result Access";
                return View("~/Views/Shared/Error.cshtml");
            }
        }



        [HttpPost]
        [MyCustomFilter]
        public ActionResult UpdateCourse(String CourseId)
        {
            return null;    
        }


        [HttpGet]
        [MyCustomFilter]
        public ActionResult NewCourse()
        {
            return View();
        }


        [HttpPost]
        [MyCustomFilter]
        public ActionResult NewCourse(Course Course)
        {
            try
            {
                DataAccess.clearparam();
                DataAccess.addParam("Teacher_Id", ((Teacher)Session["CurrentUser"]).Id.ToString());
                DataAccess.addParam("Name", Course.Name.ToString());
                DataAccess.addParam("Enroll_Id", Course.Enroll_id.ToString());
                DataAccess.addParam("Enroll_Password", Course.Enroll_Password.ToString());
                DataAccess.addParam("Description", Course.Description.ToString());
                DataAccess.addParam("Status", Course.Status);
                DataAccess.Execute("insert into Courses (Course_Id,Teacher_Id,Course_name,Enroll_Id,Enroll_Password,Date_Created,Status) values ((select isnull(max(Course_Id),0)+1 from Courses ),@Teacher_Id,@Name,@Enroll_Id , @Enroll_Password,GetDate(),'Active')", true, false);
                String Cid = DataAccess.Reader("select Course_Id from Courses where Enroll_Id = @Enroll_Id and Enroll_Password = @Enroll_Password", true, false).ToString();
                return RedirectToAction("Course", "Teacher", new { CourseId = Cid });
            }
            catch (Exception ex)
            { return View(Course); }
            
        }



        [HttpGet]
        [MyCustomFilter]
        public ActionResult Results(String CourseId , String ExamId )
        {
            List<Result> list = new List<Result>();
            try
            {
                String Query = "select * from Result as R Join Exam as E on R.Exam_Id = E.Exam_Id Join Courses as C on C.Course_Id = E.Course_Id join Student as S on R.Student_Id = S.id";
                if (CourseId != null && ExamId != null)
                { Query = Query + "where C.Course_Id = " + CourseId + " and E.Exam_Id = " + ExamId; }
                else if (CourseId != null)
                { Query = Query + "where C.Course_Id = " + CourseId; }
                else if (ExamId != null)
                { Query = Query + "where E.Exam_Id = " + ExamId; }

                DataTable dt = DataAccess.daobj(Query, false, false);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Result Result = new Result();
                        Result.ResultId = dt.Rows[i]["Result_Id"].ToString();
                        Result.Course_Name = dt.Rows[i]["Course_name"].ToString();
                        Result.Exam_Name = dt.Rows[i]["Exam_Name"].ToString();
                        Result.Student_Name = dt.Rows[i]["full_name"].ToString();
                        Result.StudentId = dt.Rows[i]["Student_Id"].ToString();
                        Result.Total_Marks = double.Parse(dt.Rows[i]["Total_Marks"].ToString());
                        Result.Obtain_Marks = double.Parse(dt.Rows[i]["Obtain_Marks"].ToString());
                        Result.Grade = dt.Rows[i]["Grade"].ToString();
                        Result.Remarks = dt.Rows[i]["Remarks"].ToString();
                        Result.Percentage = double.Parse(dt.Rows[i]["Percentage"].ToString());
                        list.Add(Result);
                    }
                }
                return View(list);
            }
            catch (Exception ex)
            { return RedirectToAction("Error", "Account"); }

        }





	}




}