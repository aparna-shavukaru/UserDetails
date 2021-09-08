using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using ScadaWebApi.Models;
using System.Reflection;
using System.Web;
using ScadaWebApi.Utils;
using System.IO;
using System.Web.Http.Cors;
using System.Text;

namespace ScadaWebApi.Controllers
{
    [Route("api/UserDetails")]     
    public class UserDetailsController : ApiController
    {
        private ECDBAZEEntities db = new ECDBAZEEntities();
        public string connectionString = WebConfigurationManager.ConnectionStrings["ECDBAZEEntities"].ConnectionString;
        public string conString = WebConfigurationManager.ConnectionStrings["ECSCadaCon"].ConnectionString;

        string SenderEmail = System.Configuration.ConfigurationSettings.AppSettings["SenderEmail"].ToString();
        // SenderEmail
        [HttpPost]
        // POST: api/UserDetails
        [ResponseType(typeof(UserDetail))]
        public IHttpActionResult PostUserDetail(UserDetail userDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // validation
            if (string.IsNullOrWhiteSpace(userDetail.Password))
                throw new Exception("Password is required");

            //checks EmpCode exists or not -  Employee code should be unique
            if (db.UserDetails.Any(x => x.EmpCode == userDetail.EmpCode))
                throw new Exception("EmpCode \"" + userDetail.EmpCode + "\" is already exists");

            //checks username exists or not -  Username code should be unique
            if (db.UserDetails.Any(x => x.Username == userDetail.Username))
                throw new Exception("Username \"" + userDetail.Username + "\" is already exists");
            string EmailID = userDetail.EmailID;
            string Subject = "";
            string Message = "";
            try
            {
                if (!string.IsNullOrEmpty(EmailID))
                {
                    //  SendMail(userDetail.EmailID,  Subject, Message )
                }
            }
            catch (Exception ex) { }
            userDetail.Password = Encryptdata(userDetail.Password);
            db.UserDetails.Add(userDetail);
            db.SaveChanges();

            CreatedAtRoute("DefaultApi", new { id = userDetail.UserID }, userDetail);
            return Ok(userDetail);
        }
        private string Encryptdata(string password)
        {
            string strmsg = string.Empty;
            byte[] encode = new byte[password.Length];
            encode = Encoding.UTF8.GetBytes(password);
            strmsg = Convert.ToBase64String(encode);
            return strmsg;
        }

        //public IEnumerable<UserDetail> GetUserDetailsOld()
        //{
        //    return db.UserDetails.ToList();
        //}
        [HttpGet]
        // GET: api/UserDetails
        public IEnumerable<UserDetail> GetUserDetails()
            {
            // return db.UserDetails.ToList();
            List<Models.UserDetail> GetUsersList = new List<UserDetail>();

            SqlConnection Connection = new SqlConnection(conString);
            System.Data.DataTable dt = new System.Data.DataTable();
            try

            {
                Connection.Open();
                string CmdTxt = "PR_GET_USERDETAILS";
                SqlCommand Command = new SqlCommand(CmdTxt, Connection);
                Command.CommandType = System.Data.CommandType.StoredProcedure;
                Command.Parameters.Add(new SqlParameter("@USERID", 0));
                dt.Load(Command.ExecuteReader());
            }
            catch (Exception e)
            {
                //this.Result = -1;
                //this.ErrorMessage = e.Message;
            }
            finally
            {
                Connection.Close();
            }
            GetUsersList = ConvertDataTable<UserDetail>(dt);
            GetUsersList = GetUsersList.Select(s => new UserDetail
            {
                UserID = s.UserID,
                FirstName = s.FirstName,
                LastName = s.LastName,
                EmpCode = s.EmpCode,
                Gender = s.Gender,
                DOB = s.DOB,
                Department = s.Department,
                ReportingManager = s.ReportingManager,
                ReportingManagerID = s.ReportingManagerID,
                Mobile = s.Mobile,
                AlternatePhone = s.AlternatePhone,
                EmailID = s.EmailID,
                Address = s.Address,
                DateofJoining = (s.DateofJoining),
                DateofRelieving = s.DateofRelieving,
                RoleID = s.RoleID,
                Username = s.Username,
                Password = s.Password,
                InsertedBy = s.InsertedBy,
                InsertedDate = s.InsertedDate,
                ModifiedDate = s.ModifiedDate
              //  JoingDate=s.JoingDate
,
            })
            .ToList();
            //var itemgroupname = GetGroupNamesList.RemoveAll(x => x.CURTRENDTITLE == null);
            return (GetUsersList);

        }


