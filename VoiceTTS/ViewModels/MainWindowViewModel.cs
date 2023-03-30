using System.Diagnostics;
using System.Reflection;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using VoiceTTS.Views;

namespace VoiceTTS.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "VT STT-TTS";
        private DelegateCommand _restartCommand;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public MainWindowViewModel()
        {

        }

        //public DelegateCommand RestartCommand => _restartCommand ??= new DelegateCommand(DoRestartApp);

        //private void DoRestartApp()
        //{
        //    var processStartInfo = new ProcessStartInfo
        //    {
        //        Arguments =
        //            "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetEntryAssembly().Location + "\"",
        //        WindowStyle = ProcessWindowStyle.Hidden,
        //        CreateNoWindow = true,
        //        FileName = "cmd.exe"
        //    };
        //    Process.Start(processStartInfo);
        //    Process.GetCurrentProcess().Kill();
        //}
    }
}