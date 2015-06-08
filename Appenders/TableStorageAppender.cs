using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log4net4zw.Persistence;
using Microsoft.WindowsAzure.Storage.Table;

namespace Log4net4zw.Appenders
{

    public class TableStorageAppender : AzureAppenderBase
    {
        public String TableName { get; set; }

        protected CloudTable _CloudTable = null;        

        protected override void AppendAsync(AppLog appLog)
        {
            TableOperation to = TableOperation.Insert(appLog);
            this._CloudTable.Execute(to);
        }

        public override void SubInitialize()
        {
            var tableClient = this.StorageAccount.CreateCloudTableClient();
            this._CloudTable = tableClient.GetTableReference(this.TableName);
            this._CloudTable.CreateIfNotExists();
            
        }
    }
}
