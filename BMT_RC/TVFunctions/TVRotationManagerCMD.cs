using HawkSync_RC.classes;
using HawkSync_RC.classes.RCClasses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using WatsonTcp;

namespace HawkSync_RC.TVFunctions
{
    public class TVRotationManagerCMD
    {
        RCSetup RCSetup;
        AppState _state;
        int ArrayID = -1;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        Dictionary<dynamic, dynamic> json_array = new Dictionary<dynamic, dynamic>();
        public TVRotationManagerCMD(RCSetup setup, AppState state, int InstanceID)
        {
            RCSetup = setup;
            _state = state;
            ArrayID = InstanceID;
        }
        public OpenClass.Status DeleteRotation(int rotationID)
        {
            Dictionary<dynamic, dynamic> sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.RotationManager.DeleteRotation" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "RotationID", rotationID }
            };
            string sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            SyncResponse reply = RCSetup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            Dictionary<dynamic, dynamic> responseArray = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            return (OpenClass.Status)responseArray["Status"];
        }
        public OpenClass.Status CreateRotation(List<MapList> rotation, string description)
        {
            Dictionary<dynamic, dynamic> sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.RotationManager.CreateRotation" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "Rotation", JsonConvert.SerializeObject(rotation) },
                { "description", description }
            };
            string sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            SyncResponse reply = RCSetup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            Dictionary<dynamic, dynamic> responseArray = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            return (OpenClass.Status)responseArray["Status"];
        }

        public OpenClass.Status UpdateRotation(List<MapList> selectedMaps, string description, int rotationID)
        {
            Dictionary<dynamic, dynamic> sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.RotationManager.UpdateRotation" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "Rotation", JsonConvert.SerializeObject(selectedMaps) },
                { "description", description }
            };
            string sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            SyncResponse reply = RCSetup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            Dictionary<dynamic, dynamic> responseArray = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            return (OpenClass.Status)responseArray["Status"];
        }
    }
}
