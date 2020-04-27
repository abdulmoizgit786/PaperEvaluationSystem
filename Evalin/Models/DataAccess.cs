using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Evalin.Models
{
    public class DataAccess
    {
        public static readonly string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DBEvalin"].ConnectionString;
        public static Dictionary<string,SqlParameter> param = new Dictionary<String,SqlParameter>();
        public static SqlConnection con = new SqlConnection(constr);
        public static SqlTransaction transaction = null;

        public static void clearparam()
        { param.Clear(); }

        public static DateTime GetDate()
        {
            return (DateTime)DataAccess.Reader("select GETDATE()");
        }

        public static void addParam(string name, object value)
        {   
            if (param.ContainsKey(name))
            { param[name].Value = value; }
            else
            { SqlParameter p = new SqlParameter(name, value);
            param.Add(name, p);}
        }


        //public static DataTable daobj(string query,Boolean WithParameters = false,Boolean IsStoredProcedure = false)
        //{
        //    using (SqlCommand com = new SqlCommand(query,con))
        //    {
        //        if (WithParameters && param.Count > 0)
        //        {
        //            foreach (string key in param.Keys)
        //            { com.Parameters.AddWithValue(key, param[key].Value); }
        //        }
        //        SqlDataAdapter adap = new SqlDataAdapter(com);
        //        DataTable dt = new DataTable();
        //        adap.Fill(dt);
        //        return dt;
        //    }
        //}


        public static DataTable daobj(string query, Boolean WithParameters = false, Boolean IsStoredProcedure = false)
        {
            try
            {
                using (SqlCommand com = new SqlCommand(query, con))
                {
                    if (WithParameters && param.Count > 0)
                    {
                        foreach (string key in param.Keys)
                        { com.Parameters.AddWithValue(key, param[key].Value); }
                    }
                    if (transaction != null)
                    { com.Transaction = transaction; }

                    SqlDataAdapter adap = new SqlDataAdapter(com);
                    DataTable dt = new DataTable();
                    adap.Fill(dt);
                    return dt;
                }
            }
            catch (Exception e)
            {
                if (transaction != null)
                { TransactionRollback(); }
                throw new Exception(e.Message);
            }
        }

      
        public static void Execute(String query,Boolean WithParameters = false,Boolean WithTransaction = false)
        {
            try
            {
                using (SqlCommand com = new SqlCommand(query, con))
                {
                    if (WithParameters && param.Count > 0)
                    {
                        foreach (string key in param.Keys)
                        { com.Parameters.AddWithValue(key, param[key].Value); }
                    }
                    if (con.State == System.Data.ConnectionState.Closed)
                    { con.Open(); }

                    if (WithTransaction)
                    {
                        if (transaction == null)
                        { transaction = con.BeginTransaction(); }
                        com.Transaction = transaction;
                    }
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                TransactionRollback();
                throw new Exception(e.Message);
            }
            finally
            { if (transaction == null) { con.Close(); } }
        }

        public static void TransactionCommit()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
                if (con.State != ConnectionState.Closed)
                { con.Close(); }
            }
        }

        public static void TransactionRollback()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
                if (con.State != ConnectionState.Closed)
                { con.Close(); }
            }
        }



        public static Object Reader(String query, Boolean WithParameters = false, Boolean IsStoredProcedure = false)
        {
            try
            {
               using(SqlCommand com = new SqlCommand(query, con))
               { 
                if (con.State == System.Data.ConnectionState.Closed)
                { con.Open(); }
                if (WithParameters && param.Count > 0)
                { 
                    foreach (string key in param.Keys)
                    { com.Parameters.AddWithValue(key, param[key].Value); }
                }
                if (transaction != null)
                {
                    com.Transaction = transaction;
                }
                return com.ExecuteScalar();
               }
            }
            catch (Exception e)
            {
                if (transaction != null)
                {
                    TransactionRollback();
                    throw new Exception(e.Message); 
                }
                return null; 
            }
            finally
            { if (transaction == null) { con.Close(); } }
        }


        public static string DataTableToJSON(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }

         public static List<Course> GetCourses(String Id, String Type , String Cid = "")
         {
             List<Course> lc = new List<Course>();
             DataTable dt = new DataTable();
             DataAccess.clearparam();
             DataAccess.addParam("id", Id.ToString().Trim());
             if (Type.Equals("Teacher"))
             { dt = DataAccess.daobj("select * from Courses where Teacher_Id = @id  and status = 'Active' "+ (String.IsNullOrEmpty(Cid)? "":" and Course_Id = '"+Cid+"'" ), true, false); }
             else if (Type.Equals("Student"))
             { dt = DataAccess.daobj("select E.Course_Id , C.Course_name from Enrollement as E Join Courses as C on E.Course_Id = C.Course_Id where E.Student_Id = @id and E.Status = 'Active'" + (String.IsNullOrEmpty(Cid) ? "" : " and Course_Id = '" + Cid + "'"), true, false); }
             for (Int16 i = 0; i < dt.Rows.Count; i++)
             {
                 lc.Add(new Course() { Name = dt.Rows[i]["Course_name"].ToString(), Course_Id = Int32.Parse(dt.Rows[i]["Course_Id"].ToString()) });
             }
             return lc;
         }

    }
    

}
