using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace ModCounterV3
{
    public partial class InitWindow : Form
    {
        public String user;
        public String oauth;
        public InitWindow()
        {
            InitializeComponent();
            /*WebKit.WebKitBrowser browser = new WebKit.WebKitBrowser();
            browser.Dock = DockStyle.Fill;
            browser.Url = new Uri("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=pa7836kplhn3zt715a0lxyrx7zabtfo&redirect_uri=http%3A%2F%2Fcbenni.com&scope=chat_login");
            browser.Navigating += new WebBrowserNavigatingEventHandler(browser_Navigating);
            this.Controls.Add(browser);
        }

        void browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            
            //e.Cancel = true;*/
        }



        private void button1_Click(object sender, EventArgs e)
        {
            using (WebClient cl = new WebClient())
            {
                var values = new NameValueCollection();
                values["user[login]"] = textBox1.Text;
                values["user[password]"] = textBox1.Text;

                var res = cl.UploadValues(new Uri("https://api.twitch.tv/kraken/oauth2/login"),values);
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            String tourl = e.Url.ToString();
            Console.WriteLine(tourl);
            if (!tourl.StartsWith("http://cbenni.com")) return;
            int eqp = tourl.IndexOf("=");
            int ampp = tourl.IndexOf("&");
            string token = tourl.Substring(eqp + 1, ampp - eqp - 1);
            using (WebClient cl = new WebClient())
            {
                String res = cl.DownloadString("https://api.twitch.tv/kraken?oauth_token=" + token);
                AuthClasses.RootObject obj = JsonConvert.DeserializeObject<AuthClasses.RootObject>(res);
                user = obj.token.user_name;
                oauth = token;
                Close();
            }
        }
    }
}
