using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ModCounterV3
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            InitWindow iw = new InitWindow();
            iw.ShowDialog();
            Application.Run(new MainWindow(iw.user,iw.oauth));
        }
    }
}
