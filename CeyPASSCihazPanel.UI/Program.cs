using AutoUpdaterDotNET;
using CeyPASSCihazPanel.Business.Abstractions;
using CeyPASSCihazPanel.Business.Services;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.DAL.Repositories;
using CeyPASSCihazPanel.UI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;

namespace CeyPASSCihazPanel
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Güncelleme Kontrolü
            try
            {
                // Otomatik güncelleme ayarları
                AutoUpdater.Mandatory = true; // Güncellemeyi zorunlu yap
                AutoUpdater.UpdateMode = Mode.ForcedDownload; // Zorla güncelleme modu
                AutoUpdater.ShowSkipButton = false; // Atla butonunu gizle
                AutoUpdater.ShowRemindLaterButton = false; // Daha sonra hatırlat butonunu gizle
                AutoUpdater.ReportErrors = false; // Hata olursa popup da çıkmasın

                // Güncelleme kontrolünü başlat
                AutoUpdater.Start(@"http://192.168.0.23/CeyPASS-CihazPanel-Updates/update.xml");
            }
            catch (Exception ex)
            {
                // Güncelleme kontrolü başarısız olsa bile program açılsın
                MessageBox.Show($"Güncelleme kontrolü yapılamadı: {ex.Message}");
            }

            var services = new ServiceCollection();
            ConfigureServices(services);
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                var loginForm = serviceProvider.GetRequiredService<loginForm>();
                Application.Run(loginForm);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // UI
            services.AddTransient<loginForm>();
            services.AddTransient<anaForm>();

            // DataAccess
            services.AddScoped<IUserRepository, SqlUserRepository>();
            services.AddScoped<ICihazRepository, SqlCihazRepository>();
            services.AddScoped<IPersonelRepository, SqlPersonelRepository>();
            services.AddScoped<IPuantajsizKartRepository, SqlPuantajsizKartRepository>();
            services.AddScoped<IKisiCihazYetkiRepository, SqlKisiCihazYetkiRepository>();
            services.AddScoped<IPuantajsizKartCihazYetkiRepository, SqlPuantajsizKartCihazYetkiRepository>();

            // Business
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminLookupService, AdminLookUpService>();
            services.AddScoped<IDeviceService, DeviceService>();
        }
    }
}