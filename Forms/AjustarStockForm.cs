using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data.SQLite;
using SistemaPOS.Data;
using SistemaPOS.Models;
using SistemaPOS.Helpers;

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
            this.Size = new Size(550, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            UITheme.ApplyTheme(this);

            // HEADER BAR
            var header = UITheme.CrearHeaderBar("Ajuste de Inventario", nombreProducto);
            this.Controls.Add(header);

            // Panel contenedor
            RoundedPanel panel = new RoundedPanel
            {
                Location = new Point(30, 100),
                Size = new Size(470, 340),
                BackColor = UITheme.PanelBackground,
                Radius = 12
            };

            // Stock actual (grande y destacado)
            Label lblStockActual = new Label
            {
                Text = "STOCK ACTUAL",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 20),
                AutoSize = true
            };
            panel.Controls.Add(lblStockActual);

            Label lblStockValor = new Label
            {
                Text = stockActual.ToString() + " unidades",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = UITheme.AccentColor,
                Location = new Point(20, 42),
                AutoSize = true
            };
            panel.Controls.Add(lblStockValor);

            // Separador
            Panel separador = new Panel
            {
                Location = new Point(20, 90),
                Size = new Size(430, 1),
                BackColor = UITheme.WithAlpha(UITheme.TextSecondary, 100)
            };
            panel.Controls.Add(separador);

            // Tipo de movimiento
            Label lblTipo = new Label
            {
                Text = "TIPO DE MOVIMIENTO",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 110),
                AutoSize = true
            };
            panel.Controls.Add(lblTipo);

            cboTipo = new ComboBox
            {
                Location = new Point(20, 132),
                Size = new Size(260, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.FontRegular
            };
            UITheme.StyleComboBox(cboTipo);
            cboTipo.Items.AddRange(new string[] { "Entrada (Compra)", "Salida (Merma/Uso)", "Ajuste (Corrección)" });
            cboTipo.SelectedIndex = 0;
            panel.Controls.Add(cboTipo);

            // Cantidad
            Label lblCant = new Label
            {
                Text = "CANTIDAD",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(300, 110),
                AutoSize = true
            };
            panel.Controls.Add(lblCant);

            txtCantidad = new TextBox
            {
                Location = new Point(300, 132),
                Size = new Size(150, 28),
                Font = new Font("Segoe UI", 11)
            };
            UITheme.StyleTextBox(txtCantidad);
            txtCantidad.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            panel.Controls.Add(txtCantidad);

            // Motivo
            Label lblMotivo = new Label
            {
                Text = "MOTIVO / NOTAS",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 180),
                AutoSize = true
            };
            panel.Controls.Add(lblMotivo);

            txtMotivo = new TextBox
            {
                Location = new Point(20, 202),
                Size = new Size(430, 60),
                Font = UITheme.FontRegular,
                Multiline = true
            };
            UITheme.StyleTextBox(txtMotivo);
            panel.Controls.Add(txtMotivo);

            // Info text
            Label lblInfo = new Label
            {
                Text = "• Entrada: suma al stock | • Salida: resta del stock | • Ajuste: reemplaza el stock",
                Font = new Font("Segoe UI", 8),
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 275),
                Size = new Size(430, 40)
            };
            panel.Controls.Add(lblInfo);

            this.Controls.Add(panel);

            // Botones
            RoundedButton btnGuardar = new RoundedButton
            {
                Text = "💾 GUARDAR",
                Location = new Point(260, 460),
                Size = new Size(140, 40),
                BackColor = UITheme.SuccessColor
            };
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            RoundedButton btnCancelar = new RoundedButton
            {
                Text = "CANCELAR",
                Location = new Point(410, 460),
                Size = new Size(120, 40),
                BackColor = UITheme.ErrorColor
            };
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancelar);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            int cantidad;
            if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tipo = cboTipo.SelectedItem.ToString().Split(' ')[0]; // Entrada, Salida, Ajuste
            int nuevoStock = stockActual;

            if (tipo == "Entrada") nuevoStock += cantidad;
            else if (tipo == "Salida") nuevoStock -= cantidad;
            else if (tipo == "Ajuste") nuevoStock = cantidad; // Ajuste reemplaza el stock

            if (nuevoStock < 0)
            {
                MessageBox.Show("El stock no puede ser negativo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                    catch(Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
