using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data.SQLite;
using SistemaPOS.Data;
using SistemaPOS.Models;

namespace SistemaPOS.Forms
{
    public partial class AjustarStockForm : Form
    {
        private int idProducto;
        private int stockActual;
        private string nombreProducto;
        private TextBox txtCantidad;
        private ComboBox cboTipo;
        private TextBox txtMotivo;

        public AjustarStockForm(int idProducto, string nombreProducto, int stockActual)
        {
            this.idProducto = idProducto;
            this.nombreProducto = nombreProducto;
            this.stockActual = stockActual;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Ajustar Stock";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(37, 37, 38);

            Panel panel = new Panel { Location = new Point(20, 20), Size = new Size(340, 270), BackColor = Color.FromArgb(45, 45, 48) };

            Label lblTitulo = new Label { Text = "AJUSTE DE INVENTARIO", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, Location = new Point(10, 10), AutoSize = true };
            
            Label lblProd = new Label { Text = string.Format("Producto: {0}", nombreProducto), Font = new Font("Segoe UI", 10), ForeColor = Color.LightGray, Location = new Point(10, 40), AutoSize = true };
            Label lblStock = new Label { Text = string.Format("Stock Actual: {0}", stockActual), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, Location = new Point(10, 65), AutoSize = true };

            Label lblTipo = new Label { Text = "Tipo de Movimiento:", ForeColor = Color.White, Location = new Point(10, 100), AutoSize = true };
            cboTipo = new ComboBox { Location = new Point(10, 120), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboTipo.Items.AddRange(new string[] { "Entrada (Compra)", "Salida (Merma/Uso)", "Ajuste (Corrección)" });
            cboTipo.SelectedIndex = 0;

            Label lblCant = new Label { Text = "Cantidad:", ForeColor = Color.White, Location = new Point(180, 100), AutoSize = true };
            txtCantidad = new TextBox { Location = new Point(180, 120), Size = new Size(140, 25) };
            txtCantidad.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };

            Label lblMotivo = new Label { Text = "Motivo / Notas:", ForeColor = Color.White, Location = new Point(10, 160), AutoSize = true };
            txtMotivo = new TextBox { Location = new Point(10, 180), Size = new Size(310, 25) };

            Button btnGuardar = new Button { Text = "GUARDAR", BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(10, 220), Size = new Size(150, 40) };
            btnGuardar.Click += BtnGuardar_Click;

            Button btnCancelar = new Button { Text = "CANCELAR", BackColor = Color.Firebrick, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(170, 220), Size = new Size(150, 40) };
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            panel.Controls.AddRange(new Control[] { lblTitulo, lblProd, lblStock, lblTipo, cboTipo, lblCant, txtCantidad, lblMotivo, txtMotivo, btnGuardar, btnCancelar });
            this.Controls.Add(panel);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            int cantidad;
            if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad <= 0) { MessageBox.Show("Ingrese una cantidad válida."); return; }
            
            string tipo = cboTipo.SelectedItem.ToString().Split(' ')[0]; // Entrada, Salida, Ajuste
            int nuevoStock = stockActual;
            
            if (tipo == "Entrada") nuevoStock += cantidad;
            else if (tipo == "Salida") nuevoStock -= cantidad;
            else if (tipo == "Ajuste") nuevoStock = cantidad; // Ajuste reemplaza el stock

            if (nuevoStock < 0) { MessageBox.Show("El stock no puede ser negativo."); return; }

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Actualizar Producto
                        string sqlUpdate = "UPDATE Productos SET StockActual = @stock WHERE IdProducto = @id";
                        using(var cmd = new SQLiteCommand(sqlUpdate, conn, trans)) {
                            cmd.Parameters.AddWithValue("@stock", nuevoStock);
                            cmd.Parameters.AddWithValue("@id", idProducto);
                            cmd.ExecuteNonQuery();
                        }

                        // Registrar Movimiento
                        string sqlMov = "INSERT INTO Movimientos_Inventario (IdProducto, TipoMovimiento, Cantidad, StockAnterior, StockNuevo, Motivo, IdUsuario) VALUES (@id, @tipo, @cant, @ant, @nue, @mot, @user)";
                        using(var cmd = new SQLiteCommand(sqlMov, conn, trans)) {
                            cmd.Parameters.AddWithValue("@id", idProducto);
                            cmd.Parameters.AddWithValue("@tipo", tipo);
                            cmd.Parameters.AddWithValue("@cant", cantidad);
                            cmd.Parameters.AddWithValue("@ant", stockActual);
                            cmd.Parameters.AddWithValue("@nue", nuevoStock);
                            cmd.Parameters.AddWithValue("@mot", txtMotivo.Text);
                            cmd.Parameters.AddWithValue("@user", SesionActual.UsuarioActivo.IdUsuario);
                            cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                        this.DialogResult = DialogResult.OK;
                    }
                    catch(Exception ex) { trans.Rollback(); MessageBox.Show("Error: " + ex.Message); }
                }
            }
        }
    }
}