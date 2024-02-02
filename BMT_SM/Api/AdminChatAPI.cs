using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web;
using System.Web.Http;
using System.Data.SQLite;

namespace BMT_TV.Api
{
    public class AdminChatAPIController : ApiController
    {
        public HttpResponseMessage Get()
        {
            // since we don't have a TimeStamp, only show the last 5 messages...
            var Response = Request.CreateResponse();
            List<AdminChatMsgs> logs = new List<AdminChatMsgs>();
            var MsgList = GlobalAppState.AppState.adminChatMsgs.Skip(Math.Max(0, GlobalAppState.AppState.adminChatMsgs.Count - 5)).Take(5);
            foreach (var item in MsgList)
            {
                logs.Add(item);
            }

            Response.Content = new StringContent(JsonConvert.SerializeObject(logs), Encoding.Default, "application/json");
            return Response;
        }

        public HttpResponseMessage Get(string timestamp, string action, string userid)
        {
            var Response = Request.CreateResponse();
            if (action == "") // assume this is INDEX. Therefore run actions LOAD.
            {
                // fuck it, return all messages, let jQuery sort it out. lol
                Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.adminChatMsgs), Encoding.Default, "application/json");
                return Response;
            }
            else
            {
                return Response;
            }
        }
        public HttpResponseMessage Post(string msg, string userid)
        {
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>();
            var Response = Request.CreateResponse();
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand countcmd = new SQLiteCommand("SELECT * FROM `adminchatlog`;", db);
            SQLiteDataReader countreader = countcmd.ExecuteReader();
            int counter = 1;
            if (countreader.HasRows)
            {
                while (countreader.Read())
                {
                    counter++; // this will increment per row in DB
                }
            }
            countreader.Close();
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `adminchatlog` (`id`, `userid`, `msg`, `datesent`) VALUES (@id, @userid, @msg, @datesent);", db);
            cmd.Parameters.AddWithValue("@id", counter);
            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.Parameters.AddWithValue("@msg", msg);
            cmd.Parameters.AddWithValue("@datesent", DateTime.Now);
            cmd.ExecuteNonQuery();
            db.Close(); // close to prevent DB locking

            // just for testing purposes...
            try
            {
                GlobalAppState.AppState.adminChatMsgs.Add(new AdminChatMsgs
                {
                    MsgID = counter,
                    UserID = Convert.ToInt32(userid),
                    Msg = msg
                });
                reply.Add("success", true);
                reply.Add("msg", "Message submitted successfully!");
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            catch (Exception e)
            {
                reply.Add("success", false);
                reply.Add("msg", "Something went wrong!");
                reply.Add("error", e);
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            return Response;
        }
    }
}
