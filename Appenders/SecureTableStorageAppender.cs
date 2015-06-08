using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Log4net4zw.Appenders
{
    //public partial class SecureTableStorageAppender : SecureAzureAppenderBase
    //{
    //    public String TableName { get; set; }
    //    public String TableStorageBaseUri { get; set; }

    //    protected CloudTable _CloudTable = null;     

    //    public override void SubInitialize()
    //    {
    //        var sasToken = this.ConnectionService.GetTableSAS(this.ClientName, this.ApplicationName, this.TableName);
    //        var credential = new StorageCredentials(sasToken);
    //        var tableClient = new Microsoft.WindowsAzure.Storage.Table.CloudTableClient(new Uri(this.TableStorageBaseUri), credential);
    //        this._CloudTable = tableClient.GetTableReference(this.TableName);            
    //    }

    //    protected override void AppendAsync(Persistence.AppLog appLog)
    //    {
    //        TableOperation to = TableOperation.Insert(appLog);
    //        this._CloudTable.Execute(to);
    //    }
    //}
}
