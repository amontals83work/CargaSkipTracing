using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace CargaSkipTracing
{
    public partial class Form1 : Form
    {
        ConexionDB conn = new ConexionDB();
        MCCommand mcComm = new MCCommand();
        private OpenFileDialog openFileDialog;

        public Form1()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            CenterToScreen();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboCartera.DataSource = conn.Select("SELECT DISTINCT CASE WHEN idCliente in (84, 85, 86, 87, 88) THEN 84 ELSE idCliente END idCliente, CASE WHEN idCliente IN (84, 85, 86, 87, 88) THEN 'ARBORKNOT' ELSE Descripcion END Descripcion FROM clientes WHERE idcliente IN (41, 81, 83, 84, 85, 86, 87, 88)");
            cboCartera.DisplayMember = "Descripcion";
            cboCartera.ValueMember = "idCliente";
        }

        private void btnFichero_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFichero.Text = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("Debe seleccionar un fichero para cargar los expedientes");
                return;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            GuardarLista(Convert.ToInt32(cboCartera.SelectedValue));
        }

        private void GuardarLista(int id)
        {
            string hoja = nHoja();

            try
            {
                OleDbConnection oleConnection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + txtFichero.Text.Trim() + ";Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1';");
                OleDbDataAdapter oleAdapter = new OleDbDataAdapter("SELECT * FROM [" + hoja + "$]", oleConnection);

                DataSet ds = new DataSet();
                oleAdapter.Fill(ds);
                oleConnection.Close();

                DataTable dt = ds.Tables[0];
                ConexionDB.AbrirConexion();

                switch (id)
                {
                    case 41://TDX
                        {
                            break;
                        }
                    case 81://AXACTOR
                        {
                            string tabla = "AXATELEFONOS";//AXATELEFONOS//_Pruebas_SkTr_AXACTOR
                            Telefonoes_AXACTOR(dt, tabla);
                            break;
                        }
                    case 83://NASSAU
                        {
                            break;
                        }
                    case 84://ARBORKNOT
                        {
                            break;
                        }
                    default:
                        MessageBox.Show("Cliente no contemplado en la aplicación.");
                        break;
                }
                ConexionDB.CerrarConexion();
            }
            catch (Exception e)
            {
                MessageBox.Show("error " + e);
            }
        }

        private void Telefonos_AXACTOR(DataTable dt, string tabla)
        {
            mcComm.command.Connection = conn.ObtenerConexion();
            foreach (DataRow fila in dt.Rows)
            {
                string idcliente = fila[1].ToString();
                string telefono = fila[4].ToString();

                mcComm.command.CommandText = "INSERT INTO " + tabla + " (idclienteald, Telefono, Procedencia, fenvio) " +
                                            "SELECT @value1, @value2, 'SkiptracingRP', GETDATE() " +
                                            "WHERE NOT EXISTS (SELECT 1 FROM " + tabla + " WHERE Telefono = @value2)";
                mcComm.command.Parameters.Clear();
                mcComm.command.Parameters.AddWithValue("@value1", idcliente);
                mcComm.command.Parameters.AddWithValue("@value2", telefono);
                mcComm.ExecuteNonQuery();
            }
            MessageBox.Show("Guardado con éxito.");
        }

        private string nHoja()
        {
            string hoja;
            var xlsApp = new Excel.Application();
            xlsApp.Workbooks.Open(txtFichero.Text.Trim());

            hoja = xlsApp.Sheets[1].Name;

            xlsApp.DisplayAlerts = false;
            xlsApp.Workbooks.Close();
            xlsApp.DisplayAlerts = true;

            xlsApp.Quit();
            xlsApp = null;

            return hoja;
        }
    }
}
