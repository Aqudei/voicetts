using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using AutoMapper;
using LiteDB;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace VoiceTTS.ViewModels
{
    public class HotKeysViewModel : BindableBase, IDialogAware
    {
        private readonly IMapper _mapper;
        private readonly IDialogCoordinator _dialogCoordinator;
        private string _keyAppendEmphasis;
        private string _keyAppendPause;
        private string _keyAppendPitch;
        private string _keyAppendSayAs;
        private string _keyAppendSpeed;
        private string _keyAppendVoice;
        private string _keyAppendVolume;
        private DelegateCommand _closeCommand;
        private string _keyManualSend;
        private string _keyPitchDown;
        private string _keyPitchUp;
        private string _keySpeedDown;
        private string _keySpeedUp;
        private string _keyVolumeDown;
        private string _keyVolumeUp;
        private DelegateCommand _saveCommand;
        private Model.Profile _profile;
        private string _title;
        private string _keyAppRestart;
        private readonly LiteDatabase _db;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string KeyManualSend
        {
            get => _keyManualSend;
            set => SetProperty(ref _keyManualSend, value);
        }

        public string KeyVolumeUp
        {
            get => _keyVolumeUp;
            set => SetProperty(ref _keyVolumeUp, value);
        }

        public string KeyVolumeDown
        {
            get => _keyVolumeDown;
            set => SetProperty(ref _keyVolumeDown, value);
        }

        public string KeyPitchDown
        {
            get => _keyPitchDown;
            set => SetProperty(ref _keyPitchDown, value);
        }

        public string KeyPitchUp
        {
            get => _keyPitchUp;
            set => SetProperty(ref _keyPitchUp, value);
        }

        public string KeySpeedUp
        {
            get => _keySpeedUp;
            set => SetProperty(ref _keySpeedUp, value);
        }

        public string KeySpeedDown
        {
            get => _keySpeedDown;
            set => SetProperty(ref _keySpeedDown, value);
        }

        public string KeyAppendVoice
        {
            get => _keyAppendVoice;
            set => SetProperty(ref _keyAppendVoice, value);
        }

        public string KeyAppendPause
        {
            get => _keyAppendPause;
            set => SetProperty(ref _keyAppendPause, value);
        }

        public string KeyAppendEmphasis
        {
            get => _keyAppendEmphasis;
            set => SetProperty(ref _keyAppendEmphasis, value);
        }

        public string KeyAppendSpeed
        {
            get => _keyAppendSpeed;
            set => SetProperty(ref _keyAppendSpeed, value);
        }

        public string KeyAppendPitch
        {
            get => _keyAppendPitch;
            set => SetProperty(ref _keyAppendPitch, value);
        }

        public string KeyAppendVolume
        {
            get => _keyAppendVolume;
            set => SetProperty(ref _keyAppendVolume, value);
        }

        public string KeyAppendSayAs
        {
            get => _keyAppendSayAs;
            set => SetProperty(ref _keyAppendSayAs, value);
        }

        public string KeyAppRestart
        {
            get => _keyAppRestart;
            set => SetProperty(ref _keyAppRestart, value);
        }

        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(DoSave);
        public DelegateCommand CloseCommand => _closeCommand ??= new DelegateCommand(DoClose);

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //KeyManualSend = Properties.Settings.Default.KeyManualSend;
            //KeyVolumeUp = Properties.Settings.Default.KeyVolumeUp;
            //KeyVolumeDown = Properties.Settings.Default.KeyVolumeDown;
            //KeyPitchDown = Properties.Settings.Default.KeyPitchDown;
            //KeyPitchUp = Properties.Settings.Default.KeyPitchUp;
            //KeySpeedUp = Properties.Settings.Default.KeySpeedUp;
            //KeySpeedDown = Properties.Settings.Default.KeySpeedDown;
            //KeyAppendVoice = Properties.Settings.Default.KeyAppendVoice;
            //KeyAppendPause = Properties.Settings.Default.KeyAppendPause;
            //KeyAppendEmphasis = Properties.Settings.Default.KeyAppendEmphasis;
            //KeyAppendSpeed = Properties.Settings.Default.KeyAppendSpeed;
            //KeyAppendPitch = Properties.Settings.Default.KeyAppendPitch;
            //KeyAppendVolume = Properties.Settings.Default.KeyAppendVolume;
            //KeyAppendSayAs = Properties.Settings.Default.KeyAppendSayAs;
            _profile = parameters.GetValue<Model.Profile>("Profile");
            _mapper.Map(_profile, this);
            Title = $"HotKeys Setup for Profile: {_profile.ProfileName}";
        }

        public HotKeysViewModel(IMapper mapper, IDialogCoordinator dialogCoordinator, LiteDatabase database)
        {
            _mapper = mapper;
            _dialogCoordinator = dialogCoordinator;
            _db = database;
        }

        public event Action<IDialogResult> RequestClose;

        private void DoClose()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        private async void DoSave()
        {
            //Properties.Settings.Default.KeyManualSend = KeyManualSend;
            //Properties.Settings.Default.KeyVolumeUp = KeyVolumeUp;
            //Properties.Settings.Default.KeyVolumeDown = KeyVolumeDown;
            //Properties.Settings.Default.KeyPitchDown = KeyPitchDown;
            //Properties.Settings.Default.KeyPitchUp = KeyPitchUp;
            //Properties.Settings.Default.KeySpeedUp = KeySpeedUp;
            //Properties.Settings.Default.KeySpeedDown = KeySpeedDown;

            //Properties.Settings.Default.KeyAppendVoice = KeyAppendVoice;
            //Properties.Settings.Default.KeyAppendPause = KeyAppendPause;
            //Properties.Settings.Default.KeyAppendEmphasis = KeyAppendEmphasis;
            //Properties.Settings.Default.KeyAppendSpeed = KeyAppendSpeed;
            //Properties.Settings.Default.KeyAppendPitch = KeyAppendPitch;
            //Properties.Settings.Default.KeyAppendVolume = KeyAppendVolume;
            //Properties.Settings.Default.KeyAppendSayAs = KeyAppendSayAs;

            _mapper.Map(this, _profile);

            var keys = new List<string>
            {
                KeyManualSend,
                KeyVolumeUp,
                KeyVolumeDown,
                KeyPitchDown,
                KeyPitchUp,
                KeySpeedUp,
                KeySpeedDown,
                KeyAppendVoice,
                KeyAppendPause,
                KeyAppendEmphasis,
                KeyAppendSpeed,
                KeyAppendPitch,
                KeyAppendVolume,
                KeyAppendSayAs,
                KeyAppRestart
        }.Where(k => !string.IsNullOrWhiteSpace(k));

            var distinctCount = keys.Distinct().Count();

            if (keys.Count() > distinctCount)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", "Found duplicate hotkeys!");
                return;
            }

            var profileCollection = _db.GetCollection<Model.Profile>();
            profileCollection.Update(_profile);
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));

        }
    }
}