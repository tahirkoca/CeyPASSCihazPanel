using CeyPASSCihazPanel.Business.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CeyPASSCihazPanel.UI
{
    public partial class loginForm : Form
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        public loginForm(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;

            InitializeComponent();
            this.KeyPreview = true;
        }

        private void loginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnGiris.PerformClick();
            }
        }
        private void loginForm_Load(object sender, EventArgs e)
        {           
            lblInfo.Text = $"{DateTime.Now.Year} Ⓒ Cey Holding\nDeveloped by ERP and Software Department";
            this.BackColor = Color.FromArgb(234, 239, 245);
            btnGiris.BackColor = Color.FromArgb(72, 100, 140);

            IList<string> userNames;
            try
            {
                userNames = _authService.GetAllUserNames() ?? new List<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcı listesi alınamadı:\n" + ex, "Hata");
                userNames = new List<string>();
            }

            cmbKullaniciAdi.DataSource = null;
            cmbKullaniciAdi.Items.Clear();

            if (userNames.Count == 0)
            {
                // İstersen login butonunu disable et
                // btnGiris.Enabled = false;
                MessageBox.Show("Sistemde tanımlı kullanıcı bulunamadı.", "Bilgi");
                return;
            }

            cmbKullaniciAdi.DataSource = userNames;
            cmbKullaniciAdi.SelectedIndex = 0; // sadece Count>0 iken güvenli
        }
        private void btnGiris_Click(object sender, EventArgs e)
        {
            try
            {
                string enteredUsername = cmbKullaniciAdi.Text?.Trim();
                string enteredPassword = txtSifre.Text ?? "";

                LoginResult result = _authService.Login(enteredUsername, enteredPassword);

                if (!result.Basarili)
                {
                    MessageBox.Show(result.Mesaj,"Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var user = result.Kullanici;
                var panel = _serviceProvider.GetRequiredService<anaForm>();
                panel.SetContext(user.UserName, user.FirmaId);
                panel.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void chkSifreGonder_CheckedChanged(object sender, EventArgs e)
        {
            txtSifre.PasswordChar = chkSifreGonder.Checked ? '\0' : '*';
        }
        private void loginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
