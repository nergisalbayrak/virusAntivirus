using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace virus_antivirus
{
    public class MainForm : Form
    {
        // UI
        private Button btnSelectFolder, btnScan;
        private Button btnStartProtection, btnStopProtection;
        private Button btnStartVirus, btnStopVirus;
        private TextBox txtFolder;
        private ListBox lstLog;

        // Real-time protection
        private FileSystemWatcher watcher;

        // Virus simulation
        private System.Windows.Forms.Timer virusTimer;
        private int virusCounter = 0;
        private bool virusRunning = false;

        public MainForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Mini Antivirus";
            this.Width = 900;
            this.Height = 550;

            btnSelectFolder = new Button { Text = "Klas�r Se�", Left = 10, Top = 10, Width = 100 };
            btnSelectFolder.Click += BtnSelectFolder_Click;

            txtFolder = new TextBox { Left = 120, Top = 12, Width = 500 };

            btnScan = new Button { Text = "Manuel Tara", Left = 630, Top = 10, Width = 120 };
            btnScan.Click += BtnScan_Click;

            btnStartProtection = new Button { Text = "Ger�ek Zamanl� Ba�lat", Left = 10, Top = 45, Width = 180 };
            btnStartProtection.Click += BtnStartProtection_Click;

            btnStopProtection = new Button { Text = "Koruma Durdur", Left = 200, Top = 45, Width = 150 };
            btnStopProtection.Click += BtnStopProtection_Click;

            btnStartVirus = new Button { Text = "Vir�s Ba�lat", Left = 370, Top = 45, Width = 120 };
            btnStartVirus.Click += BtnStartVirus_Click;

            btnStopVirus = new Button { Text = "Vir�s Durdur", Left = 500, Top = 45, Width = 120 };
            btnStopVirus.Click += BtnStopVirus_Click;

            lstLog = new ListBox { Left = 10, Top = 85, Width = 860, Height = 420 };

            this.Controls.AddRange(new Control[]
            {
                btnSelectFolder, txtFolder, btnScan,
                btnStartProtection, btnStopProtection,
                btnStartVirus, btnStopVirus,
                lstLog
            });
        }

        // -------------------- FOLDER --------------------
        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = dialog.SelectedPath;
                Log($"Klas�r se�ildi: {txtFolder.Text}");
            }
        }

        // -------------------- MANUAL SCAN --------------------
        private void BtnScan_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtFolder.Text))
            {
                MessageBox.Show("Ge�erli bir klas�r se�iniz.");
                return;
            }

            Log("Manuel tarama ba�lad�...");
            foreach (var file in Directory.GetFiles(txtFolder.Text, "*.*", SearchOption.AllDirectories))
            {
                AnalyzeFile(file);
            }
            Log("Manuel tarama tamamland�.");
        }

        // -------------------- REAL-TIME PROTECTION --------------------
        private void BtnStartProtection_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtFolder.Text))
            {
                MessageBox.Show("�nce klas�r se�iniz.");
                return;
            }

            watcher = new FileSystemWatcher(txtFolder.Text);
            watcher.Created += Watcher_Created;
            watcher.EnableRaisingEvents = true;

            Log("Ger�ek zamanl� koruma AKT�F.");
        }

        private void BtnStopProtection_Click(object sender, EventArgs e)
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
                Log("Ger�ek zamanl� koruma DURDURULDU.");
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            // UI thread�e ge�
            this.Invoke(new Action(() =>
            {
                Log($"[RT] Yeni dosya alg�land�: {e.Name}");
                AnalyzeFile(e.FullPath);
            }));
        }

        // -------------------- VIRUS SIMULATION --------------------
        private void BtnStartVirus_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtFolder.Text))
            {
                MessageBox.Show("Vir�s sim�lasyonu i�in klas�r se�iniz.");
                return;
            }

            if (virusRunning) return;

            virusTimer = new System.Windows.Forms.Timer();
            virusTimer.Interval = 2000;
            virusTimer.Tick += (s, args) => CreateVirusFile(null);
            virusTimer.Start();

            virusRunning = true;
            Log("Vir�s sim�lasyonu BA�LATILDI.");
        }

        private void BtnStopVirus_Click(object sender, EventArgs e)
        {
            virusTimer?.Dispose();
            virusRunning = false;
            Log("Vir�s sim�lasyonu DURDURULDU.");
        }

        private void CreateVirusFile(object state)
        {
            try
            {
                virusCounter++;
                string filePath = Path.Combine(
                    txtFolder.Text,
                    $"virus_copy_{virusCounter}.txt"
                );

                File.WriteAllText(filePath,
                    "This is a simulated malicious file.\n" +
                    "simulated malicious activity\n" +
                    DateTime.Now.ToString());

                this.Invoke(new Action(() =>
                {
                    Log($"[VIRUS] Dosya �retildi: {Path.GetFileName(filePath)}");
                }));
            }
            catch { }
        }

        // -------------------- ANALYSIS --------------------
        private void AnalyzeFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            bool threatDetected = false;

            // �MZA TABANLI
            if (fileName.StartsWith("virus_copy_", StringComparison.OrdinalIgnoreCase))
            {
                Log($"[TEHL�KE] ��pheli dosya ad�: {fileName}");
                threatDetected = true;
            }

            // ��ER�K TABANLI
            try
            {
                string content = File.ReadAllText(filePath);
                if (content.Contains("simulated malicious", StringComparison.OrdinalIgnoreCase))
                {
                    Log($"[TEHL�KE] ��pheli i�erik tespit edildi: {fileName}");
                    threatDetected = true;
                }
            }
            catch
            {
                Log($"[INFO] Dosya okunamad�: {fileName}");
            }

            // TEHD�T VARSA M�DAHALE
            if (threatDetected)
            {
                HandleThreat(filePath);
                return;
            }

            // HASH (bilgilendirme)
            string hash = CalculateSHA256(filePath);
            Log($"[TEM�Z] {fileName} | {hash.Substring(0, 10)}...");
        }


        // -------------------- TEHD�T M�DAHALES� --------------------
        private void HandleThreat(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);

                // Vir�s� durdur
                if (virusRunning)
                {
                    virusTimer?.Dispose();
                    virusRunning = false;
                    Log("[AKS�YON] Vir�s sim�lasyonu otomatik olarak DURDURULDU.");
                }

                // Dosyay� sil
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Log($"[AKS�YON] Zararl� dosya S�L�ND�: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Log($"[HATA] M�dahale ba�ar�s�z: {ex.Message}");
            }
        }


        // -------------------- LOG --------------------
        private void Log(string message)
        {
            lstLog.Items.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        // Dosyan�n SHA256 hash'ini hesaplayan yard�mc� metot
        private string CalculateSHA256(string filePath)
        {
            try
            {
                using (FileStream stream = File.OpenRead(filePath))
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                        sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }
            }
            catch
            {
                return "HASH_HESAPLANAMADI";
            }
        }
    }
}