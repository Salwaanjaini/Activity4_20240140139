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

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidasiInput()) return;

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NIM", txtNIM.Text.Trim());
                    cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                    cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text.Trim());
                    cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                    cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                    cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text.Trim());

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0) MessageBox.Show("Data berhasil diupdate", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else MessageBox.Show("Data tidak ditemukan", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SimpanLog("UPDATE GAGAL: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNIM.Text.Trim() == "")
                {
                    MessageBox.Show("Pilih data yang akan dihapus", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Yakin ingin menghapus data?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIM", SqlDbType.Char, 11).Value = txtNIM.Text.Trim();

                    conn.Open();
                    if (cmd.ExecuteNonQuery() > 0) MessageBox.Show("Data berhasil dihapus", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else MessageBox.Show("Data gagal dihapus atau tidak ditemukan", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SimpanLog("DELETE GAGAL: " + ex.Message);
            }
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                    IF OBJECT_ID('dbo.Mahasiswa_Backup') IS NOT NULL
                    BEGIN
                        DELETE FROM dbo.Mahasiswa;
                        INSERT INTO dbo.Mahasiswa SELECT * FROM dbo.Mahasiswa_Backup;
                    END";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Data berhasil direset", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Reset gagal: " + ex.Message, "Error Reset", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Mahasiswa SET Nama='" + txtNama.Text + "' WHERE NIM='" + txtNIM.Text + "'; ";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update berhasil");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnRekapData_Click(object sender, EventArgs e)
        {
            FormRekap frmRekap = new FormRekap();
            frmRekap.Show();
            this.Hide();
        }

        #endregion

        #region --- Event Handler Tambahan (Excel, Refresh, Upload) ---

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadData();
        }

        private void btnImpExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*.xls;*.xlsx", Title = "Pilih File Excel" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Koneksi OLEDB standar untuk membaca Excel
                        string connString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ofd.FileName};Extended Properties=""Excel 12.0 Xml;HDR=YES;""";
                        using (OleDbConnection excelConn = new OleDbConnection(connString))
                        {
                            excelConn.Open();
                            // Mengambil nama Sheet pertama secara otomatis
                            DataTable dtSchema = excelConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            string sheetName = dtSchema.Rows[0]["TABLE_NAME"].ToString();

                            using (OleDbDataAdapter da = new OleDbDataAdapter($"SELECT * FROM [{sheetName}]", excelConn))
                            {
                                DataTable dtExcel = new DataTable();
                                da.Fill(dtExcel);
                                dataGridView1.DataSource = dtExcel; // Menampilkan data Excel ke Grid
                                MessageBox.Show("Data Excel berhasil dimuat! Silakan cek di grid, lalu klik 'Import to Database' untuk menyimpan permanen.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal membaca file Excel. Pastikan file tidak sedang terbuka.\nError: " + ex.Message, "Error Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnImpDb_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada data di Grid untuk diimport!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int countSukses = 0;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue; // Abaikan baris kosong terbawah

                        using (SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            // Menarik value dari DataGridView. Pastikan nama kolom di Excel sesuai dengan nama parameter ini
                            cmd.Parameters.AddWithValue("@NIM", row.Cells["NIM"].Value?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Nama", row.Cells["Nama"].Value?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@JenisKelamin", row.Cells["JenisKelamin"].Value?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@TanggalLahir", Convert.ToDateTime(row.Cells["TanggalLahir"].Value ?? DateTime.Now));
                            cmd.Parameters.AddWithValue("@Alamat", row.Cells["Alamat"].Value?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@KodeProdi", row.Cells["KodeProdi"].Value?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@TanggalDaftar", DateTime.Now);

                            cmd.ExecuteNonQuery();
                            countSukses++;
                        }
                    }
                }
                MessageBox.Show($"{countSukses} Data berhasil disimpan ke Database SQL Server!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData(); // Kembalikan grid ke data asli database
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi error saat memasukkan data ke Database: " + ex.Message, "Error Simpan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            // Tempat jika kamu butuh upload foto/dokumen tambahan
            MessageBox.Show("Fungsi Upload siap untuk dikembangkan!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}