        [HttpPut]
        // PUT: api/UserDetails/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserDetail(UserDetail userDetail)
        {
            int id = userDetail.UserID;
            int Result = 0;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != userDetail.UserID)
            {
                return BadRequest();
            }
            // db.Entry(userDetail).State = EntityState.Modified;
            try
            {
                /*
                db.UserDetails.Attach(userDetail);
                db.SaveChanges();
                
                //checks EmpCode exists or not -  Employee code should be unique
                if (db.UserDetails.Any(x => x.EmpCode == userDetail.EmpCode))
                    throw new Exception("EmpCode \"" + userDetail.EmpCode + "\" is already exists");
                */
                SqlConnection Connection = new SqlConnection(conString);
                Connection.Open();
                bool isError = false;
                SqlTransaction objTrans = Connection.BeginTransaction();
                try
                {
                    SqlCommand Command = new SqlCommand("PR_INSERTUPDATE_USERDETAILS", Connection);
                    Command.CommandType = System.Data.CommandType.StoredProcedure;
                    Command.Transaction = objTrans;// Begin traction 
                    Command.Parameters.Add(new SqlParameter("@UserID", id));
                    Command.Parameters.Add(new SqlParameter("@FirstName", userDetail.FirstName));
                    Command.Parameters.Add(new SqlParameter("@LastName", userDetail.LastName));
                    Command.Parameters.Add(new SqlParameter("@EmpCode ", userDetail.EmpCode));
                    Command.Parameters.Add(new SqlParameter("@Gender", userDetail.Gender));
                    Command.Parameters.Add(new SqlParameter("@DOB", userDetail.DOB == DateTime.MinValue ? null : userDetail.DOB.ToString()));
                    //Command.Parameters.Add(new SqlParameter("@DOB", userDetail.DOB));
                    Command.Parameters.Add(new SqlParameter("@Department", userDetail.Department));
                    Command.Parameters.Add(new SqlParameter("@ReportingManager", userDetail.ReportingManager));
                    Command.Parameters.Add(new SqlParameter("@ReportingManagerID", userDetail.ReportingManagerID));
                    Command.Parameters.Add(new SqlParameter("@Mobile", userDetail.Mobile));
                    Command.Parameters.Add(new SqlParameter("@AlternatePhone", userDetail.AlternatePhone));
                    Command.Parameters.Add(new SqlParameter("@EmailID", userDetail.EmailID));
                    Command.Parameters.Add(new SqlParameter("@Address", userDetail.Address));
                    Command.Parameters.Add(new SqlParameter("@DateofJoining", userDetail.DateofJoining == DateTime.MinValue ? null : userDetail.DateofJoining.ToString()));
                    Command.Parameters.Add(new SqlParameter("@DateofRelieving", userDetail.DateofRelieving == DateTime.MinValue ? null : userDetail.DateofRelieving.ToString()));
                   // Command.Parameters.Add(new SqlParameter("@JoingDate", userDetail.JoingDate));
                    //Command.Parameters.Add(new SqlParameter("@DateofRelieving", userDetail.DateofRelieving));
                    Command.Parameters.Add(new SqlParameter("@RoleID", userDetail.RoleID));
                    Command.Parameters.Add(new SqlParameter("@ActiveStatus", userDetail.ActiveStatus));
                    SqlDataReader rdr = Command.ExecuteReader();
                    while (rdr.Read())
                    {
                        Result = rdr.GetInt32(0);
                        // UserDetail.UserID = rdr.GetString(1);
                    }
                    rdr.Close();
                    isError = Result > 0 ? false : true;


                    if (isError)
                        objTrans.Rollback();
                    else
                        objTrans.Commit();// End Transaction

                }
                catch (Exception e)
                {
                    Result = -1;
                }
                finally
                {
                    Connection.Close();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserDetailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

     
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserDetailExists(int id)
        {
            return db.UserDetails.Count(e => e.UserID == id) > 0;
        }

        [HttpGet]
        //[Route("UserDetailsByID")]
        // GET: api/UserAuthentication/5
        [ResponseType(typeof(UserDetail))]
        public IHttpActionResult UserDetails(int id)
        {
            UserDetail userDetail = db.UserDetails.Find(id);
            if (userDetail == null)
            {
                return NotFound();
            }

            return Ok(userDetail);
        }


        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);


            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)// && column.ColumnName != DBNull.Value)
                        if (dr[column.ColumnName] != DBNull.Value )
                            pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        private void SendUserCredentials(string UserName, string Password, string Phone, string EmailID, string FirstName, string LastName)
        {
            if (EmailID != "")
            {
                Services.SendEmail dcEmail = new Services.SendEmail();
                string Date = DateTime.Now.ToShortDateString();
                SendUserRegistrationMail(EmailID, "::1", UserName,Password, FirstName, LastName);

            } 
        }

