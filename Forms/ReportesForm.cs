using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using SistemaPOS.Services;

namespace SistemaPOS.Forms
{
    public partial class ReportesForm : Form
    {
        private DateTimePicker dtpInicio, dtpFin;
        private DataGridView dgvReporte;

        public ReportesForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Reportes";
            this.Size = new Size(900, 600);
            this.BackColor = Color.FromArgb(30, 30, 30);

            Panel panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(37, 37, 38) };
            
            Label lblTitulo = new Label { Text = "REPORTES DE VENTAS", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 10), AutoSize = true };
            
            Label lblDe = new Label { Text = "Desde:", ForeColor = Color.White, Location = new Point(20, 45), AutoSize = true };
            dtpInicio = new DateTimePicker { Location = new Point(70, 42), Format = DateTimePickerFormat.Short, Width = 100 };
            
            Label lblA = new Label { Text = "Hasta:", ForeColor = Color.White, Location = new Point(190, 45), AutoSize = true };
            dtpFin = new DateTimePicker { Location = new Point(240, 42), Format = DateTimePickerFormat.Short, Width = 100 };

            Button btnGenerar = new Button { Text = "Generar Reporte", BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(360, 40), Size = new Size(120, 25) };
            btnGenerar.Click += BtnGenerar_Click;

            panelTop.Controls.AddRange(new Control[] { lblTitulo, lblDe, dtpInicio, lblA, dtpFin, btnGenerar });

            dgvReporte = new DataGridView { 
                Dock = DockStyle.Fill, 
                BackgroundColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Black,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            this.Controls.Add(dgvReporte);
            this.Controls.Add(panelTop);
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            var ventas = VentaService.ObtenerResumenVentas(dtpInicio.Value, dtpFin.Value);
            dgvReporte.DataSource = ventas;
        }
    }
}