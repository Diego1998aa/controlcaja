using System;
using System.Globalization;
using System.Threading;
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
            try 
            {
                // Configurar cultura para Pesos Chilenos (es-CL)
                CultureInfo culture = new CultureInfo("es-CL");
                culture.NumberFormat.CurrencySymbol = "$";
                culture.NumberFormat.CurrencyDecimalDigits = 0; // Sin decimales para CLP
                culture.NumberFormat.NumberDecimalDigits = 0;
                
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                DatabaseHelper.InitializeDatabase();
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Error crítico al iniciar la aplicación:\n\n{0}\n\n{1}", ex.Message, ex.StackTrace),
                    "Error Fatal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}