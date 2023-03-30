﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AutoMapper;
using LiteDB;
using MahApps.Metro.Controls.Dialogs;
using NAudio.Wave;
using NHotkey;
using NHotkey.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using VoiceTTS.Model;
using VoiceTTS.Properties;
using Profile = VoiceTTS.Model.Profile;
using Task = System.Threading.Tasks.Task;

namespace VoiceTTS.ViewModels
{
    public class TTSViewModel : BindableBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DialogService _dialogService;
        private readonly LiteDatabase _db;
        private readonly HotkeyManager _hkManager = HotkeyManager.Current;
        private readonly IMapper _mapper;
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();

        private readonly List<VoiceMaker.VoiceInfo> _voiceInfos = new List<VoiceMaker.VoiceInfo>();

        private readonly VoiceMaker _voiceMaker = new VoiceMaker();
        private DelegateCommand _activateProfileCommand;
        private bool _autoSending = false;

        private int _autoSendMillis;
        private string _body;
        private string _defaultEffect = "breathing";
        private string _defaultEmphasis = "strong";
        private string _defaultPause = "5s";
        private string _defaultPitch = "high";
        private string _defaultSayAs = "spell-out";
        private string _defaultSpeed = "slow";
        private string _defaultVolume = "loud";
        private string _effect;
        private DelegateCommand<string> _effectCommand;
        private string _engine = "standard";
        private string _languageCode;
        private string _languageCodeFilter;
        private int _masterPitch;
        private int _masterSpeed;
        private int _masterVolume;
        private DelegateCommand _offAutoSendCommand;
        private DelegateCommand _onAutoSendCommand;
        private Tuple<int, string> _outputDevice;
        private Profile _profile = new Profile();
        private string _profileName;
        private DelegateCommand _renameProfileCommand;
        private DelegateCommand _saveProfileCommand;

        private DelegateCommand _sendAudioCommand;
        private DelegateCommand _setupDefaultsCommand;

        private DispatcherTimer _timer;
        private string _voiceId;
        private DelegateCommand _setupHotKeysCommand;
        private string _errorText;
        private readonly Dispatcher _dispatcher;

        public int MaxSpeed { get; set; } = 100;
        public int MinSpeed { get; set; } = -100;
        public int MaxVolume { get; set; } = 20;
        public int MinVolume { get; set; } = -20;
        public int MaxPitch { get; set; } = 100;
        public int MinPitch { get; set; } = -100;

        public TTSViewModel(IMapper mapper, IDialogCoordinator dialogCoordinator, DialogService dialogService, LiteDatabase db)
        {
            _mapper = mapper;
            _dialogCoordinator = dialogCoordinator;
            _dialogService = dialogService;
            _db = db;
            _dispatcher = Application.Current.Dispatcher;

            var getVoicesTask = VoiceMaker.GetVoicesAsync().ContinueWith(task =>
            {

                _dispatcher.BeginInvoke(new Action(() =>
                {
                    _voiceInfos.AddRange(task.Result);
                    Engines.AddRange(_voiceInfos.Select(v => v.Engine).Where(v => !string.IsNullOrWhiteSpace(v)).ToHashSet());
                    Engines.Add("standard");
                    Engine = "standard";
                    LanguageCodes.AddRange(_voiceInfos.Select(v => v.Language).Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToHashSet());
                    LanguageCode = LanguageCodes.FirstOrDefault();
                    VoiceIds.AddRange(_voiceInfos.Select(v => v.VoiceId));
                    VoiceId = VoiceIds.FirstOrDefault();
                    Effect = Effects.FirstOrDefault();
                }));
            });





            //using (var db = new TTSContext())
            //{

            //    Profiles.AddRange(db.Profiles.ToList());
            //}

            var profilesCollection = _db.GetCollection<Profile>();
            Profiles.AddRange(profilesCollection.FindAll());

            if (Profiles.Any())
            {
                Profile = Profiles.FirstOrDefault();
                ActivateProfile();
            }

            for (var i = -1; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                Devices.Add(new Tuple<int, string>(i, capabilities.ProductName));
            }

            if (!string.IsNullOrWhiteSpace(Settings.Default.OutputDevice))
            {
                var device = Devices.FirstOrDefault(d => d.Item2 == Settings.Default.OutputDevice);
                if (device != null) OutputDevice = device;
            }

            AutoSendMillis = Settings.Default.AutoSendMillis;
            PropertyChanged += TTSViewModel_PropertyChanged;
        }

