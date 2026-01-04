using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using SistemaPOS.Models;
using SistemaPOS.Services;

namespace SistemaPOS.Forms
{
    public partial class PuntoVentaForm : Form
    {
        private List<ItemVenta> itemsVenta = new List<ItemVenta>();
        private TextBox txtBusqueda;
        private DataGridView dgvProductos;
        private Label lblTotal;
        private decimal tasaIVA = 0.16m;

        public PuntoVentaForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            
            txtBusqueda = new TextBox { Location = new Point(10, 10), Size = new Size(400, 30), Font = new Font("Segoe UI", 12) };
            txtBusqueda.KeyPress += (s, e) => { if(e.KeyChar == 13) BuscarProducto(); };
            
            Button btnBuscar = new Button { Text = "🔍", Location = new Point(420, 10), Size = new Size(50, 30), BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnBuscar.Click += (s, e) => BuscarProducto();

            dgvProductos = new DataGridView { Location = new Point(10, 50), Size = new Size(600, 400), BackgroundColor = Color.FromArgb(45, 45, 48), ForeColor = Color.Black, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvProductos.Columns.Add("Prod", "Producto"); dgvProductos.Columns.Add("Cant", "Cant"); dgvProductos.Columns.Add("Total", "Total");

            lblTotal = new Label { Text = "TOTAL: $0.00", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.LimeGreen, Location = new Point(630, 50), AutoSize = true };

            Button btnCobrar = new Button { Text = "COBRAR", Location = new Point(630, 100), Size = new Size(150, 60), BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            btnCobrar.Click += BtnCobrar_Click;

            Button btnQuitar = new Button { Text = "Quitar Item", Location = new Point(10, 460), Size = new Size(100, 30), BackColor = Color.Firebrick, ForeColor = Color.White };
            btnQuitar.Click += (s, e) => { if(dgvProductos.SelectedRows.Count > 0) { itemsVenta.RemoveAt(dgvProductos.SelectedRows[0].Index); ActualizarGrid(); } };

            this.Controls.AddRange(new Control[] { txtBusqueda, btnBuscar, dgvProductos, lblTotal, btnCobrar, btnQuitar });
        }

        private void BuscarProducto()
        {
            var prod = ProductoService.BuscarPorCodigo(txtBusqueda.Text.Trim());
            if (prod != null)
            {
                if (prod.StockActual <= 0) { MessageBox.Show("Sin Stock"); return; }
                var item = itemsVenta.FirstOrDefault(i => i.Producto.IdProducto == prod.IdProducto);
                if (item != null) item.IncrementarCantidad();
                else itemsVenta.Add(new ItemVenta(prod, 1));
                
                ActualizarGrid();
                txtBusqueda.Clear();
            }
            else MessageBox.Show("Producto no encontrado");
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
            lblTotal.Text = $"TOTAL: {(total * (1 + tasaIVA)).ToString("C")}";
        }

        private void BtnCobrar_Click(object sender, EventArgs e)
        {
            if (itemsVenta.Count > 0)
            {
                if (new CobrarForm(itemsVenta, tasaIVA).ShowDialog() == DialogResult.OK)
                {
                    itemsVenta.Clear();
                    ActualizarGrid();
                }
            }
        }
    }
}