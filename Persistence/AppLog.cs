using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Microsoft.WindowsAzure.Storage.Table;

namespace Log4net4zw.Persistence
{
    /// <summary>
    /// App Log entity dervied from Table Entity to log into Azure table and can be serialized to save in azure blob
    /// Here Partition Key is  String.Format("{0}-{1}", ApplicationName, LogDate.ToString("yyyy-MM-dd"))
    /// And Row Key is String.Format("{0}-{1}-{2}", LevelName, LogDate.ToString("HH:mm"),Guid)
    /// If Guid is null then String.Format("{0}-{1}-", LevelName, LogDate.ToString("HH:mm"))
    /// 
    /// </summary>
    public class AppLog : TableEntity
    {
        public AppLog() { }

        public AppLog(String _ApplicationName, String _LevelName, DateTime _LogDate)
        {
            this.ApplicationName = _ApplicationName;
            this.LevelName = _LevelName;
            this.LogDate = _LogDate;
            GenerateKeys();
        }

        public AppLog(log4net.Core.LoggingEvent le,String _ClientName,String _ApplicationName,String _InstanceName)
        { 
            this.LogDate = DateTime.UtcNow;
            this.ClientName = _ClientName;
            this.ApplicationName = _ApplicationName;
            this.InstanceName = _InstanceName;

            this.LevelName = le.Level.DisplayName;
            this.Message = le.RenderedMessage;
            StringBuilder expMessage = new StringBuilder(); 
            Exception exp = le.ExceptionObject;
            while (exp != null)
            {
                expMessage.AppendLine(String.Format("*************************************Start Exception {0}***************", exp.GetType().FullName));
                expMessage.AppendLine(exp.Message);
                expMessage.AppendLine("*************************************Stack Trace**************************");
                expMessage.AppendLine(exp.StackTrace);
                expMessage.AppendLine(String.Format("*************************************End Exception {0}***************", exp.GetType().FullName));
                exp = exp.InnerException;
            }

            this.Exception = expMessage.ToString();
            var context = log4net.ThreadContext.Properties;
            if (context!=null)
            {
                if (!String.IsNullOrEmpty(context["ApplicationName"] as String))
                {
                    this.ApplicationName = context["ApplicationName"] as string;
                }

                if (!String.IsNullOrEmpty(context["Extension"] as String))
                {
                    this.Extension = context["Extension"] as string;
                }
                if (!String.IsNullOrEmpty(context["InstanceName"] as String))
                {
                    this.InstanceName = context["InstanceName"] as String;
                }
            }
             

            GenerateKeys();
        }               

        public String InstanceName { get; set; }

        private String _ClientName;
        public String ClientName
        {
            get
            { return _ClientName; }
            set
            {
                _ClientName = value;
            }
               
        }

        public String Extension { get; set; }

         
        public String ApplicationName
        {
            get;
            set;
        }

        public DateTime LogDate {get;set;}

        public String LevelName { get; set; }

        public String Message { get; set; }

        public String Exception { get; set; }         

        //public override IDictionary<string, EntityProperty> WriteEntity(Microsoft.WindowsAzure.Storage.OperationContext operationContext)
        //{
        //    var properties = base.WriteEntity(operationContext);
        //    if (this.Extension != null)
        //    {
        //        foreach (var pair in this.Extension)
        //        {
        //            if (pair.Value != null)
        //            {
        //                if (pair.Value is String)
        //                {
        //                    properties.AddOrUpdate(pair.Key, new EntityProperty((String)pair.Value));
        //                }
        //                else if (pair.Value is int)
        //                {
        //                    properties.AddOrUpdate(pair.Key, new EntityProperty((int)pair.Value));
        //                }
        //                else if (pair.Value is long)
        //                {
        //                    properties.AddOrUpdate(pair.Key, new EntityProperty((long)pair.Value));
        //                }
        //                else if (pair.Value is double)
        //                {
        //                    properties.AddOrUpdate(pair.Key, new EntityProperty((double)pair.Value));
        //                }
        //                else if (pair.Value is DateTime)
        //                {
        //                    properties.AddOrUpdate(pair.Key, new EntityProperty((DateTime)pair.Value));
        //                }
        //            }
        //        }
        //    }
        //    return properties;
        //}       