        private void InitHotkeys()
        {
            _hkManager.Remove(nameof(Profile.KeyManualSend));
            _hkManager.Remove(nameof(Profile.KeyVolumeUp));
            _hkManager.Remove(nameof(Profile.KeyVolumeDown));
            _hkManager.Remove(nameof(Profile.KeyPitchDown));
            _hkManager.Remove(nameof(Profile.KeyPitchUp));
            _hkManager.Remove(nameof(Profile.KeySpeedUp));
            _hkManager.Remove(nameof(Profile.KeySpeedDown));
            _hkManager.Remove(nameof(Profile.KeyAppendVoice));
            _hkManager.Remove(nameof(Profile.KeyAppendPause));
            _hkManager.Remove(nameof(Profile.KeyAppendEmphasis));
            _hkManager.Remove(nameof(Profile.KeyAppendSpeed));
            _hkManager.Remove(nameof(Profile.KeyAppendPitch));
            _hkManager.Remove(nameof(Profile.KeyAppendVolume));
            _hkManager.Remove(nameof(Profile.KeyAppendSayAs));
            _hkManager.Remove(nameof(Profile.KeyAppRestart));

            if (!string.IsNullOrWhiteSpace(Profile.KeyManualSend))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyManualSend), ToKeyCode(Profile.KeyManualSend), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyManualSend);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyAppendSpeed))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyAppendSpeed), ToKeyCode(Profile.KeyAppendSpeed), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyAppendSpeed);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyAppendEmphasis))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyAppendEmphasis), ToKeyCode(Profile.KeyAppendEmphasis), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyAppendEmphasis);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyPitchUp))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyPitchUp), ToKeyCode(Profile.KeyPitchUp), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyPitchUp);

            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyAppendVolume))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyAppendVolume), ToKeyCode(Profile.KeyAppendVolume), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyAppendVolume);

            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyAppendVoice))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyAppendVoice), ToKeyCode(Profile.KeyAppendVoice), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyAppendVoice);

            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyAppendSayAs))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyAppendSayAs), ToKeyCode(Profile.KeyAppendSayAs), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyAppendSayAs);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyAppendPitch))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyAppendPitch), ToKeyCode(Profile.KeyAppendPitch), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyAppendPitch);

            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyAppendPause))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyAppendPause), ToKeyCode(Profile.KeyAppendPause), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyAppendPause);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyPitchDown))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyPitchDown), ToKeyCode(Profile.KeyPitchDown), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyPitchDown);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeySpeedDown))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeySpeedDown), ToKeyCode(Profile.KeySpeedDown), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeySpeedDown);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeySpeedUp))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeySpeedUp), ToKeyCode(Profile.KeySpeedUp), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeySpeedUp);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyVolumeDown))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyVolumeDown), ToKeyCode(Profile.KeyVolumeDown), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyVolumeDown);
            }

            if (!string.IsNullOrWhiteSpace(Profile.KeyVolumeUp))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyVolumeUp), ToKeyCode(Profile.KeyVolumeUp), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyVolumeUp);
            }
            if (!string.IsNullOrWhiteSpace(Profile.KeyAppRestart))
            {
                _hkManager.AddOrReplace(nameof(Profile.KeyAppRestart), ToKeyCode(Profile.KeyAppRestart), ModifierKeys.Control | ModifierKeys.Shift, OnHotKeyAppRestart);
            }
        }

        private void OnHotKeyAppRestart(object sender, HotkeyEventArgs e)
        {
            if (SaveProfileCommand.CanExecute())
            {
                SaveProfileCommand.Execute();
            }

            //var programPath = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VoiceTTS.exe")}";

            //var processStartInfo = new ProcessStartInfo
            //{
            //    //Arguments =
            //    //    "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetEntryAssembly().Location + "\"",
            //    WindowStyle = ProcessWindowStyle.Normal,
            //    UseShellExecute = true,
            //    //CreateNoWindow = true,
            //    WorkingDirectory = Path.GetDirectoryName(programPath),
            //    FileName = programPath
            //};
            // Process.Start(processStartInfo);

            var programPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "VoiceTTS.appref-ms");
            Process.Start(programPath);
            Application.Current.Shutdown();
        }

        private void OnHotKeyVolumeUp(object sender, HotkeyEventArgs e)
        {
            MasterVolume += 1;
        }

        private void OnHotKeyVolumeDown(object sender, HotkeyEventArgs e)
        {
            MasterVolume -= 1;
        }

        private void OnHotKeySpeedUp(object sender, HotkeyEventArgs e)
        {
            MasterSpeed += 1;
        }

        private void OnHotKeySpeedDown(object sender, HotkeyEventArgs e)
        {
            MasterSpeed -= 1;
        }

        private void OnHotKeyPitchDown(object sender, HotkeyEventArgs e)
        {
            MasterPitch -= 1;
        }

        private void OnHotKeyPitchUp(object sender, HotkeyEventArgs e)
        {
            MasterPitch += 1;
        }

        private void OnHotKeyAppendVolume(object sender, HotkeyEventArgs e)
        {
            AddEffect("VOLUME");
        }

        private void OnHotKeyAppendVoice(object sender, HotkeyEventArgs e)
        {
            AddEffect("EFFECT");
        }

        private void OnHotKeyAppendSayAs(object sender, HotkeyEventArgs e)
        {
            AddEffect("SAYAS");
        }

        private void OnHotKeyAppendPitch(object sender, HotkeyEventArgs e)
        {
            AddEffect("PITCH");
        }

        private void OnHotKeyAppendPause(object sender, HotkeyEventArgs e)
        {
            AddEffect("PAUSE");
        }


        private void OnHotKeyAppendEmphasis(object sender, HotkeyEventArgs e)
        {
            AddEffect("EMPHASIS");
        }

        private void OnHotKeyAppendSpeed(object sender, HotkeyEventArgs e)
        {
            AddEffect("SPEED");
        }

        private Key ToKeyCode(string key)
        {
            var vk = (Key)Enum.Parse(typeof(Key), key);
            return vk;
        }

        public int Id { get; set; }

        public string ProfileName
        {
            get => _profileName;
            set => SetProperty(ref _profileName, value);
        }

        public ObservableCollection<Profile> Profiles { get; set; } = new ObservableCollection<Profile>();

        public DelegateCommand<string> EffectCommand =>
            _effectCommand ??= new DelegateCommand<string>(AddEffect);

        public Profile Profile
        {
            get => _profile;
            set => SetProperty(ref _profile, value);
        }

        public ObservableCollection<Tuple<int, string>> Devices { get; set; }
            = new ObservableCollection<Tuple<int, string>>();

        public DelegateCommand SendAudioCommand => _sendAudioCommand ??= new DelegateCommand(DoSendAudio);

        public string Engine
        {
            get => _engine;
            set => SetProperty(ref _engine, value);
        }

        public int AutoSendMillis
        {
            get => _autoSendMillis;
            set
            {
                SetProperty(ref _autoSendMillis, value);
            }
        }

        public string Body
        {
            get => _body;
            set
            {
                SetProperty(ref _body, value);
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }

                if (AutoSending)
                    _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(AutoSendMillis), DispatcherPriority.Input,
                        AutoSendHandler, Dispatcher.CurrentDispatcher);
            }
        }

        public string LanguageCodeFilter
        {
            get => _languageCodeFilter;
            set
            {
                SetProperty(ref _languageCodeFilter, value);
                LanguageCodes.Clear();
                VoiceIds.Clear();

                if (string.IsNullOrWhiteSpace(value))
                {
                    LanguageCodes.AddRange(_voiceInfos.Select(v => v.Language)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToHashSet());
                    VoiceIds.AddRange(_voiceInfos.Select(v => v.VoiceId));
                }
                else
                {
                    LanguageCodes.AddRange(_voiceInfos.Select(v => v.Language)
                        .Where(v => !string.IsNullOrWhiteSpace(v) && v.ToLower().Contains(value.ToLower()))
                        .ToHashSet());

                    VoiceIds.AddRange(_voiceInfos
                        .Where(v => v.Language.ToLower().Contains(value.ToLower())).Select(v => v.VoiceId));
                }

                VoiceId = VoiceIds.FirstOrDefault();
                LanguageCode = LanguageCodes.FirstOrDefault();
            }
        }

        public ObservableCollection<string> Engines { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> LanguageCodes { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> VoiceIds { get; set; } = new ObservableCollection<string>();

        public string LanguageCode
        {
            get => _languageCode;
            set
            {
                SetProperty(ref _languageCode, value);

                if (string.IsNullOrWhiteSpace(value))
                    return;
                VoiceIds.Clear();
                VoiceIds.AddRange(_voiceInfos.Where(v => v.Language.ToLower() == value.ToLower())
                    .Select(v => v.VoiceId));
                VoiceId = VoiceIds.FirstOrDefault();
            }
        }

        public string VoiceId
        {
            get => _voiceId;
            set => SetProperty(ref _voiceId, value);
        }

        public string[] Effects { get; set; } = { "default", "breathing", "soft", "whispered", "conversational", "news" };

        public string Effect
        {
            get => _effect;
            set => SetProperty(ref _effect, value);
        }

        public int MasterSpeed
        {
            get => _masterSpeed;
            set
            {
                if (value > MaxSpeed)
                {
                    value = MaxSpeed;
                }
                if (value < MinSpeed)
                {
                    value = MinSpeed;
                }
                SetProperty(ref _masterSpeed, value);
            }
        }

        public int MasterVolume
        {
            get => _masterVolume;
            set
            {
                if (value > MaxVolume)
                {
                    value = MaxVolume;
                }
                if (value < MinVolume)
                {
                    value = MinVolume;
                }
                SetProperty(ref _masterVolume, value);
            }
        }

        public int MasterPitch
        {
            get => _masterPitch;
            set
            {
                if (value > MaxPitch)
                {
                    value = MaxPitch;
                }
                if (value < MinPitch)
                {
                    value = MinPitch;
                }
                SetProperty(ref _masterPitch, value);
            }
        }

        public DelegateCommand SaveProfileCommand => _saveProfileCommand ??= new DelegateCommand(SaveProfile);

        public DelegateCommand RenameProfileCommand => _renameProfileCommand ??=
            new DelegateCommand(RenameProfile, () => Profile != null).ObservesProperty(() => Profile);

        public DelegateCommand ActivateProfileCommand =>
            _activateProfileCommand ??= new DelegateCommand(ActivateProfile);

        public string DefaultEffect
        {
            get => _defaultEffect;
            set => SetProperty(ref _defaultEffect, value);
        }

        public string DefaultPause
        {
            get => _defaultPause;
            set => SetProperty(ref _defaultPause, value);
        }

        public string DefaultEmphasis
        {
            get => _defaultEmphasis;
            set => SetProperty(ref _defaultEmphasis, value);
        }

        public string DefaultSpeed
        {
            get => _defaultSpeed;
            set => SetProperty(ref _defaultSpeed, value);
        }

        public string DefaultPitch
        {
            get => _defaultPitch;
            set => SetProperty(ref _defaultPitch, value);
        }

        public string DefaultVolume
        {
            get => _defaultVolume;
            set => SetProperty(ref _defaultVolume, value);
        }

        public string DefaultSayAs
        {
            get => _defaultSayAs;
            set => SetProperty(ref _defaultSayAs, value);
        }

        public Tuple<int, string> OutputDevice
        {
            get => _outputDevice;
            set => SetProperty(ref _outputDevice, value);
        }

        public DelegateCommand SetupDefaultsCommand => _setupDefaultsCommand ??=
            new DelegateCommand(SetupDefaults, () => Profile != null)
                .ObservesProperty(() => Profiles);

        public DelegateCommand OnAutoSendCommand => _onAutoSendCommand ??= new DelegateCommand(() => SetAutoSend(true));

        public DelegateCommand OffAutoSendCommand =>
            _offAutoSendCommand ??= new DelegateCommand(() => SetAutoSend(false));

        public bool AutoSending
        {
            get => _autoSending;
            set => SetProperty(ref _autoSending, value);
        }

        private void AutoSendHandler(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Body))
                return;

            DoSendAudio();
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            if (AutoSending)
                _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(AutoSendMillis), DispatcherPriority.Input,
                    AutoSendHandler, Dispatcher.CurrentDispatcher);
        }

        private void OnHotKeyManualSend(object sender, HotkeyEventArgs e)
        {
            DoSendAudio();
        }

        private void TTSViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (nameof(Profile) == e.PropertyName)
                if (Profile != null)
                    ActivateProfile();

            if (nameof(OutputDevice) == e.PropertyName)
            {
                Settings.Default.OutputDevice = OutputDevice.Item2;
                Settings.Default.Save();
            }

            if (nameof(AutoSending) == e.PropertyName)
            {
                SetAutoSend(AutoSending);
            }

            if (nameof(AutoSendMillis) == e.PropertyName)
            {
                Settings.Default.AutoSendMillis = AutoSendMillis;
                Settings.Default.Save();
                SetAutoSend(AutoSending);
            }
        }

        private void SaveProfile()
        {
            var profileCollection = _db.GetCollection<Profile>();
            _mapper.Map(this, Profile);
            profileCollection.Update(Profile);
        }

        private void AddEffect(string effect)
        {
            switch (effect.ToUpper())
            {
                case "EFFECT":
                    Body += $"<{DefaultEffect}></{DefaultEffect}>";
                    break;
                case "PAUSE":
                    Body += $"<break time='{DefaultPause}'/>";
                    break;
                case "EMPHASIS":
                    Body += $"<emphasis level='{DefaultEmphasis}'></emphasis>";
                    break;
                case "SPEED":
                    Body += $"<prosody rate='{DefaultSpeed}'></prosody>";
                    break;
                case "PITCH":
                    Body += $"<prosody pitch='{DefaultPitch}'></prosody>";
                    break;
                case "VOLUME":
                    Body += $"<prosody volume='{DefaultVolume}'></prosody>";
                    break;
                case "SAYAS":
                    Body += $"<say-as interpret-as='{DefaultSayAs}'></say-as>";
                    break;
            }
        }

        private void PlayAudio(string url)
        {
            using var mf = new MediaFoundationReader(url);
            using var wo = new WaveOutEvent();
            wo.DeviceNumber = OutputDevice.Item1;

            wo.Init(mf);
            wo.Play();
            while (wo.PlaybackState == PlaybackState.Playing) Thread.Sleep(1000);
        }

        private async void DoSendAudio()
        {
            try
            {
                await Task.Run(async () =>
                {
                    var req = new VoiceMakerRequest
                    {
                        Engine = Engine,
                        VoiceId = VoiceId,
                        LanguageCode = LanguageCode,
                        Text = Body,
                        OutputFormat = "mp3",
                        SampleRate = "48000",
                        Effect = Effect,
                        MasterSpeed = MasterSpeed.ToString(),
                        MasterVolume = MasterVolume.ToString(),
                        MasterPitch = MasterPitch.ToString()
                    };

                    Body = "";

                    var audioUrl = await _voiceMaker.GenerateAudioAsync(req);
                    PlayAudio(audioUrl);
                });
            }
            catch (Exception e)
            {
                ErrorText += $"\n{DateTime.Now} {e.Message}";
            }
        }


        private async void RenameProfile()
        {
            var result = await _dialogCoordinator.ShowInputAsync(this, "Rename Profile", "Enter Profile name:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                Profile.ProfileName = result;
                ProfileName = result;


                var profileCollection = _db.GetCollection<Profile>();
                profileCollection.Update(Profile);
            }
        }

        public string ErrorText
        {
            get => _errorText;
            set => SetProperty(ref _errorText, value);
        }

        private void ActivateProfile()
        {
            _mapper.Map(Profile, this);

            try
            {
                InitHotkeys();
            }
            catch (Exception e)
            {
                ErrorText += $"\n{DateTime.Now} {e.Message}";
            }
        }

        private void SetupDefaults()
        {
            var parameters = new DialogParameters();
            parameters.Add("Id", Id);
            _dialogService.ShowDialog("DefaultEditor", parameters, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var profileCollection = _db.GetCollection<Profile>();
                    var updatedProfile = profileCollection.FindOne(p => p.Id == Id);
                    _mapper.Map(updatedProfile, this);
                    var existingProfile = Profiles.FirstOrDefault(p => p.Id == updatedProfile.Id);
                    if (existingProfile != null) _mapper.Map(updatedProfile, existingProfile);
                }
            }, "dialogWindow");
        }

        public DelegateCommand SetupHotKeysCommand => _setupHotKeysCommand ??= new DelegateCommand(SetupHotkeys, () => Profile != null).ObservesProperty(() => Profile);

        private void SetupHotkeys()
        {
            var dialogParam = new DialogParameters();
            dialogParam.Add("Profile", Profile);
            _dialogService.ShowDialog("HotKeys", dialogParam, result =>
            {
                ActivateProfile();
            }, "dialogWindow");
        }

        private void SetAutoSend(bool state)
        {

            if (_timer != null)
            {
                _timer.IsEnabled = false;
                _timer.Stop();
                _timer = null;
            }

            if (state)
            {
                _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(AutoSendMillis), DispatcherPriority.Normal,
                    AutoSendHandler, Dispatcher.CurrentDispatcher);
            }
        }
    }
}