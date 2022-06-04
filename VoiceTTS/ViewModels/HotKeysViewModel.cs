using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using AutoMapper;
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
        private Models.Profile _profile;
        private string _title;
        private string _keyAppRestart;
        private string _keyAppendBreathing;
        private string _keyAppendSoft;
        private string _keyAppendWhispered;
        private string _keyAppendConversational;
        private string _keyAppendNews;
        private string _keyAppendCustomerSupport;
        private string _keyAppendAssistant;
        private string _keyAppendHappy;
        private string _keyAppendEmphatic;
        private string _keyAppendClam;
        private string _keyAppendDefault;
        private string keyAppendSad;
        private string keyAppendAngry;
        private string keyAppendExcited;
        private string keyAppendFriendly;
        private string keyAppendHopeful;
        private string keyAppendShouting;
        private string keyAppendTerrified;
        private string keyAppendUnFriendly;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string KeyManualSend
        {
            get => _keyManualSend;
            set => SetProperty(ref _keyManualSend, value?.ToUpper());
        }

        public string KeyVolumeUp
        {
            get => _keyVolumeUp;
            set => SetProperty(ref _keyVolumeUp, value?.ToUpper());
        }

        public string KeyVolumeDown
        {
            get => _keyVolumeDown;
            set => SetProperty(ref _keyVolumeDown, value?.ToUpper());
        }

        public string KeyPitchDown
        {
            get => _keyPitchDown;
            set => SetProperty(ref _keyPitchDown, value?.ToUpper());
        }

        public string KeyPitchUp
        {
            get => _keyPitchUp;
            set => SetProperty(ref _keyPitchUp, value?.ToUpper());
        }

        public string KeySpeedUp
        {
            get => _keySpeedUp;
            set => SetProperty(ref _keySpeedUp, value?.ToUpper());
        }

        public string KeySpeedDown
        {
            get => _keySpeedDown;
            set => SetProperty(ref _keySpeedDown, value?.ToUpper());
        }

        public string KeyAppendVoice
        {
            get => _keyAppendVoice;
            set => SetProperty(ref _keyAppendVoice, value?.ToUpper());
        }


        public string KeyAppendBreathing
        {
            get => _keyAppendBreathing;
            set => SetProperty(ref _keyAppendBreathing, value?.ToUpper());
        }

        public string KeyAppendSoft
        {
            get => _keyAppendSoft;
            set => SetProperty(ref _keyAppendSoft, value?.ToUpper());
        }

        public string KeyAppendWhispered
        {
            get => _keyAppendWhispered;
            set => SetProperty(ref _keyAppendWhispered, value?.ToUpper());
        }
        public string KeyAppendConversational
        {
            get => _keyAppendConversational;
            set => SetProperty(ref _keyAppendConversational, value?.ToUpper());
        }

        public string KeyAppendNews
        {
            get => _keyAppendNews;
            set => SetProperty(ref _keyAppendNews, value?.ToUpper());
        }

        public string KeyAppendCustomerSupport
        {
            get => _keyAppendCustomerSupport;
            set => SetProperty(ref _keyAppendCustomerSupport, value?.ToUpper());
        }
        public string KeyAppendAssistant
        {
            get => _keyAppendAssistant;
            set => SetProperty(ref _keyAppendAssistant, value?.ToUpper());
        }
        public string KeyAppendDefault
        {
            get => _keyAppendDefault;
            set => SetProperty(ref _keyAppendDefault, value?.ToUpper());
        }

        public string KeyAppendHappy
        {
            get => _keyAppendHappy;
            set => SetProperty(ref _keyAppendHappy, value?.ToUpper());
        }
        public string KeyAppendEmphatic
        {
            get => _keyAppendEmphatic;
            set => SetProperty(ref _keyAppendEmphatic, value?.ToUpper());
        }

        public string KeyAppendClam
        {
            get => _keyAppendClam;
            set => SetProperty(ref _keyAppendClam, value?.ToUpper());
        }
        public string KeyAppendSad { get => keyAppendSad; set => SetProperty(ref keyAppendSad, value?.ToUpper()); }
        public string KeyAppendAngry { get => keyAppendAngry; set => SetProperty(ref keyAppendAngry, value?.ToUpper()); }
        public string KeyAppendExcited { get => keyAppendExcited; set => SetProperty(ref keyAppendExcited, value?.ToUpper()); }
        public string KeyAppendFriendly { get => keyAppendFriendly; set => SetProperty(ref keyAppendFriendly, value?.ToUpper()); }
        public string KeyAppendHopeful { get => keyAppendHopeful; set => SetProperty(ref keyAppendHopeful, value?.ToUpper()); }
        public string KeyAppendShouting { get => keyAppendShouting; set => SetProperty(ref keyAppendShouting, value?.ToUpper()); }
        public string KeyAppendTerrified { get => keyAppendTerrified; set => SetProperty(ref keyAppendTerrified, value?.ToUpper()); }
        public string KeyAppendUnFriendly { get => keyAppendUnFriendly; set => SetProperty(ref keyAppendUnFriendly, value?.ToUpper()); }

        public string KeyAppendPause
        {
            get => _keyAppendPause;
            set => SetProperty(ref _keyAppendPause, value?.ToUpper());
        }

        public string KeyAppendEmphasis
        {
            get => _keyAppendEmphasis;
            set => SetProperty(ref _keyAppendEmphasis, value?.ToUpper());
        }

        public string KeyAppendSpeed
        {
            get => _keyAppendSpeed;
            set => SetProperty(ref _keyAppendSpeed, value?.ToUpper());
        }

        public string KeyAppendPitch
        {
            get => _keyAppendPitch;
            set => SetProperty(ref _keyAppendPitch, value?.ToUpper());
        }

        public string KeyAppendVolume
        {
            get => _keyAppendVolume;
            set => SetProperty(ref _keyAppendVolume, value?.ToUpper());
        }

        public string KeyAppendSayAs
        {
            get => _keyAppendSayAs;
            set => SetProperty(ref _keyAppendSayAs, value?.ToUpper());
        }

        public string KeyAppRestart
        {
            get => _keyAppRestart;
            set => SetProperty(ref _keyAppRestart, value?.ToUpper());
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
            _profile = parameters.GetValue<Models.Profile>("Profile");
            _mapper.Map(_profile, this);
            Title = $"HotKeys Setup for Profile: {_profile.ProfileName}";
        }

        public HotKeysViewModel(IMapper mapper, IDialogCoordinator dialogCoordinator)
        {
            _mapper = mapper;
            _dialogCoordinator = dialogCoordinator;
        }

        public event Action<IDialogResult> RequestClose;

        private void DoClose()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        private async void DoSave()
        {
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
                KeyAppendPause,
                KeyAppendEmphasis,
                KeyAppendSpeed,
                KeyAppendPitch,
                KeyAppendVolume,
                KeyAppendSayAs,
                KeyAppRestart,

                KeyAppendSoft,
                KeyAppendBreathing,
                KeyAppendWhispered,
                KeyAppendConversational,
                KeyAppendNews,
                KeyAppendCustomerSupport,
                KeyAppendAssistant,
                KeyAppendHappy,
                KeyAppendEmphatic,
                KeyAppendClam,
                KeyAppendDefault,
                KeyAppendSad ,
                KeyAppendAngry ,
                KeyAppendExcited ,
                KeyAppendFriendly ,
                KeyAppendHopeful ,
                KeyAppendShouting ,
                KeyAppendTerrified ,
                KeyAppendUnFriendly
    }.Where(k => !string.IsNullOrWhiteSpace(k));

            var distinctCount = keys.Distinct().Count();

            if (keys.Count() > distinctCount)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", "Found duplicate hotkeys!");
                return;
            }

            using (var db = new Models.TTSContext())
            {
                db.Entry(_profile).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }
        }
    }
}