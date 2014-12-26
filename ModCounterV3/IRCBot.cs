using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ModCounterV3
{
    public class IRCBot
    {
        TcpClient IRCConnection = null;
        NetworkStream ns = null;
        StreamReader sr = null;
        StreamWriter sw = null;
        Thread t;
        public MainWindow fm;
        List<string> ops;
        Random rng;
        public int cmdcnt;
        public bool started;
        public bool waiting;
        public String ch;

        public IRCBot(MainWindow f)
        {
            ch = "#kbenni";
            ops = new List<string>();
            rng = new Random();
            fm = f;
            cmdcnt = 0;
            started = false;
            waiting = false;
            if (!reconnect()) throw new IOException("Could not (re)connect to IRC server");
            t = new Thread(new ThreadStart(start));
            t.Start();
        }

        public void start()
        {
            while (!fm.killing)
            {
                IRCWork();
            }
        }

        bool reconnect()
        {
            try
            {
                IRCConnection = new TcpClient("199.9.250.239", 443);
                ns = IRCConnection.GetStream();
                sr = new StreamReader(ns);
                sw = new StreamWriter(ns);
                Thread.Sleep(100);
                sendData("PASS", "oauth:"+fm.useroauth);
                sendData("USER", fm.username + " " + fm.username + "  " + fm.username + " :" + fm.username);
                sendData("NICK", fm.username);
                sendData("TWITCHCLIENT", "3");
            }
            catch (Exception e)
            {
                fm.log("Communication error: " + e.Message);
                return false;
            }
            return true;
        }

        public void leaving()
        {
            try
            {
                sendData("QUIT", " :ByeBye");
                fm.bots.Remove(this);
                fm.allbots.Remove(this);
                sr.Close();
            }
            catch (Exception e)
            {
            }
        }

        ~IRCBot()
        {
            if (sr != null)
                sr.Close();
            if (sw != null)
                sw.Close();
            if (ns != null)
                ns.Close();
            if (IRCConnection != null)
                IRCConnection.Close();
        }

        public void IRCWork()
        {
            try
            {
                string[] ex;
                string data = sr.ReadLine();
                if (data == "" || data == null) return;
                //Console.WriteLine(data);
                char[] charSeparator = new char[] { ' ' };
                if (data == null) return;
                ex = data.Split(charSeparator, 5);

                if (ex[0] == "PING")
                {
                    sendData("PONG", ex[1]);
                }
                if (ex[1] == "003")
                {
                    fm.botDone(this);
                    started = true;
                }
                if (ex[1] == "PRIVMSG" && ex[0].StartsWith(":jtv!"))
                {
                    String modmsg = ":The moderators of this room are: ";
                    String msg = "";
                    for (int i = 3; i < ex.Count(); ++i)
                    {
                        if (msg != "") msg += " ";
                        msg += ex[i];
                    }
                    if (msg.StartsWith(modmsg))
                    {
                        waiting = false;
                        fm.onMods(ex[2].Replace("#", ""), msg.Substring(modmsg.Length));
                    }
                    else if (ex[3].StartsWith(":HISTORYEND"))
                    {
                    }
                }

            }
            catch (System.IO.IOException ee)
            {
                if (!fm.killing)
                {
                    fm.log(ee.Message + "\r\n" + ee.StackTrace);
                    fm.log("Killing and rebuilding bot with id " + fm.allbots.IndexOf(this));
                    try
                    {
                        leaving();
                    }
                    catch { }
                }
            }
            catch (Exception ee)
            {
                return;
            }
        }

        public void sendData(string cmd, string param)
        {
            try
            {
                if (param == null)
                {
                    sw.WriteLine(cmd);
                    sw.Flush();
                }
                else
                {
                    sw.WriteLine(cmd + " " + param);
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                reconnect();
            }
        }

        public void stop()
        {
            try
            {
                leaving();
                if (sr != null)
                    sr.Close();
                if (sw != null)
                    sw.Close();
                if (ns != null)
                    ns.Close();
                if (IRCConnection != null)
                    IRCConnection.Close();
            }
            catch { };
        }
    }
}
