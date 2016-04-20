/* 
   Licensed under the Apache License, Version 2.0 (the "License"); 
   you may not use this file except in compliance with the License. 
   You may obtain a copy of the License at 
 
       http://www.apache.org/licenses/LICENSE-2.0 
 
   Unless required by applicable law or agreed to in writing, software 
   distributed under the License is distributed on an "AS IS" BASIS, 
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
   See the License for the specific language governing permissions and 
   limitations under the License. 
*/
/* Source code from MSDN: https://code.msdn.microsoft.com/windowsdesktop/Inter-process-communication-e96e94e7 */
/*
 * My changes: 
 * 1. Change to use EventWaitHandle instead Thread.Sleep for read/write thread. Four signals per communicator, any better way?
 * 2. Cache MemoryMappedFileCommunicator for specific key in factory.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunicationService.Message;
namespace CommunicationService.Communicator
{
    public class MemoryMappedFileCommunicator : ICommunicator
    {
        #region Constants
        private const int DATA_OFFSET = 10;
        private const string WRITE = "WRITE";
        private const string READ = "READ";
        private const string EventWaitHandlePrefix = "Signal_{0}_{1}_{2}_{3}";
        private const string EventWaitHandleApplicationPrefix = "Signal_Application_{0}_{1}_{2}_{3}";
        #endregion

        #region Properties

        private MemoryMappedFile MappedFile { get; set; }
        private EventWaitHandle OwnWriteEventWaitHandle = null;
        private EventWaitHandle OppositeReadEventWaitHandle = null;
        private EventWaitHandle OppositeWriteEventWaitHandle = null;
        private EventWaitHandle OwnReadEventWaitHandle = null;

        public event Action<CommunicatorEventArgs> DataReceived;

        private int ReadPosition;
        private int WritePosition;

        private string User;
        private string Env;
        private bool IsFromApplicationProcess;

        private string application;
        private string Application
        {
            get { return application; }
            set
            {
                application = value;
                OwnWriteEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, string.Format(IsFromApplicationProcess ? EventWaitHandleApplicationPrefix : EventWaitHandlePrefix, User, Env, application, WRITE));
                OppositeReadEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, string.Format(IsFromApplicationProcess ? EventWaitHandlePrefix : EventWaitHandleApplicationPrefix, User, Env, application, READ));

                OppositeWriteEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, string.Format(IsFromApplicationProcess ? EventWaitHandlePrefix : EventWaitHandleApplicationPrefix, User, Env, application, WRITE));
                OwnReadEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, string.Format(IsFromApplicationProcess ? EventWaitHandleApplicationPrefix : EventWaitHandlePrefix, User, Env, application, READ));

                OppositeReadEventWaitHandle.Set();
            }
        }

        #endregion

        private MemoryMappedViewAccessor view;
        private AsyncOperation operation;
        private SendOrPostCallback callback;
        private bool started;
        private bool disposed;

        private List<byte[]> dataToSend;
        private bool writerThreadRunning;

        public MemoryMappedFileCommunicator(string user, string env, string application, bool isApplication = false, int capacity = 4096)
            : this(MemoryMappedFile.CreateOrOpen(string.Format("MemoryMappedFile_{0}_{1}_{2}", user, env, application), capacity), user, env, 0, capacity / 2, application, isApplication, MemoryMappedFileAccess.ReadWrite)
        {
        }
        public MemoryMappedFileCommunicator(MemoryMappedFile mappedFile, string user, string env, int writePosition, int readPosition, string application, bool isFromApplicatonProcess = false, MemoryMappedFileAccess access = MemoryMappedFileAccess.ReadWrite)
        {
            this.MappedFile = mappedFile;
            this.view = mappedFile.CreateViewAccessor(0, 0, access);
            this.IsFromApplicationProcess = isFromApplicatonProcess;
            this.User = user;
            this.Env = env;
            this.Application = application;
            this.ReadPosition = isFromApplicatonProcess ? writePosition : readPosition;
            this.WritePosition = isFromApplicatonProcess ? readPosition : writePosition;
            this.dataToSend = new List<byte[]>();

            this.callback = new SendOrPostCallback(OnDataReceivedInternal);
            this.operation = AsyncOperationManager.CreateOperation(null);

        }

        public void StartReader()
        {
            if (started)
                return;

            if (ReadPosition < 0 || WritePosition < 0)
                throw new ArgumentException();
            Task.Factory.StartNew(ReaderThread);
        }
        public void Write(IMessage data, MessageKind kind)
        {
            Write(new CommunicatorEventArgs(data, kind));
        }
        private object obj = new object();
        public void Write(CommunicatorEventArgs args)
        {
            if (ReadPosition < 0 || WritePosition < 0)
                throw new ArgumentException();

            lock (obj)
            {
                dataToSend.Add(args.Serialize());
                if (!writerThreadRunning)
                {
                    writerThreadRunning = true;
                    Task.Factory.StartNew(WriterThread);
                }
            }
        }

        private void WriterThread()
        {
            lock (obj)
            {
                while (dataToSend.Count > 0 && !disposed)
                {
                    byte[] data = null;
                    data = dataToSend[0];
                    dataToSend.RemoveAt(0);
                    OppositeReadEventWaitHandle.WaitOne();
                    // Sets length and write data. 
                    view.Write(WritePosition, data.Length);
                    view.WriteArray<byte>(WritePosition + DATA_OFFSET, data, 0, data.Length);
                    OwnWriteEventWaitHandle.Set();
                }
                writerThreadRunning = false;
            }
        }

        public void CloseReader()
        {
            started = false;
        }

        private void ReaderThread()
        {
            started = true;
            while (started)
            {

                OppositeWriteEventWaitHandle.WaitOne();

                // Checks how many bytes to read. 
                int availableBytes = view.ReadInt32(ReadPosition);
                var bytes = new byte[availableBytes];
                // Reads the byte array. 
                int read = view.ReadArray<byte>(ReadPosition + DATA_OFFSET, bytes, 0, availableBytes);

                OwnReadEventWaitHandle.Set();

                operation.Post(callback, bytes.Deserialize<CommunicatorEventArgs>());
            }
        }

        private void OnDataReceivedInternal(object state)
        {
            OnDataReceived((CommunicatorEventArgs)state);
        }

        protected virtual void OnDataReceived(CommunicatorEventArgs e)
        {
            if (!EqualityComparer<CommunicatorEventArgs>.Default.Equals(e, default(CommunicatorEventArgs)) && DataReceived != null)
                DataReceived(e);
        }

        #region IDisposable

        public void Dispose()
        {
            CloseReader();
            if (view != null)
            {
                try
                {
                    view.Dispose();
                    view = null;
                }
                catch { }
            }

            if (MappedFile != null)
            {
                try
                {
                    MappedFile.Dispose();
                    MappedFile = null;
                }
                catch { }
            }

            disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
