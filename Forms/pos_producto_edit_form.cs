using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using SistemaPOS.Models;
using SistemaPOS.Services;

namespace SistemaPOS.Forms
{
    public partial class ProductoEditForm : Form
    {
        private int? idProducto;
        private Producto productoActual;
        private TextBox txtCodigo, txtSKU, txtNombre, txtDescripcion;
        private TextBox txtPrecioCompra, txtPrecioVenta, txtStock, txtStockMin;
        private ComboBox cboCategoria;

        public ProductoEditForm(int? idProducto = null)
        {
            this.idProducto = idProducto;
            InitializeComponent();
            CargarCategorias();
            
            if (idProducto.HasValue)
            {
                CargarProducto();
            }
        }

        private void InitializeComponent()
        {
            this.Text = idProducto.HasValue ? "Editar Producto" : "Nuevo Producto";
            this.Size = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(37, 37, 38);

            // Panel principal
            Panel panelPrincipal = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(540, 570),
                BackColor = Color.FromArgb(45, 45, 48),
                AutoScroll = true
            };

            int y = 20;

            // Título
            Label lblTitulo = new Label
            {
                Text = idProducto.HasValue ? "EDITAR PRODUCTO" : "NUEVO PRODUCTO",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, y),
                AutoSize = false,
                Size = new Size(500, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelPrincipal.Controls.Add(lblTitulo);
            y += 50;

            // Código de barras
            panelPrincipal.Controls.Add(CrearLabel("Código de Barras:", 20, y));
            txtCodigo = CrearTextBox(20, y + 25, 240);
            panelPrincipal.Controls.Add(txtCodigo);

            // SKU
            panelPrincipal.Controls.Add(CrearLabel("SKU:", 280, y));
            txtSKU = CrearTextBox(280, y + 25, 240);
            panelPrincipal.Controls.Add(txtSKU);
            y += 70;

            // Nombre
            panelPrincipal.Controls.Add(CrearLabel("Nombre del Producto: *", 20, y));
            txtNombre = CrearTextBox(20, y + 25, 500);
            panelPrincipal.Controls.Add(txtNombre);
            y += 70;

            // Descripción
            panelPrincipal.Controls.Add(CrearLabel("Descripción:", 20, y));
            txtDescripcion = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, y + 25),
                Size = new Size(500, 60),
                Multiline = true
            };
            panelPrincipal.Controls.Add(txtDescripcion);
            y += 95;

