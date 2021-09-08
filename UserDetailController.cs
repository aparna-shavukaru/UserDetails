
using System.Web;
using System.Web.Mvc;
using ScadaWebApi.Models;
using ScadaWebApi.Gateway;
using System.Web.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Web.Http;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Web.Http.Cors;

namespace ScadaWebApi.Controllers
{
   [RoutePrefix("api/UserDetail")]
   // [EnableCors(origins: "http://183.82.4.93:5887/ECScadaTrends", headers: "*", methods: "get,post")]
    //[EnableCors("*","*","*")]
    public class UserDetailController : ApiController
    {
        private ECDBAZEEntities db = new ECDBAZEEntities();
        public string connectionString = WebConfigurationManager.ConnectionStrings["ECDBAZEEntities"].ConnectionString;
        readonly string conString = WebConfigurationManager.ConnectionStrings["ECSCadaCon"].ConnectionString;

        // GET: api/UserDetail
       
        [System.Web.Http.HttpGet]
        public IEnumerable<UserDetail> Get(int? UserID)
        {
            List<Models.UserDetail> GetUsersList = new List<UserDetail>();
            
            SqlConnection Connection = new SqlConnection(conString);
            System.Data.DataTable dt = new System.Data.DataTable();
            try
           
            {
                Connection.Open();
                string CmdTxt = "PR_GET_USERDETAILS";
                SqlCommand Command = new SqlCommand(CmdTxt, Connection);
                Command.CommandType = System.Data.CommandType.StoredProcedure;
                Command.Parameters.Add(new SqlParameter("@USERID", UserID));


                dt.Load(Command.ExecuteReader());
            }
            catch(Exception e)
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
                DateofJoining = s.DateofJoining,
                DateofRelieving = s.DateofRelieving,
                RoleID = s.RoleID,
                ActiveStatus = s.ActiveStatus,
                Username = s.Username,
                Password=s.Password,
                InsertedBy=s.InsertedBy,
                InsertedDate=s.InsertedDate,
                ModifiedDate = s.ModifiedDate
               // JoingDate = s.JoingDate
,            })
            .ToList();
            var password = GetUsersList.FirstOrDefault().Password;
            var decryptedPassword = Decryptdata(password);
            //var itemgroupname = GetGroupNamesList.RemoveAll(x => x.CURTRENDTITLE == null);
            return (GetUsersList);

        }
        private string Decryptdata(string encryptpwd)
        {
            string decryptpwd = string.Empty;
            UTF8Encoding encodepwd = new UTF8Encoding();
            Decoder Decode = encodepwd.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encryptpwd);
            int charCount = Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            decryptpwd = new String(decoded_char);
            return decryptpwd;
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
                        if (dr[column.ColumnName] != DBNull.Value)
                            pro.SetValue(obj, dr[column.ColumnName], null);
                        else
                        continue;
                }
            }
            return obj;
        }


        // GET: api/UserDetails/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //    Gender,DOB,Department,ReportingManager,ReportingManagerID		 ,Mobile,AlternatePhone,EmailID,Address,DateofJoining,isnull(DateofRelieving,'01/01/1900')DateofRelieving,
        //    ROleID, ActiveStatus,InsertedBy, InsertedDate,ModifiedDate,Username,Password


        // POST: api/UserDetail/create
        // [System.Web.Http.Route("api/UserDetail/{FirstName}/{LastName}/{EmpCode}/{Gender}/{DOB}/{Department}/{ReportingManager}/{ReportingManagerID}/{Mobile}/{AlternatePhone}/{EmailID}/{Address}/{DateofJoining}/{DateofRelieving}/{RoleID}/{ActiveStatus}/{Username}/{Password}")]
        // [System.Web.Http.Route("UserDetail/{FirstName}/{LastName}/{EmpCode}/{Gender}/{DOB}/{Department}/{ReportingManager}/{ReportingManagerID}/{Mobile}/{AlternatePhone}/{EmailID}/{Address}/{DateofJoining}/{DateofRelieving}/{RoleID}/{ActiveStatus}/{Username}/{Password}")]
        //[System.Web.Http.Route("UserDetail?FirstName=FirstName&LastName=LastName")]
       // [System.Web.Http.Route("~/api/UserDetail/CreateUser")]
        [System.Web.Http.HttpPost]
        //public void Post([FromUri]string FirstName, string LastName, string EmpCode, string Gender, DateTime DOB, string Department
        //    , string ReportingManager, int ReportingManagerID, string Mobile, string AlternatePhone, string EmailID, string Address, DateTime DateofJoining, DateTime DateofRelieving
        //    , int RoleID, bool ActiveStatus, string Username, string Password)
        //public void Post([FromUri]string FirstName, string LastName)

        public void Post([FromBody] UserDetail userDetail)
        {
            int Result = 0;
            try
            {
                SqlConnection Connection = new SqlConnection(connectionString);
                Connection.Open();
                bool isError = false;
                SqlTransaction objTrans = Connection.BeginTransaction();
                try
                {
                    SqlCommand Command = new SqlCommand("PR_INSERTUPDATE_USERDETAILS", Connection);
                    Command.CommandType = System.Data.CommandType.StoredProcedure;
                    Command.Transaction = objTrans;// Begin traction 

                    Command.Parameters.Add(new SqlParameter("@UserID", 0));
                    Command.Parameters.Add(new SqlParameter("@FirstName", userDetail.FirstName));
                    Command.Parameters.Add(new SqlParameter("@LastName", userDetail.LastName)); 
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
            catch (Exception ex)
            {
                 
            }

        }

        // PUT: api/UserDetails/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/UserDetails/5
        public void Delete(int id)
        {
        }
    }
}
