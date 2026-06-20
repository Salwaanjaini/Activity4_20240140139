using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form3 : Form
    {
        // Gunakan Integrated Security=True untuk Windows Authentication
        private static readonly string connectionString = "Data Source=MSI\\AWAYYY;Initial Catalog=DBAkademikADO;Integrated Security=True";

        public string Prodi { get; set; }
        public DateTime TglMasuk { get; set; }

        public Form3(string prodi, DateTime tglMasuk)
        {
            InitializeComponent();
            this.Prodi = prodi;
            this.TglMasuk = tglMasuk;

            try
            {
                DataTable dt = new DataTable();

                // 1. Ambil data dari Database menggunakan Stored Procedure
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Report", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@inProdi", Prodi);
                    cmd.Parameters.AddWithValue("@inTglMsuk", TglMasuk.Year.ToString());

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                // 2. Validasi jika data kosong
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Data tidak ditemukan untuk kriteria tersebut.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return; // Hentikan proses jika data tidak ada
                }

                // 3. Persiapkan Crystal Report
                // Nama TableName HARUS SAMA dengan yang ada di Database Expert (.rpt)
                dt.TableName = "Mahasiswa";

                // Masukkan DataTable yang sudah berisi data ke dalam laporan
                CrystalReport11.SetDataSource(dt);

                // 4. Tampilkan laporan ke dalam Viewer dan Refresh
                crystalReportViewer1.ReportSource = CrystalReport11;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void LoadReport()
        {
            
        }

        // Biarkan kosong agar tidak error di Designer jika sudah telanjur terhubung
        private void CrystalReport11_InitReport(object sender, EventArgs e)
        {

        }

        // Biarkan kosong
        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}