            // Categoría
            panelPrincipal.Controls.Add(CrearLabel("Categoría:", 20, y));
            cboCategoria = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, y + 25),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panelPrincipal.Controls.Add(cboCategoria);
            y += 70;

            // Precio de compra
            panelPrincipal.Controls.Add(CrearLabel("Precio de Compra: *", 20, y));
            txtPrecioCompra = CrearTextBox(20, y + 25, 240);
            txtPrecioCompra.KeyPress += ValidarNumeroDecimal;
            panelPrincipal.Controls.Add(txtPrecioCompra);

            // Precio de venta
            panelPrincipal.Controls.Add(CrearLabel("Precio de Venta: *", 280, y));
            txtPrecioVenta = CrearTextBox(280, y + 25, 240);
            txtPrecioVenta.KeyPress += ValidarNumeroDecimal;
            panelPrincipal.Controls.Add(txtPrecioVenta);
            y += 70;

            // Stock actual
            panelPrincipal.Controls.Add(CrearLabel("Stock Actual: *", 20, y));
            txtStock = CrearTextBox(20, y + 25, 240);
            txtStock.KeyPress += ValidarNumeroEntero;
            panelPrincipal.Controls.Add(txtStock);

            // Stock mínimo
            panelPrincipal.Controls.Add(CrearLabel("Stock Mínimo: *", 280, y));
            txtStockMin = CrearTextBox(280, y + 25, 240);
            txtStockMin.KeyPress += ValidarNumeroEntero;
            txtStockMin.Text = "5";
            panelPrincipal.Controls.Add(txtStockMin);
            y += 70;

            // Nota obligatorios
            Label lblNota = new Label
            {
                Text = "* Campos obligatorios",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.LightGray,
                Location = new Point(20, y),
                AutoSize = true
            };
            panelPrincipal.Controls.Add(lblNota);
            y += 40;

            // Botones
            Button btnGuardar = new Button
            {
                Text = "💾 GUARDAR",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(240, 45),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            panelPrincipal.Controls.Add(btnGuardar);

            Button btnCancelar = new Button
            {
                Text = "✗ CANCELAR",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(280, y),
                Size = new Size(240, 45),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            panelPrincipal.Controls.Add(btnCancelar);

            this.Controls.Add(panelPrincipal);
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private TextBox CrearTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, y),
                Size = new Size(width, 25)
            };
        }

        private void ValidarNumeroDecimal(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            TextBox txt = sender as TextBox;
            if (e.KeyChar == '.' && txt.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void ValidarNumeroEntero(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void CargarCategorias()
        {
            var categorias = ProductoService.ObtenerCategorias();
            cboCategoria.Items.Clear();
            cboCategoria.Items.Add("-- Sin categoría --");
            
            foreach (var cat in categorias)
            {
                cboCategoria.Items.Add(cat);
            }
            
            cboCategoria.DisplayMember = "NombreCategoria";
            cboCategoria.ValueMember = "IdCategoria";
            cboCategoria.SelectedIndex = 0;
        }

        private void CargarProducto()
        {
            var productos = ProductoService.ObtenerTodos(false);
            productoActual = productos.FirstOrDefault(p => p.IdProducto == idProducto.Value);

            if (productoActual != null)
            {
                txtCodigo.Text = productoActual.CodigoBarras;
                txtSKU.Text = productoActual.SKU;
                txtNombre.Text = productoActual.Nombre;
                txtDescripcion.Text = productoActual.Descripcion;
                txtPrecioCompra.Text = productoActual.PrecioCompra.ToString("F2");
                txtPrecioVenta.Text = productoActual.PrecioVenta.ToString("F2");
                txtStock.Text = productoActual.StockActual.ToString();
                txtStockMin.Text = productoActual.StockMinimo.ToString();

                // Seleccionar categoría
                if (productoActual.IdCategoria > 0)
                {
                    for (int i = 1; i < cboCategoria.Items.Count; i++)
                    {
                        Categoria cat = cboCategoria.Items[i] as Categoria;
                        if (cat != null && cat.IdCategoria == productoActual.IdCategoria)
                        {
                            cboCategoria.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio.", "Campo Requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrecioCompra.Text, out decimal precioCompra) || precioCompra < 0)
            {
                MessageBox.Show("Ingrese un precio de compra válido.", "Precio Inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecioCompra.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta) || precioVenta <= 0)
            {
                MessageBox.Show("Ingrese un precio de venta válido.", "Precio Inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecioVenta.Focus();
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Ingrese un stock válido.", "Stock Inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStock.Focus();
                return;
            }

            if (!int.TryParse(txtStockMin.Text, out int stockMin) || stockMin < 0)
            {
                MessageBox.Show("Ingrese un stock mínimo válido.", "Stock Mínimo Inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStockMin.Focus();
                return;
            }

            // Crear o actualizar producto
            Producto producto = new Producto
            {
                CodigoBarras = string.IsNullOrWhiteSpace(txtCodigo.Text) ? null : txtCodigo.Text.Trim(),
                SKU = string.IsNullOrWhiteSpace(txtSKU.Text) ? null : txtSKU.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
                PrecioCompra = precioCompra,
                PrecioVenta = precioVenta,
                StockActual = stock,
                StockMinimo = stockMin,
                IdCategoria = cboCategoria.SelectedIndex > 0 
                    ? ((Categoria)cboCategoria.SelectedItem).IdCategoria 
                    : 0
            };

            bool exito;
            if (idProducto.HasValue)
            {
                producto.IdProducto = idProducto.Value;
                exito = ProductoService.ActualizarProducto(producto);
            }
            else
            {
                exito = ProductoService.CrearProducto(producto);
            }

            if (exito)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Error al guardar el producto. Verifique que el código de barras o SKU no estén duplicados.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}