using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Log4net4zw.Appenders
{
    public class BlobAppender : AzureAppenderBase
    {        
        public String ContainerName { get; set; }
        public String DirectoryName { get; set; }        
        protected Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer Container;       
        
        protected virtual Microsoft.WindowsAzure.Storage.Blob.CloudPageBlob getBlob(out long startOffset)
        { 
            startOffset = 0;
                
                var pb = Container.GetDirectoryReference(this.DirectoryName).GetPageBlobReference(String.Format("{0}.txt",DateTime.UtcNow.ToString("yyyy-MM-dd")));

                if (!pb.Exists())
                {

                    pb.Properties.ContentType = "application/json";
                    pb.Metadata.Add("source", "log4net4zw");
                    pb.Metadata.Add("application", ApplicationName);

                    pb.Create(1024 * 1);
                    pb.SetProperties();
                    pb.SetMetadata();
                }
                else
                {

                    startOffset = pb.GetPageRanges().DefaultIfEmpty(new Microsoft.WindowsAzure.Storage.Blob.PageRange(-1, -1)).LastOrDefault().EndOffset + 1;
                }

                return pb;
            
        }

        protected override void AppendAsync(Persistence.AppLog appLog)
        {
            long lastOffset = 0;

            System.IO.Stream stream = null;
            try
            {

                var pb = this.getBlob(out lastOffset);
                var data = System.Text.Encoding.UTF8.GetBytes
                    (String.Format(",{0}", Newtonsoft.Json.JsonConvert.SerializeObject(appLog)));


                if ((pb.Properties.Length - 1024 - lastOffset - data.Length) < 0)
                    pb.Resize(pb.Properties.Length + 1024 * 1024);
                Byte[] page = null;
                int startIndex = 0;
                var maxSize = 1024 * 1024 * 4;
                do
                {
                    page = data.Skip(startIndex).Take(maxSize).ToArray();
                    var size = (int)Math.Ceiling(page.Length / 512d) * 512;
                    if (page.Length < size)
                        Array.Resize<Byte>(ref page, size);
                    using (var ms = new MemoryStream(page))
                    {
                        pb.WritePages(ms, lastOffset);
                    }
                    startIndex += page.Length;
                    lastOffset += page.Length;
                } while (page != null && page.Length == maxSize);
            }            
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        public override void SubInitialize()
        {
            Container = this.StorageAccount.CreateCloudBlobClient().GetContainerReference(ContainerName); 
        }

       
    }
}
