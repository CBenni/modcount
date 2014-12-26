using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace ModCounterV3
{
    public partial class MainWindow : Form
    {
        public MainWindow(String user, String oauth)
        {
            InitializeComponent();
            killing = false;
            followedcount = new Dictionary<String, long>();
            viewedcount = new Dictionary<String, long>();
            currentfollows = new List<string>();
            bots = new List<IRCBot>();
            allbots = new List<IRCBot>();
            mods = new Dictionary<String, String[]>();
            loglines = new Queue<string>();
            readqueue = new Queue<string>();
            currentuser = "";
            username = user;
            useroauth = oauth;
            textBox1.Text = user;
        }
        public String username;
        public String useroauth;
        public List<IRCBot> bots;
        public List<IRCBot> allbots;
        bool sentFinishMsg;
        Queue<String> readqueue; // need to get .mods from these
        List<String> currentfollows; // current user following list
        String currentuser; // current user
        Dictionary<String, long> followedcount;
        Dictionary<String, long> viewedcount;
        Dictionary<String, String[]> mods;
        Queue<String> loglines;
        private void Form1_Load(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(makebots));
            t.Start();
        }

        internal void onMods(String channel, String p)
        {
            mods[channel] = p.Split(new char[] { ' ', ',' },StringSplitOptions.RemoveEmptyEntries);
        }

        public void updateList()
        {
            List<ChannelData> data = new List<ChannelData>();
            long totalfollowers = 0;
            long totalviews = 0;
            bool nallloaded = false;
            List<String> chs = currentfollows;
            if (currentuser == "")
            {
                chs = followedcount.Keys.ToList();
            }
            foreach (String x in chs)
            {
                ChannelData d = new ChannelData();
                d.name = x;
                d.followers = "";
                d.views = "";
                d.ismod = "";
                d.loaded = "";
                if (followedcount.ContainsKey(x))
                {
                    totalfollowers += followedcount[x];
                    totalviews += viewedcount[x];
                    d.followers = followedcount[x].ToString();
                    d.views = viewedcount[x].ToString();
                }
                else nallloaded = true;
                data.Add(d);
            }
            ChannelData totals = new ChannelData();
            totals.name = currentfollows.Count.ToString();
            totals.followers = totalfollowers.ToString();
            totals.views = totalviews.ToString();
            totals.ismod = "";// totalmods.ToString();
            totals.loaded = nallloaded?"":"X";
            ChannelData modtotals = new ChannelData();
            modtotals.name = "";// currentfollows.Count.ToString();
            modtotals.followers = "";
            modtotals.views = "";
            modtotals.ismod = "";
            totals.loaded = "";
            data.Add(totals);
            data.Add(modtotals);
            dataGridView1.DataSource = data;
            dataGridView1.Rows[data.Count - 2].HeaderCell.Value = "Total";
            dataGridView1.Rows[data.Count - 1].HeaderCell.Value = "Mod Total";
        }

        internal void botDone(IRCBot b)
        {
            bots.Add(b);
            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Update();
            currentfollows = new List<string>();
            currentuser = textBox1.Text.Trim().ToLowerInvariant();
            sentFinishMsg = false;
            if (currentuser == "")
            {
                currentfollows = followedcount.Keys.OrderBy(x => (-followedcount[x])).ToList();
                updateList();
                return;
            }
            using (WebClient cl = new WebClient())
            {
                cl.Proxy = null;
                String nextUrl = "https://api.twitch.tv/kraken/users/" + currentuser + "/follows/channels?direction=DESC&limit=250&offset=0&sortby=created_at";
                int cnt = 0; int cnt2 = 0;
                while (true)
                {
                    int i = 0;
                    string data = null;
                    while (i++ < 10)
                    {
                        try
                        {
                            data = cl.DownloadString(nextUrl);
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(100);
                        }
                    }
                    Thread.Sleep(100);
                    if (data == null)
                    {
                        MessageBox.Show("Error accessing twitch API");
                    }
                    FollowClasses.RootObject obj = JsonConvert.DeserializeObject<FollowClasses.RootObject>(data);
                    if (obj.follows.Count == 0) break;
                    cnt2 += obj.follows.Count;
                    foreach (FollowClasses.Follow fl in obj.follows)
                    {
                        if(!mods.ContainsKey(fl.channel.name))
                        {
                            readqueue.Enqueue(fl.channel.name);
                        }
                        currentfollows.Add(fl.channel.name);
                        followedcount[fl.channel.name] = fl.channel.followers;
                        viewedcount[fl.channel.name] = fl.channel.views;
                    }
                    nextUrl = obj._links.next;
                    cnt = obj._total;
                }
                if (cnt != cnt2) MessageBox.Show("Error: Followcount and followed channels dont agree!");
                currentfollows = currentfollows.OrderBy(x => (-followedcount[x])).ToList();
                readqueue = new Queue<String>(readqueue.OrderBy(x => (-followedcount[x])));
                updateList();
            }
        }
        public void log(string p, bool output = false)
        {
            try
            {
                if (output) loglines.Enqueue(p);
                Console.WriteLine(p);
                File.AppendAllLines("modcounter.log", new String[] { p });
            }
            catch (Exception e)
            {
            }
        }

        const int MAXBOTS = 30;
        public void makebots()
        {
            while (!killing)
            {
                if (allbots.Count < MAXBOTS)
                {
                    IRCBot bot = new IRCBot(this);
                    Console.WriteLine("Bot with id " + allbots.Count + " created.");
                    allbots.Add(bot);
                }
                Thread.Sleep(2000);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (killing) return;
            String l="";
            while(loglines.Count>0)
            {
                l = loglines.Dequeue();
                richTextBox1.Text += l + "\r\n";
            }
            if (currentfollows != null && dataGridView1.Rows.Count>0)
            {
                bool nall = false;
                long totalmodfollowers = 0;
                long totalmodviews = 0;
                long totalmods = 0;
                for (int i = 0; i < dataGridView1.Rows.Count - 2; i++)
                {
                    DataGridViewRow row = dataGridView1.Rows[i];
                    String rowch = (String)row.Cells[0].Value;
                    if (mods.ContainsKey(rowch))
                    {
                        row.Cells[4].Value = "X";
                        if (mods[rowch].Contains(currentuser))
                        {
                            row.Cells[3].Value = "X";
                            totalmodfollowers += followedcount[rowch];
                            totalmodviews += viewedcount[rowch];
                            totalmods++;
                        }
                    }
                    else
                    {
                        nall = true;
                    }
                }
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = totalmodfollowers;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[2].Value = totalmodviews;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[3].Value = totalmods;
                if (!nall && !sentFinishMsg)
                {
                    sentFinishMsg = true;
                    dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[4].Value = "X";
                    log(currentuser + ": mod in " + totalmods + " channels, total followers: " + totalmodfollowers, true);
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int nbot = 0;
            List<String> sent = new List<string>();
            while (!killing)
            {
                if(readqueue.Count>0)
                {
                    String channel = readqueue.Dequeue();
                    if (!sent.Contains(channel))
                    {
                        bots[nbot].sendData("PRIVMSG", "#" + channel + " :.mods");
                        sent.Add(channel);
                    }
                    if (killing) return;
                    Thread.Sleep(2000 / bots.Count);
                    nbot = (nbot + 1) % bots.Count;
                } 
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
        public bool killing;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            killing = true;
            while(allbots.Count>0)
            {
                allbots[0].leaving();
            }
        }
    }
    public class ChannelData
    {
        public String name { get; set; }
        public String followers { get; set; }
        public String views { get; set; }
        public String ismod { get; set; }
        public String loaded { get; set; }
    }
}
