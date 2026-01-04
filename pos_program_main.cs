using System;
using System.Windows.Forms;
using SistemaPOS.Data;
using SistemaPOS.Forms;

namespace SistemaPOS
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                // Inicializar la base de datos
                DatabaseHelper.InitializeDatabase();
                
                // Mostrar formulario de login
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error crítico al iniciar la aplicación:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Error Fatal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}