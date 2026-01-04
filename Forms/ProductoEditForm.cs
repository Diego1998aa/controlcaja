using System;
using System.Windows.Forms;
using System.Drawing;
using SistemaPOS.Models;
using SistemaPOS.Services;

namespace SistemaPOS.Forms
{
    public partial class ProductoEditForm : Form
    {
        private int? idProducto;
        private TextBox txtCodigo, txtNombre, txtPrecioC, txtPrecioV, txtStock;
        private ComboBox cboCategoria;

        public ProductoEditForm(int? id = null)
        {
            idProducto = id;
            InitializeComponent();
            CargarCategorias();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(37, 37, 38);

            int y = 20;
            this.Controls.Add(CrearLabel("Código:", y)); txtCodigo = CrearTxt(y + 20); this.Controls.Add(txtCodigo); y += 60;
            this.Controls.Add(CrearLabel("Nombre:", y)); txtNombre = CrearTxt(y + 20); this.Controls.Add(txtNombre); y += 60;
            this.Controls.Add(CrearLabel("Categoría:", y)); cboCategoria = new ComboBox { Location = new Point(20, y + 20), Width = 340 }; this.Controls.Add(cboCategoria); y += 60;
            this.Controls.Add(CrearLabel("Precio Compra:", y)); txtPrecioC = CrearTxt(y + 20); this.Controls.Add(txtPrecioC); y += 60;
            this.Controls.Add(CrearLabel("Precio Venta:", y)); txtPrecioV = CrearTxt(y + 20); this.Controls.Add(txtPrecioV); y += 60;
            this.Controls.Add(CrearLabel("Stock:", y)); txtStock = CrearTxt(y + 20); this.Controls.Add(txtStock); y += 60;

            Button btn = new Button { Text = "GUARDAR", Location = new Point(20, y), Size = new Size(340, 40), BackColor = Color.SeaGreen, ForeColor = Color.White };
            btn.Click += BtnGuardar_Click;
            this.Controls.Add(btn);
        }

        private Label CrearLabel(string t, int y) => new Label { Text = t, ForeColor = Color.White, Location = new Point(20, y), AutoSize = true };
        private TextBox CrearTxt(int y) => new TextBox { Location = new Point(20, y), Width = 340 };

        private void CargarCategorias()
        {
            cboCategoria.DataSource = ProductoService.ObtenerCategorias();
            cboCategoria.DisplayMember = "NombreCategoria";
            cboCategoria.ValueMember = "IdCategoria";
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Producto p = new Producto
                {
                    CodigoBarras = txtCodigo.Text,
                    Nombre = txtNombre.Text,
                    IdCategoria = (int)cboCategoria.SelectedValue,
                    PrecioCompra = decimal.Parse(txtPrecioC.Text),
                    PrecioVenta = decimal.Parse(txtPrecioV.Text),
                    StockActual = int.Parse(txtStock.Text),
                    StockMinimo = 5
                };

                if (idProducto.HasValue)
                {
                    p.IdProducto = idProducto.Value;
                    ProductoService.ActualizarProducto(p);
                }
                else ProductoService.CrearProducto(p);

                this.DialogResult = DialogResult.OK;
            }
            catch { MessageBox.Show("Datos inválidos"); }
        }
    }
}