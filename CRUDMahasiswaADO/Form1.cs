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

        private void Form1_Load(object sender, EventArgs e)
        {
            // Inisialisasi komponen pilihan Jenis Kelamin
            cmbJK.Items.Clear();
            cmbJK.Items.Add("L");
            cmbJK.Items.Add("P");

            // Konfigurasi Grid Tampilan Data agar rapi dan aman
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Menghubungkan event handler grid data
            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.DataError += dataGridView1_DataError;

            // Memuat data pertama kali saat form dibuka
            LoadData();
        }

        #region --- Fungsi Inti Database & UI ---

        private void ConnectDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi berhasil", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal: " + ex.Message, "Error Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SimpanLog("KONEKSI GAGAL: " + ex.Message);
            }
        }

        
        #endregion
    }
}