using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VoiceTTS.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using LiteDB;
using MahApps.Metro.Controls.Dialogs;
using Prism.Regions;
using VoiceTTS.Model;
using VoiceTTS.ViewModels;
using Profile = VoiceTTS.Model.Profile;



namespace VoiceTTS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override Window CreateShell()
        {
            Logger.Info($"Creating Shell...");
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            Logger.Info($"Registering Types");

            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName));
            var globalVariables = new GlobalVariables
            {
                AppDataFolder = appDataFolder
            };

            Directory.CreateDirectory(appDataFolder);

            var dbPath = Path.Combine(appDataFolder, "MyData.db");
            globalVariables.DbPath = dbPath;
            globalVariables.AudioPath = Path.Combine(appDataFolder, "Audio");
            Directory.CreateDirectory(globalVariables.AudioPath);

            var audioFiles = Directory.GetFiles(globalVariables.AudioPath);

            for (var i = audioFiles.Length - 1; i >= 0; i--)
            {
                File.Delete(audioFiles[i]);
            }


            var db = new LiteDatabase(dbPath);

            containerRegistry.RegisterInstance(DialogCoordinator.Instance);
            containerRegistry.RegisterInstance(globalVariables);
            containerRegistry.Register<VoiceMaker>();

            var profileCollection = db.GetCollection<Profile>();
            var profiles = profileCollection.FindAll();
            if (!profiles.Any())
            {
                for (var i = 0; i < 10; i++)
                {
                    var newProfile = new Profile()
                    {
                        ProfileName = $"Profile Slot #{i + 1}"
                    };
                    profileCollection.Insert(newProfile);
                }
            }

            containerRegistry.RegisterInstance(db);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Profile, TTSViewModel>().ReverseMap();
                cfg.CreateMap<DefaultEditorViewModel, Profile>().ReverseMap();
                cfg.CreateMap<HotKeysViewModel, Profile>().ReverseMap();
                cfg.CreateMap<HotKeysViewModel, HotkeySet>().ReverseMap();
            });
            containerRegistry.RegisterInstance(config.CreateMapper());

            containerRegistry.RegisterDialogWindow<MahappsWindow>("dialogWindow");
            containerRegistry.RegisterDialog<DefaultEditor, DefaultEditorViewModel>();
            containerRegistry.RegisterDialog<HotKeys, HotKeysViewModel>();



            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ContentRegion", typeof(TTSView));
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error(e.Exception);
        }
    }
}