        public void GenerateKeys()
        {
            //ext = String.Format("{0}-{1}", LevelName, Extension).TrimEnd(Dash);
            this.PartitionKey = ReplaceInvalidCharacters(String.Format("{0}-{1}", ApplicationName, LogDate.ToString("yyyy-MM-dd")));
            //if (!String.IsNullOrEmpty(this.InstanceName))
            //{
            //    this.PartitionKey = String.Format("{0}-{1}", this.PartitionKey, this.InstanceName);
            //}

            this.RowKey = ReplaceInvalidCharacters(String.Format("{0}-{1}-{2}", this.LevelName, LogDate.ToString("HH:mm"), GetUniqueKey()));
            
        }

        public void GeneratePK(out String StartKey,out String EndKey)
        {
            StartKey = EndKey = String.Format("{0}-", ApplicationName);
            if (LogDate.Year>2000)
            {
                StartKey = EndKey = String.Format("{0}-{1}", ApplicationName, LogDate.ToString("yyyy-MM-dd"));
                if (!String.IsNullOrEmpty(this.InstanceName))
                {
                    StartKey = EndKey = String.Format("{0}-{1}", StartKey, InstanceName);
                }
                else
                {
                    EndKey = String.Format("{0}-zz", EndKey);
                }
            }
            else
            {
                EndKey = String.Format("{0}{1}", EndKey, "9999-99-99");
            }

            StartKey = ReplaceInvalidCharacters(StartKey);
            EndKey = ReplaceInvalidCharacters(EndKey);

        }

        public void GenerateRK(out String StartKey, out String EndKey, String UniqueKey=null)
        {
            var ext = String.Format("{0}-{1}", LevelName, Extension).TrimEnd(Dash);
            if (LogDate.Year > 2000 && LogDate > LogDate.Date)
            {
                StartKey = String.Format("{0}-{1}", ext, LogDate.Date.ToString("HH:mm"));
                EndKey = String.Format("{0}-{1}",ext, LogDate.ToString("HH:mm"));
                if (!String.IsNullOrEmpty(UniqueKey))
                {
                    StartKey = ReplaceInvalidCharacters(String.Format("{0}-{1}", StartKey, UniqueKey));
                    EndKey = ReplaceInvalidCharacters(String.Format("{0}-{1}", EndKey, UniqueKey));
                    return;
                }
            }

            StartKey = String.Format("{0}-", ext);
            EndKey = String.Format("{0}-zz", ext);
        }

        public String ReplaceInvalidCharacters(String key)
        {
            return ReplaceInvalidCharacters(key, Dash, null);
        }

        public String ReplaceInvalidCharacters(String key, char[] replaceWith, char[] optional)
        {
            if (String.IsNullOrEmpty(key)) return key;
            String toReplace = InvalidKeyCharacters;
            if (optional != null && optional.Length > 0)
                toReplace = toReplace + new String(optional);

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(toReplace);

            return regex.Replace(key, new string(replaceWith));
        }
         

        public static char[]  Dash = new char[]{'-'};
        public const String InvalidKeyCharacters = @"[\\?/#]";

        private const int MinKeySize = 8;

        public string GetUniqueKey()
        {


            char[] chars = new char[62];
            string a;
            a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            chars = a.ToCharArray();
            int size = MinKeySize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = MinKeySize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }

        public override string ToString()
        {
            return String.Format("{0}|{1}", this.PartitionKey, this.RowKey);
        }

        public override bool Equals(object obj)
        {
            var al = obj as AppLog;
            return al!=null 
                    && al.PartitionKey == this.PartitionKey 
                    && al.RowKey == this.RowKey ;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    public static class DictionaryExtension
    {
        public static IDictionary<K, V> AddOrUpdate<K, V>(this IDictionary<K, V> dic, K key, V value)
        {
            if (dic.ContainsKey(key))
                dic[key] = value;
            else
                dic.Add(key, value);
            return dic;
        }
    }
 
}
