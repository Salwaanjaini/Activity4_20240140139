using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb; // Tambahan wajib untuk membaca Excel
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        // Pengaturan string koneksi database SQL Server
        private readonly string connectionString = "Data Source=MSI\\AWAYYY;Initial Catalog=DBAkademikADO;Integrated Security=True";
        private DataTable dtMahasiswa = new DataTable();

        public Form1()
        {
            InitializeComponent();
        }

        
}