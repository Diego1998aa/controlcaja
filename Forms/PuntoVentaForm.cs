using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using SistemaPOS.Models;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class PuntoVentaForm : Form
    {
        private List<ItemVenta> itemsVenta = new List<ItemVenta>();
        private TextBox txtBusqueda;
        private DataGridView dgvProductos;
        private Label lblTotal;
        private decimal tasaIVA = 0.19m; // IVA Chile 19%

        public PuntoVentaForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = UITheme.DarkBackground;
            this.Size = new Size(1000, 700);

            // Layout Principal
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(20)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Panel de Búsqueda
            Panel pnlBusqueda = new Panel { Dock = DockStyle.Fill };
            txtBusqueda = new TextBox { Width = 500, Location = new Point(0, 10) };
            UITheme.StyleTextBox(txtBusqueda);
            // txtBusqueda.PlaceholderText = "Código de barras o nombre...";
            txtBusqueda.KeyPress += (s, e) => { if(e.KeyChar == 13) BuscarProducto(); };

            Button btnBuscar = new Button { Text = "🔍", Location = new Point(510, 8), Size = new Size(60, 36) };
            UITheme.StyleButton(btnBuscar, UITheme.AccentColor);
            btnBuscar.Click += (s, e) => BuscarProducto();

            pnlBusqueda.Controls.Add(txtBusqueda);
            pnlBusqueda.Controls.Add(btnBuscar);

            // Grid de Productos
            dgvProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = UITheme.PanelBackground,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = UITheme.BorderColor,
                EnableHeadersVisualStyles = false
            };
            
            // Estilo del Grid
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = UITheme.PrimaryColor;
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = UITheme.FontBold;
            dgvProductos.DefaultCellStyle.BackColor = UITheme.PanelBackground;
            dgvProductos.DefaultCellStyle.ForeColor = UITheme.TextPrimary;
            dgvProductos.DefaultCellStyle.SelectionBackColor = UITheme.AccentColor;
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProductos.DefaultCellStyle.Font = UITheme.FontText;

            dgvProductos.Columns.Add("Prod", "Producto");
            dgvProductos.Columns.Add("Cant", "Cantidad");
            dgvProductos.Columns.Add("Total", "Total");

            // Panel de Totales y Acciones
            Panel pnlTotales = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = UITheme.PanelBackground, 
                Padding = new Padding(15) 
            };

            lblTotal = new Label 
            { 
                Text = "TOTAL\n$0", 
                Dock = DockStyle.Top, 
                Height = 100,
                Font = new Font("Segoe UI", 28, FontStyle.Bold), 
                ForeColor = UITheme.SuccessColor, 
                TextAlign = ContentAlignment.MiddleRight 
            };

            Button btnCobrar = new Button { Text = "COBRAR", Dock = DockStyle.Bottom, Height = 80 };
            UITheme.StyleButton(btnCobrar, UITheme.SuccessColor);
            btnCobrar.Click += BtnCobrar_Click;

            Button btnQuitar = new Button { Text = "Eliminar Item", Dock = DockStyle.Bottom, Height = 50 };
            UITheme.StyleButton(btnQuitar, UITheme.ErrorColor);
            btnQuitar.Click += (s, e) => { if(dgvProductos.SelectedRows.Count > 0) { itemsVenta.RemoveAt(dgvProductos.SelectedRows[0].Index); ActualizarGrid(); } };

            Panel spacer = new Panel { Dock = DockStyle.Bottom, Height = 20 };

            pnlTotales.Controls.Add(lblTotal);
            pnlTotales.Controls.Add(btnQuitar);
            pnlTotales.Controls.Add(spacer);
            pnlTotales.Controls.Add(btnCobrar);

            // Agregar todo al Layout
            layout.Controls.Add(pnlBusqueda, 0, 0);
            layout.SetColumnSpan(pnlBusqueda, 2);
            layout.Controls.Add(dgvProductos, 0, 1);
            layout.Controls.Add(pnlTotales, 1, 1);

            this.Controls.Add(layout);
        }

        private void BuscarProducto()
        {
            var prod = ProductoService.BuscarPorCodigo(txtBusqueda.Text.Trim());
            if (prod != null)
            {
                if (prod.StockActual <= 0) { MessageBox.Show("Producto sin stock disponible", "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                
                using (var form = new CantidadForm(prod.Nombre, prod.StockActual))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (form.Cantidad > prod.StockActual)
                        {
                            MessageBox.Show(string.Format("Stock insuficiente. Disponible: {0}", prod.StockActual), "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        var item = itemsVenta.FirstOrDefault(i => i.Producto.IdProducto == prod.IdProducto);
                        if (item != null) 
                        {
                            if (item.Cantidad + form.Cantidad > prod.StockActual)
                            {
                                MessageBox.Show(string.Format("No se puede agregar esa cantidad. Stock restante: {0}", prod.StockActual - item.Cantidad), "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            item.Cantidad += form.Cantidad;
                            item.CalcularSubtotal();
                        }
                        else 
                        {
                            itemsVenta.Add(new ItemVenta(prod, form.Cantidad));
                        }
                        
                        ActualizarGrid();
                        txtBusqueda.Clear();
                        txtBusqueda.Focus();
                    }
                }
            }
            else 
            {
                MessageBox.Show("Producto no encontrado", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtBusqueda.SelectAll();
            }
        }

        private void ActualizarGrid()
        {
            dgvProductos.Rows.Clear();
            decimal total = 0;
            foreach (var item in itemsVenta)
            {
                dgvProductos.Rows.Add(item.Producto.Nombre, item.Cantidad, item.Subtotal.ToString("C"));
                total += item.Subtotal;
            }
            lblTotal.Text = "TOTAL\n" + total.ToString("C");
        }

        private void BtnCobrar_Click(object sender, EventArgs e)
        {
            if (itemsVenta.Count > 0)
            {
                // Asegurarse de que CobrarForm también tenga un estilo decente, 
                // pero por ahora solo invocamos.
                if (new CobrarForm(itemsVenta, tasaIVA).ShowDialog() == DialogResult.OK)
                {
                    itemsVenta.Clear();
                    ActualizarGrid();
                    MessageBox.Show("Venta realizada con éxito", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("No hay items para cobrar", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
