using CommunicationService.Communicator;
using CommunicationService.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatorService
{
    public enum CommunicatorType
    {
        MappedFileType
    }
    public class Service
    {
        //Add Log feature
        //private static ILogger logger = LoggerManager.GetCurrentClassLogger();
        private static Service instance = new Service();
        public static Service Instance { get { return instance; } }

        private const string KEY_FORMAT = "{0}_{1}_{2}{3}";
        private Dictionary<string, ICommunicator> commList = new Dictionary<string, ICommunicator>(StringComparer.OrdinalIgnoreCase);
        private object lockObj = new object();
        private ICommunicator CreateCommunicator(CommunicatorType type, string user, string env, string application, bool isFromApplicatonProcess = false)
        {
            var key = string.Format(KEY_FORMAT, user, env, application, isFromApplicatonProcess ? "_App" : "");
            ICommunicator comm = null;
            switch (type)
            {
                case CommunicatorType.MappedFileType:
                    comm = new MemoryMappedFileCommunicator(user, env, application, isFromApplicatonProcess);
                    break;
                default:
                    break;
            }
            if (comm != null)
            {
                lock (lockObj)
                {
                    commList.Add(key, comm);
                }
                comm.StartReader();
            }
            //logger.DebugFormat("CreateCommunicator, Key = {0}", key);
            return comm;
        }
        private ICommunicator GetCommunicator(string user, string env, string application, bool isFromApplicatonProcess = false)
        {
            var key = string.Format(KEY_FORMAT, user, env, application, isFromApplicatonProcess ? "_App" : "");
            lock (lockObj)
            {
                if (!commList.ContainsKey(key))
                {
                    CreateCommunicator(CommunicatorType.MappedFileType, user, env, application, isFromApplicatonProcess);
                }
                return commList.ContainsKey(key) ? commList[key] : null;
            }

        }
        public void Subscrible(string user, string env, string application, Action<CommunicatorEventArgs> action = null, bool isFromApplicatonProcess = false)
        {
            ICommunicator comm = GetCommunicator(user, env, application, isFromApplicatonProcess);
            if (comm != null && action != null)
                comm.DataReceived += action;
        }
        public void Publish(string user, string env, string application, IMessage data, bool isFromApplicatonProcess = false)
        {
            var comm = GetCommunicator(user, env, application, isFromApplicatonProcess);
            if (comm != null)
            {
                try
                {
                    comm.Write(data, data.Kind);
                    //logger.InfoFormat("CommunicateService Publish, User = {0}, Env  = {1}, Application = {2}, MessageKind= {3}, FromApplicatonProcess = {4}", user, env, application, data.Kind, isFromApplicatonProcess);
                }
                catch
                {
                    //logger.ErrorFormat("CommunicateService Publish throws exception:{5}, User = {0}, Env  = {1}, Application = {2}, MessageKind= {3}, FromApplicatonProcess = {4}", user, env, application, data.Kind, isFromApplicatonProcess, ex.Message);
                }

            }
        }
    }
}
