using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HawkSync_RC.classes;
using HawkSync_RC.classes.RCClasses;
using log4net;
using Newtonsoft.Json;

namespace HawkSync_RC.TVFunctions
{
    public class rotationManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AppState _state;
        private readonly int ArrayID = -1;
        private Dictionary<dynamic, dynamic> json_array = new Dictionary<dynamic, dynamic>();
        private readonly RCSetup RCSetup;

        public rotationManager(RCSetup setup, AppState state, int InstanceID)
        {
            RCSetup = setup;
            _state = state;
            ArrayID = InstanceID;
        }

        public OpenClass.Status DeleteRotation(int rotationID)
        {
            var sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.RotationManager.DeleteRotation" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].instanceID },
                { "RotationID", rotationID }
            };
            var sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            var bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            var reply = RCSetup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            var responseArray =
                JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(
                    Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            return (OpenClass.Status)responseArray["Status"];
        }

        public OpenClass.Status CreateRotation(List<MapList> rotation, string description)
        {
            var sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.RotationManager.CreateRotation" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].instanceID },
                { "Rotation", JsonConvert.SerializeObject(rotation) },
                { "description", description }
            };
            var sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            var bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            var reply = RCSetup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            var responseArray =
                JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(
                    Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            return (OpenClass.Status)responseArray["Status"];
        }

        public OpenClass.Status UpdateRotation(List<MapList> selectedMaps, string description, int rotationID)
        {
            var sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.RotationManager.UpdateRotation" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].instanceID },
                { "Rotation", JsonConvert.SerializeObject(selectedMaps) },
                { "description", description }
            };
            var sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            var bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            var reply = RCSetup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            var responseArray =
                JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(
                    Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            return (OpenClass.Status)responseArray["Status"];
        }
    }
}