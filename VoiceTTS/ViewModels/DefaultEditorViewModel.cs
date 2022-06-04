using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using VoiceTTS.Models;
using Profile = VoiceTTS.Models.Profile;

namespace VoiceTTS.ViewModels
{
    public class DefaultEditorViewModel : BindableBase, IDialogAware
    {
        private readonly IMapper _mapper;
        private DelegateCommand _cancelCommand;
        private string _defaultEffect;

        private string _defaultEmphasis;


        private DelegateCommand _okCommand;
        private string _defaultPause;

        private string _defaultPitch;

        private string _defaultSayAs;

        private string _defaultSpeed;

        private string _defaultVolume;
        private int ProfileId;
        public Profile Profile { get; set; }

        public string DefaultEffect
        {
            get => _defaultEffect;
            set => SetProperty(ref _defaultEffect, value);
        }

        public List<string> Effects { get; set; } = new List<string>
        {
            "breathing",
            "soft",
            "whispered",
            "conversational",
            "news",
            "customersupport",
            "assistant",
            "happy",
            "empathic",
            "clam"
        };

        public string DefaultPause
        {
            get => _defaultPause;
            set => SetProperty(ref _defaultPause, value);
        }

        public List<string> Pauses { get; set; } = new List<string>
        {
            "500ms",
            "1s",
            "2s",
            "3s",
            "4s",
            "5s",
            "6s"
        };

        public string DefaultEmphasis
        {
            get => _defaultEmphasis;
            set => SetProperty(ref _defaultEmphasis, value);
        }


        public List<string> Emphases { get; set; } = new List<string>
        {
            "strong",
            "moderate",
            "reduced"
        };

        public string DefaultSpeed
        {
            get => _defaultSpeed;
            set => SetProperty(ref _defaultSpeed, value);
        }

        public List<string> Speeds { get; set; } = new List<string>
        {
            "x-slow",
            "slow",
            "medium",
            "fast",
            "x-fast"
        };

        public string DefaultPitch
        {
            get => _defaultPitch;
            set => SetProperty(ref _defaultPitch, value);
        }

        public List<string> Pitches { get; set; } = new List<string>
        {
            "x-low",
            "low",
            "medium",
            "default",
            "high",
            "x-high"
        };

        public string DefaultVolume
        {
            get => _defaultVolume;
            set => SetProperty(ref _defaultVolume, value);
        }

        public List<string> Volumes { get; set; } = new List<string>
        {
            "x-soft",
            "soft",
            "medium",
            "loud",
            "x-loud"
        };

        public string DefaultSayAs
        {
            get => _defaultSayAs;
            set => SetProperty(ref _defaultSayAs, value);
        }

        public List<string> Says { get; set; } = new List<string>
        {
            "address",
            "telephone",
            "spell-out",
            "cardinal",
            "ordinal",
            "characters",
            "digits",
            "fraction"
        };

        public DelegateCommand OkCommand => _okCommand ??= new DelegateCommand(DoOk);
        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(DoCancel);

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Id"))
            {
                ProfileId = parameters.GetValue<int>("Id");
                using (var db = new TTSContext())
                {
                    Profile = db.Profiles.Find(ProfileId);
                    _mapper.Map(Profile, this);
                }
            }
        }

        public string Title { get; }
        public event Action<IDialogResult> RequestClose;

        private void DoCancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public DefaultEditorViewModel(IMapper mapper)
        {
            _mapper = mapper;
        }
        private void DoOk()
        {
            using (var db = new TTSContext())
            {
                _mapper.Map(this, Profile);
                db.Entry(Profile).State = EntityState.Modified;
                db.SaveChanges();
            }
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, null));
        }
    }
}