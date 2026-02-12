using System;
using System.Windows.Forms;
using System.Drawing;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public class CantidadForm : Form
    {
        public int Cantidad { get; private set; }
        private TextBox txtCantidad;

        public CantidadForm(string nombreProducto, int stockMaximo)
        {
            this.Text = "Ingresar Cantidad";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            UITheme.ApplyTheme(this);

            Label lblProd = new Label { Text = string.Format("Producto: {0}", nombreProducto), Location = new Point(30, 30), AutoSize = true, Font = new Font("Segoe UI", 12) };
            UITheme.StyleLabel(lblProd, UITheme.FontRegular);
            
            Label lblCant = new Label { Text = "Cantidad:", Location = new Point(30, 80), AutoSize = true, Font = new Font("Segoe UI", 12) };
            UITheme.StyleLabel(lblCant, UITheme.FontRegular);

            txtCantidad = new TextBox { Location = new Point(130, 77), Width = 200, Text = "1", Font = new Font("Segoe UI", 14) };
            UITheme.StyleTextBox(txtCantidad);
            txtCantidad.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            txtCantidad.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Confirmar(); };
            
            Button btnOk = new Button { Text = "Aceptar", Location = new Point(100, 160), Size = new Size(130, 45), Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            UITheme.StyleButton(btnOk, UITheme.PrimaryColor);
            btnOk.Click += (s, e) => Confirmar();

            Button btnCancel = new Button { Text = "Cancelar", Location = new Point(250, 160), Size = new Size(130, 45), Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            UITheme.StyleButton(btnCancel, UITheme.ErrorColor);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblProd);
            this.Controls.Add(lblCant);
            this.Controls.Add(txtCantidad);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
        }

        private void Confirmar()
        {
            int cant;
            if (int.TryParse(txtCantidad.Text, out cant) && cant > 0)
            {
                Cantidad = cant;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Por favor ingrese una cantidad válida mayor a 0.", "Cantidad Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.SelectAll();
                txtCantidad.Focus();
            }
        }
        
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtCantidad.Focus();
            txtCantidad.SelectAll();
        }
    }
}