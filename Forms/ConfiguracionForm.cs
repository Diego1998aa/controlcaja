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
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            UITheme.ApplyTheme(this);

            Label lblTitulo = new Label 
            { 
                Text = "DATOS DE LA EMPRESA", 
                Font = UITheme.FontTitle, 
                ForeColor = UITheme.TextPrimary, 
                Location = new Point(20, 20),
                AutoSize = true 
            };

            // Campos
            txtNombreEmpresa = CrearCampo("Razón Social / Nombre:", 70);
            txtRUT = CrearCampo("RUT / Identificación:", 130);
            txtGiro = CrearCampo("Giro / Actividad:", 190);
            txtDireccion = CrearCampo("Dirección:", 250);
            txtTelefono = CrearCampo("Teléfono:", 310);

            // Botón Guardar
            Button btnGuardar = new Button 
            { 
                Text = "💾 GUARDAR CAMBIOS", 
                Location = new Point(20, 380), 
                Size = new Size(200, 40), 
                Font = UITheme.FontBold 
            };
            UITheme.StyleButton(btnGuardar, UITheme.SuccessColor);
            btnGuardar.Click += BtnGuardar_Click;

            // Botón Cancelar
            Button btnCancelar = new Button 
            { 
                Text = "CANCELAR", 
                Location = new Point(240, 380), 
                Size = new Size(150, 40), 
                Font = UITheme.FontBold 
            };
            UITheme.StyleButton(btnCancelar, UITheme.ErrorColor);
            btnCancelar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitulo, btnGuardar, btnCancelar });
        }

        private TextBox CrearCampo(string etiqueta, int y)
        {
            Label lbl = new Label 
            { 
                Text = etiqueta, 
                Location = new Point(20, y), 
                AutoSize = true, 
                Font = UITheme.FontRegular,
                ForeColor = UITheme.TextSecondary
            };

            TextBox txt = new TextBox 
            { 
                Location = new Point(20, y + 25), 
                Size = new Size(440, 30), 
                Font = UITheme.FontRegular 
            };
            UITheme.StyleTextBox(txt);

            this.Controls.Add(lbl);
            this.Controls.Add(txt);
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
