using System;
using VoiceTTS.Views;
using Prism.Ioc;
using System.Windows;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using VoiceTTS.Models;
using VoiceTTS.ViewModels;
using Profile = VoiceTTS.Models.Profile;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace VoiceTTS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    public partial class App
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override Window CreateShell()
        {
            InitVoices().GetAwaiter().GetResult();

            return Container.Resolve<MainWindow>();
        }



        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {


            containerRegistry.RegisterInstance(DialogCoordinator.Instance);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Profile, TTSViewModel>().ReverseMap();
                cfg.CreateMap<DefaultEditorViewModel, Profile>().ReverseMap();
                cfg.CreateMap<HotKeysViewModel, Profile>().ReverseMap();
                cfg.CreateMap<HotKeysViewModel, HotkeySet>().ReverseMap();
            });
            containerRegistry.RegisterInstance(config.CreateMapper());
            containerRegistry.RegisterInstance(new VoiceMakerClient());
            containerRegistry.RegisterDialogWindow<MahappsWindow>("dialogWindow");
            containerRegistry.RegisterDialog<DefaultEditor, DefaultEditorViewModel>();
            containerRegistry.RegisterDialog<HotKeys, HotKeysViewModel>();
        }

        protected override void OnInitialized()
        {

            base.OnInitialized();

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Debug(e);
        }

        private async Task InitVoices()
        {
            try
            {
                var voiceMaker = Container.Resolve<VoiceMakerClient>();

                using (var db = new TTSContext())
                {
                    if (db.Voices.Any())
                    {
                        var result = MessageBox.Show("Voices already present in database. Do you want to re-fetch voices from online?", "Download voices", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            var voices = await voiceMaker.GetVoicesFromApi();

                            //File.WriteAllText(@".\voices.json",JsonConvert.SerializeObject(voices));


                            db.Voices.RemoveRange(db.Voices.ToList());
                            db.Voices.AddRange(voices);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var voices = await voiceMaker.GetVoicesFromApi();
                        db.Voices.AddRange(voices);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
