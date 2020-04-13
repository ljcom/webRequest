using System;
using System.Data.SqlTypes;

// For the SQL Server integration
using Microsoft.SqlServer.Server;

// Other things we need for WebRequest
using System.Net;
using System.Text;
using System.IO;

namespace webRequest
{
    public partial class Functions
    {
        // Function to return a web URL as a string value.
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString GET(SqlString uri, SqlString username, SqlString passwd, SqlString proxyHost, SqlInt16 proxyPort, SqlString userAgent)
        {
            // The SqlPipe is how we send data back to the caller
            SqlPipe pipe = SqlContext.Pipe;
            SqlString document;

            // Set up the request, including authentication
            WebRequest req = WebRequest.Create(Convert.ToString(uri));
            
            if (Convert.ToString(proxyHost) != null & proxyPort.IsNull & Convert.ToInt16(proxyPort.ToString()) > 0)
            {
                req.Proxy = new WebProxy(Convert.ToString(proxyHost), Convert.ToInt16(proxyPort.ToString()));
            }

            if (Convert.ToString(username) != null & Convert.ToString(username) != "")
            {
                req.Credentials = new NetworkCredential(
                    Convert.ToString(username),
                    Convert.ToString(passwd));
            }
            else req.UseDefaultCredentials = true;

            if (userAgent == null) userAgent = "CLR web client on SQL Server";
            ((HttpWebRequest)req).UserAgent = userAgent.ToString();

            // Fire off the request and retrieve the response.
            // We'll put the response in the string variable "document".
            WebResponse resp = req.GetResponse();
            Stream dataStream = resp.GetResponseStream();
            StreamReader rdr = new StreamReader(dataStream);
            document = (SqlString)rdr.ReadToEnd();

            // Close up everything...
            rdr.Close();
            dataStream.Close();
            resp.Close();

            // .. and return the output to the caller.
            return (document);
        }

        // Function to submit a HTTP POST and return the resulting output.
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString POST(SqlString uri, SqlString postData, SqlString username, SqlString passwd, SqlString headers, SqlString proxyHost, SqlInt16 proxyPort, SqlString userAgent)
        {
            SqlString document = "";

            try
            {
                SqlPipe pipe = SqlContext.Pipe;

                byte[] postByteArray = Encoding.UTF8.GetBytes(Convert.ToString(postData));

                var urix = new Uri(Convert.ToString(uri));
                var p = ServicePointManager.FindServicePoint(urix);
                p.Expect100Continue = false;


                //// Create a WebPermission.
                //WebPermission myWebPermission1 = new WebPermission();

                //// Allow Connect access to the specified URLs.
                //myWebPermission1.AddPermission(NetworkAccess.Connect, new Regex("http://www\.twitter\.com/.*",
                //  RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline));

                //myWebPermission1.Demand();

                // Set up the request, including authentication, 
                // method=POST and encoding:

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Convert.ToString(uri));
                req.ServicePoint.Expect100Continue = false;
                
                if (Convert.ToString(proxyHost) != null & proxyPort.IsNull & Convert.ToInt16(proxyPort.ToString()) > 0)
                {
                    req.Proxy = new WebProxy(Convert.ToString(proxyHost), Convert.ToInt16(proxyPort.ToString()));
                }

                if (headers != "")
                {
                    foreach (string x in headers.ToString().Split('&'))
                    {
                        var x1 = x.Split('=');
                        req.Headers.Add(x1[0], x1[1]);
                    }
                }

                if (Convert.ToString(username) != null & Convert.ToString(username) != "")
                {
                    req.Credentials = new NetworkCredential(
                        Convert.ToString(username),
                        Convert.ToString(passwd));
                }
                if (Convert.ToString(username) != null & Convert.ToString(username) != "")
                {
                    req.Credentials = new NetworkCredential(
                        Convert.ToString(username),
                        Convert.ToString(passwd));
                }
                else req.UseDefaultCredentials = true;

                if (userAgent == null) userAgent = "CLR web client on SQL Server";
                ((HttpWebRequest)req).UserAgent = userAgent.ToString();

                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                // Submit the POST data
                Stream dataStream = req.GetRequestStream();
                dataStream.Write(postByteArray, 0, postByteArray.Length);
                dataStream.Close();

                // Collect the response, put it in the string variable "document"
                WebResponse resp = req.GetResponse();
                dataStream = resp.GetResponseStream();
                StreamReader rdr = new StreamReader(dataStream);
                document = (SqlString)rdr.ReadToEnd();

                // Close up and return
                rdr.Close();
                dataStream.Close();
                resp.Close();

            }
            catch (NullReferenceException exc)
            {
                // send error back
                SqlContext.Pipe.Send(exc.Message);
                //document = exc.Message;
            }

            catch (Exception exc)
            {
                // send error back
                SqlContext.Pipe.Send(exc.Message);
                //document = exc.Message;
            }
            return (document);
        }

        public static SqlBoolean Download(SqlString uri, SqlString localPath,SqlString username, SqlString passwd, SqlString proxyHost, SqlInt16 proxyPort)
        {
            
            using (var client = new WebClient())
            {
                if (Convert.ToString(username) != null & Convert.ToString(username) != "")
                {
                    client.Credentials = new NetworkCredential(
                        Convert.ToString(username),
                        Convert.ToString(passwd));
                }
                if (Convert.ToString(username) != null & Convert.ToString(username) != "")
                {
                    client.Credentials = new NetworkCredential(
                        Convert.ToString(username),
                        Convert.ToString(passwd));
                }
                else client.UseDefaultCredentials = true;

                client.DownloadFile(uri.ToString(), localPath.ToString());
            }
            return (File.Exists(localPath.ToString()));
        }

      
    }
}
