using FluentFTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CToolkit.v1_2Core.Net
{
    public class CtkFtp
    {

        public static void test()
        {
            // connect to the FTP server
            var client = new FtpClient();
            client.Host = "123.123.123.123";
            client.Credentials = new NetworkCredential("david", "pass123");
            client.Connect();

            // upload a file
            client.UploadFile(@"C:\MyVideo.mp4", "/htdocs/big.txt");

            // rename the uploaded file
            client.Rename("/htdocs/big.txt", "/htdocs/big2.txt");

            // download the file again
            client.DownloadFile(@"C:\MyVideo_2.mp4", "/htdocs/big2.txt");
        }

        public static void Upload(String host, String acc, String pwd, String src, String dest)
        {
            // connect to the FTP server
            using (var client = new FtpClient())
            {
                client.Host = host;
                client.Credentials = new NetworkCredential(acc, pwd);
                client.Connect();
                // upload a file
                client.UploadFile(src, dest);
            }
        }



    }
}