        /// <summary>
        /// To Send Temporary User Credentials through Email (by Gayathri.M on 18/04/2018) 
        /// </summary>
        /// <param name="ProviderEmail"></param>
        /// <param name="ClientIpAddress"></param>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <param name="FirstName"></param>
        /// <param name="LastName"></param>
        internal void SendUserRegistrationMail(string ProviderEmail, string ClientIpAddress, string UserName, string Password, string FirstName, string LastName)
        {
            try
            {
                string message = "Welcome to  ECIL Team";
                string emoName = "EC Scada Application";
                string domainURL = "";
                string body = ReadFile(VirtualPathUtility.MakeRelative(VirtualPathUtility.ToAppRelative(System.Web.HttpContext.Current.Request.CurrentExecutionFilePath), "/Utils/MailtoUserAfterUserCreation-Scada.html"));
                body = body.Replace("##ReferringProvider##", GlobalFunctions.GetCamelCaseString(FirstName + " " + LastName));
                body = body.Replace("##A##", "");
                body = body.Replace("##UserName##", UserName);
                body = body.Replace("##Password##", Password);
                body = body.Replace("##Specialty##", "");
                body = body.Replace("##Date##", DateTime.Now.ToShortDateString());
                body = body.Replace("##IPAddress##", ClientIpAddress);
                body = body.Replace("##SignedUPDate##", DateTime.Now.ToString());
                body = body.Replace("##NAME##", Convert.ToString(emoName));
                body = body.Replace("##DOMAIN_URL##", Convert.ToString(domainURL));
                Services.SendEmail _objMail = new Services.SendEmail();
                _objMail.SmtpMail(ProviderEmail, "", message, body);
            }
            catch (Exception ex)
            {
                #region Exception
                //ErrorLog Err = new ErrorLog();
                //System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
                //string str = trace.GetFrame(0).GetMethod().ReflectedType.FullName;
                //Err.ErrorsEntry(ex.ToString(), ex.Message, ex.GetHashCode(), str, trace.GetFrame(0).GetMethod().Name);
                #endregion
            }
        }

        public static string ReadFile(string fileName)
        {
            try
            {
                String filename = HttpContext.Current.Server.MapPath(fileName);
                StreamReader objStreamReader = File.OpenText(filename);
                String contents = objStreamReader.ReadToEnd();
                return contents;
            }
            catch (Exception ex)
            {
                #region Exception
                //ErrorLog Err = new ErrorLog();
                //System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
                //string str = trace.GetFrame(0).GetMethod().ReflectedType.FullName;
                //Err.ErrorsEntry(ex.ToString(), ex.Message, ex.GetHashCode(), str, trace.GetFrame(0).GetMethod().Name);
                #endregion
            }
            return "";
        }


    }
}