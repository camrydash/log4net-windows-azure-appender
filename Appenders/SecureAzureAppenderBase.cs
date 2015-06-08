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
    //public abstract class SecureAzureAppenderBase : log4net.Appender.AppenderSkeleton
    //{        
    //    public String ClientName { get; set; }
    //    public String ApplicationName { get; set; }
    //    public String EndpointAddress { get; set; }
    //    public String InstanceName { get; set; }
    //    protected CloudStorageAccount StorageAccount;
    //    protected Boolean IsInitialized;

    //    private SecureConnectionService.IConnectionService _ConnectionService;
    //    protected SecureConnectionService.IConnectionService ConnectionService
    //    {
    //        get
    //        {
    //            System.Threading.Monitor.Enter(this);
    //            if(_ConnectionService == null)
    //            {
    //                if (String.IsNullOrEmpty(this.EndpointAddress))
    //                {
    //                    _ConnectionService = new SecureConnectionService.ConnectionServiceClient();
    //                }
    //                else
    //                {
    //                    var endpoint = new System.ServiceModel.EndpointAddress(this.EndpointAddress);                        
    //                    var binding = new System.ServiceModel.BasicHttpBinding();
    //                    var service = new SecureConnectionService.ConnectionServiceClient(binding, endpoint);
    //                    service.Endpoint.Behaviors.Add(new SCP.WCFExtension.Behaviors.ClientAuthenticationEndpointBehavior());
    //                    _ConnectionService = service;
                        
    //                }
    //            }
    //            System.Threading.Monitor.Exit(this);
    //            return _ConnectionService;
    //        }
    //    }

    //    public virtual void Initialize()
    //    {
    //        try
    //        {                
    //            var _InstanceName = log4net.GlobalContext.Properties["InstanceName"] as String;
    //            if (!String.IsNullOrEmpty(_InstanceName))
    //            {
    //                this.InstanceName = _InstanceName;
    //            }                
    //            SubInitialize();
    //            IsInitialized = true;
    //            return;
    //        }
    //        catch(Exception exp)
    //        {
    //            if (exp != null)
    //            {
    //            }
    //        }
    //        IsInitialized = false;

    //    }

    //    public abstract void SubInitialize();

    //    protected override void Append(LoggingEvent loggingEvent)
    //    {
    //        if (!IsInitialized)
    //            Initialize();
    //        if (IsInitialized)
    //        {

    //            AppLog al = new AppLog(loggingEvent, this.ClientName, this.ApplicationName, this.InstanceName);

    //            System.Threading.Tasks.Task task = new System.Threading.Tasks.Task((obj) =>
    //            {
    //                var appLog = obj as AppLog;
    //                do
    //                {
    //                    int retry = 0;
    //                    try
    //                    {
    //                        this.AppendAsync(appLog);
    //                    }                            
    //                    catch (Microsoft.WindowsAzure.Storage.StorageException se)
    //                    {
    //                        retry++;
    //                        var we = se.InnerException as System.Net.WebException;
    //                        if (we != null)
    //                        {
    //                            var res = we.Response as System.Net.HttpWebResponse;
    //                            if (res != null && res.StatusCode == System.Net.HttpStatusCode.Forbidden)
    //                            {
    //                                this.Initialize();
    //                                if (retry <= 2)
    //                                    continue;

    //                            }
    //                        }
    //                        if (retry <= 2)
    //                        {
    //                            System.Threading.Thread.Sleep(retry * 10000);
    //                            continue;
    //                        }
    //                    }
    //                    catch
    //                    {
    //                    }
    //                    break;
    //                } while (true);
    //            }, al, System.Threading.Tasks.TaskCreationOptions.PreferFairness);

    //            task.Start();
    //            task.Wait(10);
    //        }


    //    }

    //    protected abstract void AppendAsync(AppLog appLog);


    //}
}
