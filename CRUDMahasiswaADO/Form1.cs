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

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    dtMahasiswa = new DataTable();
                    da.Fill(dtMahasiswa);

                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = dtMahasiswa;
                }
                HitungTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message, "Error Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SimpanLog("LOAD DATA GAGAL: " + ex.Message);
            }
        }

        private void HitungTotal()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    ParameterDirection direction = ParameterDirection.Output;
                    SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int) { Direction = direction };

                    cmd.Parameters.Add(outputParam);
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    lblCountMhs.Text = "Total Mahasiswa: " + outputParam.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghitung total: " + ex.Message, "Error Count", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SimpanLog("COUNT TOTAL GAGAL: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            cmbJK.Text = "";
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            txtNIM.Focus();
        }

        private bool ValidasiInput()
        {
            if (txtNIM.Text.Trim() == "") { MessageBox.Show("NIM harus diisi", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtNIM.Focus(); return false; }
            if (txtNama.Text.Trim() == "") { MessageBox.Show("Nama harus diisi", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtNama.Focus(); return false; }
            if (cmbJK.Text.Trim() == "") { MessageBox.Show("Jenis Kelamin harus dipilih", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning); cmbJK.Focus(); return false; }
            if (txtKodeProdi.Text.Trim() == "") { MessageBox.Show("Kode Prodi harus diisi", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtKodeProdi.Focus(); return false; }
            return true;
        }

       
}