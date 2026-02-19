using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaPOS.Data;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class ConfiguracionForm : Form
    {
        private TextBox txtNombreEmpresa;
        private TextBox txtRUT;
        private TextBox txtDireccion;
        private TextBox txtTelefono;
        private TextBox txtGiro;

        public ConfiguracionForm()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuración de Empresa";
            this.Size = new Size(600, 570);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            UITheme.ApplyTheme(this);

            // HEADER BAR
            var header = UITheme.CrearHeaderBar("Configuración de Empresa", "Datos que aparecerán en tickets y reportes");
            this.Controls.Add(header);

            // Panel contenedor
            RoundedPanel panel = new RoundedPanel
            {
                Location = new Point(30, 100),
                Size = new Size(520, 380),
                BackColor = UITheme.PanelBackground,
                Radius = 12
            };

            // Campos
            txtNombreEmpresa = CrearCampo(panel, "Razón Social / Nombre:", 20);
            txtRUT = CrearCampo(panel, "RUT / Identificación:", 90);
            txtGiro = CrearCampo(panel, "Giro / Actividad:", 160);
            txtDireccion = CrearCampo(panel, "Dirección:", 230);
            txtTelefono = CrearCampo(panel, "Teléfono:", 300);

            this.Controls.Add(panel);

            // Botones
            RoundedButton btnGuardar = new RoundedButton
            {
                Text = "💾 GUARDAR CAMBIOS",
                Location = new Point(280, 500),
                Size = new Size(180, 40),
                BackColor = UITheme.SuccessColor
            };
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            RoundedButton btnCancelar = new RoundedButton
            {
                Text = "CANCELAR",
                Location = new Point(470, 500),
                Size = new Size(120, 40),
                BackColor = UITheme.ErrorColor
            };
            btnCancelar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancelar);
        }

        private TextBox CrearCampo(Panel contenedor, string etiqueta, int y)
        {
            Label lbl = new Label
            {
                Text = etiqueta,
                Location = new Point(20, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary
            };
            contenedor.Controls.Add(lbl);

            TextBox txt = new TextBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(480, 30),
                Font = new Font("Segoe UI", 11)
            };
            UITheme.StyleTextBox(txt);
            contenedor.Controls.Add(txt);

            return txt;
        }

        private void CargarDatos()
        {
            txtNombreEmpresa.Text = DatabaseHelper.GetConfiguracion("NombreEmpresa");
            txtRUT.Text = DatabaseHelper.GetConfiguracion("RUT");
            txtGiro.Text = DatabaseHelper.GetConfiguracion("Giro");
            txtDireccion.Text = DatabaseHelper.GetConfiguracion("Direccion");
            txtTelefono.Text = DatabaseHelper.GetConfiguracion("Telefono");
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseHelper.SetConfiguracion("NombreEmpresa", txtNombreEmpresa.Text.Trim());
                DatabaseHelper.SetConfiguracion("RUT", txtRUT.Text.Trim());
                DatabaseHelper.SetConfiguracion("Giro", txtGiro.Text.Trim());
                DatabaseHelper.SetConfiguracion("Direccion", txtDireccion.Text.Trim());
                DatabaseHelper.SetConfiguracion("Telefono", txtTelefono.Text.Trim());

                MessageBox.Show("Datos guardados exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
