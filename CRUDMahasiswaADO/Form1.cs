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

        private void SimpanLog(string pesan)
        {
            try
            {
                using (SqlConnection connLog = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO LogError (waktu, pesan_error) VALUES (GETDATE(), @pesan)";
                    using (SqlCommand cmdLog = new SqlCommand(query, connLog))
                    {
                        cmdLog.Parameters.AddWithValue("@pesan", pesan);
                        connLog.Open();
                        cmdLog.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception) { /* Abaikan jika pencatatan log error juga gagal */ }
        }

        #endregion

        #region --- Event Handler CRUD & DataGridView ---

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            txtNIM.Text = row.Cells["NIM"].Value?.ToString();
            txtNama.Text = row.Cells["Nama"].Value?.ToString();
            cmbJK.Text = row.Cells["JenisKelamin"].Value?.ToString();

            if (row.Cells["TanggalLahir"].Value != null && row.Cells["TanggalLahir"].Value != DBNull.Value)
            {
                dtpTanggalLahir.Value = Convert.ToDateTime(row.Cells["TanggalLahir"].Value);
            }

            txtAlamat.Text = row.Cells["Alamat"].Value?.ToString();
            txtKodeProdi.Text = row.Cells["KodeProdi"].Value?.ToString();
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void btnConnect_Click(object sender, EventArgs e) { ConnectDatabase(); }

        private void btnLoad_Click(object sender, EventArgs e) { LoadData(); }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                    cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                    cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                    cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                    cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text);
                    cmd.Parameters.AddWithValue("@TanggalDaftar", DateTime.Now);
                    cmd.ExecuteNonQuery();

                    SqlCommand cmdLog = new SqlCommand(@"INSERT INTO LogAktivitasSalah (aktivitas,waktu) VALUES (@aktivitas,GETDATE())", conn, trans);
                    cmdLog.Parameters.AddWithValue("@aktivitas", "INSERT MAHASISWA : " + txtNIM.Text);
                    cmdLog.ExecuteNonQuery();

                    trans.Commit();
                    MessageBox.Show("Data berhasil ditambahkan", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    SimpanLog("INSERT ERROR : " + ex.Message);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

       