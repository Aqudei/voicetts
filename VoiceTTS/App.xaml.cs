using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VoiceTTS.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using CsvHelper.Configuration;
using LiteDB;
using MahApps.Metro.Controls.Dialogs;
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
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(DialogCoordinator.Instance);
            var appLocalFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appLocalFolder, Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName));
            Directory.CreateDirectory(dbPath);
            var db = new LiteDatabase(Path.Combine(dbPath, @"MyData.db"));

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
        }
    }
}
