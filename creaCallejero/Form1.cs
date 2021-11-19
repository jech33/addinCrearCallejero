using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.DataAccess.Client;
using System.Configuration;

namespace creaCallejero
{
    public partial class Form1 : Form
    {
        public string cnn;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {

                DataTable dt = new DataTable();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = Conectar();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $"select * from {Global.dbTableName}"; // Nombre de tabla
                OracleDataAdapter da = new OracleDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

        }

        private OracleConnection Conectar()
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = cnn;
            try { conn.Open(); }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return conn;

        }

    }
}
