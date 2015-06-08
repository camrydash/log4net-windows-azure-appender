using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net.Core;
using Log4net4zw.Persistence;
using Microsoft.WindowsAzure.Storage;


namespace Log4net4zw.Appenders
{
    public abstract class AzureAppenderBase : log4net.Appender.AppenderSkeleton
    {
        public String DataConnectionString { get; set; }
        public String ClientName { get; set; }
        public String ApplicationName { get; set; }
        public String InstanceName {get;set;}
        protected CloudStorageAccount StorageAccount;
        protected Boolean IsInitialized;      

        public virtual void Initialize()
        {
            try
            {
                System.Diagnostics.Debug.Write("Initialized");
                var gc = log4net.GlobalContext.Properties["DataConnectionString"] as String;
                if (!String.IsNullOrEmpty(gc))
                {
                    DataConnectionString = gc;
                }
                var _InstanceName = log4net.GlobalContext.Properties["InstanceName"] as String;
                if (!String.IsNullOrEmpty(_InstanceName))
                {
                    this.InstanceName = _InstanceName;
                }
                StorageAccount = CloudStorageAccount.Parse(DataConnectionString);
                SubInitialize();
                IsInitialized = true;
                return;
            }
            catch(Exception exp)
            {
                System.Diagnostics.Trace.WriteLine(exp.Message);
                System.Diagnostics.Trace.WriteLine(exp.StackTrace);
            }
            IsInitialized = false;
            
        }

        public abstract void SubInitialize();

        protected override void Append(LoggingEvent loggingEvent)
        {            
            if (!IsInitialized)
                Initialize();
            if (IsInitialized)
            {
                
                AppLog al = new AppLog(loggingEvent, this.ClientName, this.ApplicationName, this.InstanceName);
                
                System.Threading.Tasks.Task task = new System.Threading.Tasks.Task((obj) =>
                    {
                        var appLog = obj as AppLog;
                        do
                        {
                            int retry = 0;
                            try
                            {
                                this.AppendAsync(appLog);
                            }
                            catch (Microsoft.WindowsAzure.Storage.StorageException se)
                            { 
                                retry++;
                                if (retry <= 2)
                                {
                                    System.Threading.Thread.Sleep(retry * 10000);
                                    continue;
                                }
                                System.Diagnostics.Trace.WriteLine(se.Message);
                                System.Diagnostics.Trace.WriteLine(se.StackTrace);
                            }
                            catch (Exception exp)
                            {
                                System.Diagnostics.Trace.WriteLine(exp.Message);
                                System.Diagnostics.Trace.WriteLine(exp.StackTrace);
                            }
                            break;
                        } while (true);
                    }, al, System.Threading.Tasks.TaskCreationOptions.PreferFairness);

                task.Start();
                task.Wait(10);
            }
           

        }

        protected abstract void AppendAsync(AppLog appLog);
        

    }
}
