using System;
using System.Windows.Forms;
using SistemaPOS.Data;
using SistemaPOS.Forms;

namespace SistemaPOS
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DatabaseHelper.InitializeDatabase();
            Application.Run(new LoginForm());
        }
    }
}