using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using MahApps.Metro.Controls.Dialogs;
using NAudio.Wave;
using NHotkey;
using NHotkey.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using VoiceTTS.Models;
using VoiceTTS.Properties;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace VoiceTTS.ViewModels
{
    public class TTSViewModel : BindableBase
    {
        private readonly ConcurrentQueue<VoiceMakerRequest> _sendRequests = new ConcurrentQueue<VoiceMakerRequest>();
        // private readonly ConcurrentQueue<string> _playRequests = new ConcurrentQueue<string>();


        private const string VOICEMAKER_KEY = "VOICEMAKER_KEY";
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DialogService _dialogService;
        private readonly Dispatcher ThisDispatcher;
        private readonly HotkeyManager _hkManager = HotkeyManager.Current;
        private readonly AutoMapper.IMapper _mapper;
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();

        private readonly List<VoiceInfo> _voiceInfos = new List<VoiceInfo>();

        private readonly VoiceMakerClient _voiceMaker;
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
        public int ActiveEffectCount { get => activeEffectCount; set => SetProperty(ref activeEffectCount, value); }

        public int MaxSpeed { get; set; } = 100;
        public int MinSpeed { get; set; } = -100;
        public int MaxVolume { get; set; } = 20;
        public int MinVolume { get; set; } = -20;
        public int MaxPitch { get; set; } = 100;
        public int MinPitch { get; set; } = -100;


        private DelegateCommand<string> _unEffectCommand;
        private int activeEffectCount;

        public DelegateCommand<string> UnEffectCommand
        {
            get { return _unEffectCommand = _unEffectCommand ?? new DelegateCommand<string>(RemoveEffect); }
        }


        private DelegateCommand _setupApiKeyCommand;
        private Thread _senderThread;

        public DelegateCommand SetupApiKeyCommand
        {
            get { return _setupApiKeyCommand = _setupApiKeyCommand ?? new DelegateCommand(SetupApiKey); }
        }

        private async void SetupApiKey()
        {
            var existing = Environment.GetEnvironmentVariable(VOICEMAKER_KEY, EnvironmentVariableTarget.User);
            var result = await _dialogCoordinator.ShowInputAsync(this, "Api Key", $"Enter VoiceMaker API Key:\nCurrent: {existing}");
            if (!string.IsNullOrWhiteSpace(result))
            {
                Environment.SetEnvironmentVariable(VOICEMAKER_KEY, result, EnvironmentVariableTarget.User);
            }
        }

        public TTSViewModel(AutoMapper.IMapper mapper, IDialogCoordinator dialogCoordinator, DialogService dialogService, VoiceMakerClient voiceMaker)
        {
            _voiceMaker = voiceMaker;
            _mapper = mapper;
            _dialogCoordinator = dialogCoordinator;
            _dialogService = dialogService;
            ThisDispatcher = Application.Current.Dispatcher;

            using (var db = new TTSContext())
            {
                _voiceInfos.AddRange(db.Voices.ToList());
            }

            Engines.AddRange(_voiceInfos.Select(v => v.Engine).Where(v => !string.IsNullOrWhiteSpace(v)).ToHashSet());
            Engines.Add("standard");
            Engine = "standard";

            Languages.AddRange(_voiceInfos.Select(v => v.Language).Where(v => !string.IsNullOrWhiteSpace(v))
                .ToHashSet());
            Language = Languages.FirstOrDefault();

            VoiceIds.AddRange(_voiceInfos.Select(v => v.VoiceId).OrderBy(v => v));
            VoiceId = VoiceIds.FirstOrDefault();

            Effect = Effects.FirstOrDefault();

            using (var db = new TTSContext())
            {
                if (!db.Profiles.Any())
                {
                    for (int i = 0; i < 10; i++)
                    {
                        db.Profiles.Add(new Profile()
                        {
                            ProfileName = $"Profile {i}",
                            // LanguageCode = "en-US",
                            VoiceId = "ai1-Ivy",
                            MasterSpeed = 0,
                            Engine = "neural",
                            MasterVolume = 0,
                            MasterPitch = 0,
                            Effect = "default",
                            AccentCode = "en-US"
                        });
                    }
                    db.SaveChanges();
                }

                Profiles.AddRange(db.Profiles.ToList());
            }

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

            _senderThread = new Thread(SenderThreadHandler);
            _senderThread.IsBackground = true;  
            _senderThread.Start();
            //_playerThread = new Thread(PlayerThreadHandler);
            //_playerThread.Start();

        }

        //private void PlayerThreadHandler()
        //{
        //    while (true)
        //    {
        //        if (_playRequests.TryDequeue(out var audioUrl))
        //        {
        //            PlayAudio(audioUrl);
        //        }
        //        else
        //        {
        //            Thread.Sleep(1);
        //        }
        //    }
        //}

        private void SenderThreadHandler()
        {
            while (true)
            {
                if (_sendRequests.TryDequeue(out var req))
                {
                    Debug.WriteLine($"Sending {req.Text}..");
                    SendAudioRequest(req);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void OnHotKeyAppRestart(object sender, HotkeyEventArgs e)
        {
            if (SaveProfileCommand.CanExecute())
            {
                SaveProfileCommand.Execute();
            }

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
            //AddEffect("VOLUME");
            ApplyVolume = !ApplyVolume;
        }

        private void OnHotKeyAppendSayAs(object sender, HotkeyEventArgs e)
        {
            //AddEffect("SAYAS");
            ApplySayAs = !ApplySayAs;
        }

        private void OnHotKeyAppendPitch(object sender, HotkeyEventArgs e)
        {
            //AddEffect("PITCH");
            ApplyPitch = !ApplyPitch;
        }

        private void OnHotKeyAppendPause(object sender, HotkeyEventArgs e)
        {
            //AddEffect("/*PAUSE*/");
        }


        private void OnHotKeyAppendEmphasis(object sender, HotkeyEventArgs e)
        {
            //AddEffect("EMPHASIS");
            ApplyEmphasis = !ApplyEmphasis;
        }

        private void OnHotKeyAppendSpeed(object sender, HotkeyEventArgs e)
        {
            //AddEffect("SPEED");
            ApplySpeed = !ApplySpeed;
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

        public DelegateCommand SendAudioCommand => _sendAudioCommand ??= new DelegateCommand(DoManualSend);

        private void DoManualSend()
        {
            EnqueueAudioRequest();
        }

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

                _timer?.Stop();
                _timer = null;

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
                Languages.Clear();
                VoiceIds.Clear();

                if (string.IsNullOrWhiteSpace(value))
                {
                    Languages.AddRange(_voiceInfos.Select(v => v.Language)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToHashSet());
                    VoiceIds.AddRange(_voiceInfos.Select(v => v.VoiceId));
                }
                else
                {
                    Languages.AddRange(_voiceInfos.Select(v => v.Language)
                        .Where(v => !string.IsNullOrWhiteSpace(v) && v.ToLower().Contains(value.ToLower()))
                        .ToHashSet());

                    VoiceIds.AddRange(_voiceInfos
                        .Where(v => v.Language.ToLower().Contains(value.ToLower())).Select(v => v.VoiceId));
                }

                VoiceId = VoiceIds.FirstOrDefault();
                Language = Languages.FirstOrDefault();
            }
        }

        public ObservableCollection<string> Engines { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Languages { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> VoiceIds { get; set; } = new ObservableCollection<string>();

        public string Language
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

        public string[] Effects { get; set; } = {
            "default",
            "breathing",
            "soft",
            "whispered",
            "conversational",
            "news",
            "customersupport",
            "assistant",
            "happy",
            "emphatic",
            "clam",
            "sad",
            "angry",
            "excited",
            "friendly",
            "hopeful",
            "shouting",
            "terrified",
            "unfriendly"
        };

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
            EnqueueAudioRequest();
        }

        private void OnHotKeyManualSend(object sender, HotkeyEventArgs e)
        {
            EnqueueAudioRequest();
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
            using (var db = new TTSContext())
            {
                _mapper.Map(this, Profile);

                db.Entry(Profile).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        private void RemoveEffect(string effect)
        {
            XmlNode node = null;
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml($"<ssml>{Body}</ssml>");
            switch (effect.ToUpper())
            {
                case "PAUSE":
                    var nodes = xmlDoc.SelectNodes("//break");
                    foreach (XmlNode node1 in nodes)
                    {
                        node1.ParentNode.RemoveChild(node1);
                    }

                    Body = xmlDoc.FirstChild.InnerXml;
                    break;
                case "EMPHASIS":
                    node = xmlDoc.SelectSingleNode("//emphasis");
                    Body = node.InnerXml;
                    break;

                case "SPEED":
                    node = xmlDoc.SelectSingleNode("//prosody[@rate]");
                    Body = node.InnerXml;
                    break;
                case "PITCH":
                    node = xmlDoc.SelectSingleNode("//prosody[@pitch]");
                    Body = node.InnerXml;
                    break;
                case "VOLUME":
                    node = xmlDoc.SelectSingleNode("//prosody[@volume]");
                    Body = node.InnerXml;
                    break;
                case "SAYAS":
                    node = xmlDoc.SelectSingleNode("//say-as");
                    Body = node.InnerXml;
                    break;
            }
        }


        private bool _applyEmphasis;

        public bool ApplyEmphasis
        {
            get { return _applyEmphasis; }
            set { SetProperty(ref _applyEmphasis, value); }
        }
        private bool _applySpeed;

        public bool ApplySpeed
        {
            get { return _applySpeed; }
            set { SetProperty(ref _applySpeed, value); }
        }

        private bool _applyPitch;

        public bool ApplyPitch
        {
            get { return _applyPitch; }
            set { _applyPitch = value; }
        }

        private bool _applyVolume;

        public bool ApplyVolume
        {
            get { return _applyVolume; }
            set { SetProperty(ref _applyVolume, value); }
        }


        private bool _applySayAs;
        private Thread _playerThread;

        public bool ApplySayAs
        {
            get { return _applySayAs; }
            set { SetProperty(ref _applySayAs, value); }
        }

        private void AddEffect(string effect)
        {

        }

        private void PlayAudio(string url)
        {
            using var mf = new MediaFoundationReader(url);
            using var wo = new WaveOutEvent();
            wo.DeviceNumber = OutputDevice.Item1;

            wo.Init(mf);
            wo.Play();
            while (wo.PlaybackState == PlaybackState.Playing) Thread.Sleep(1);
        }


        private async Task SendAudioRequest(VoiceMakerRequest req)
        {
            try
            {
                var voiceInfo = _voiceInfos.FirstOrDefault(v => v.VoiceId == VoiceId);
                if (voiceInfo == null)
                    return;

                var audioUrl = await _voiceMaker.GenerateAudio(req);
                PlayAudio(audioUrl);
            }
            catch (Exception e)
            {
                await ThisDispatcher.BeginInvoke(() => ErrorText += $"\n{DateTime.Now} {e.Message}");
            }
        }


        private void EnqueueAudioRequest()
        {
            if (!string.IsNullOrWhiteSpace(Body))
            {
                var voiceInfo = _voiceInfos.FirstOrDefault(v => v.VoiceId == VoiceId);
                if (voiceInfo != null)
                {
                    var text = _body;
                    if (ApplyEmphasis)
                    {
                        text = $"<emphasis level='{DefaultEmphasis}'>{text}</emphasis>";
                    }
                    if (ApplySpeed)
                    {
                        text = $"<prosody rate='{DefaultSpeed}'>{text}</prosody>";
                    }
                    if (ApplyPitch)
                    {
                        text = $"<prosody pitch='{DefaultPitch}'>{text}</prosody>";
                    }
                    if (ApplyVolume)
                    {
                        text = $"<prosody volume='{DefaultVolume}'>{text}</prosody>";
                    }
                    if (ApplySayAs)
                    {
                        text = $"<say-as interpret-as='{DefaultSayAs}'>{text}</say-as>";
                    }
                    ErrorText += $"\nRequest Text: {text}";

                    _sendRequests.Enqueue(new VoiceMakerRequest
                    {
                        Engine = Engine,
                        VoiceId = voiceInfo.VoiceId,
                        LanguageCode = voiceInfo.Language,
                        Text = text,
                        OutputFormat = "mp3",
                        SampleRate = "48000",
                        Effect = Effect,
                        MasterSpeed = MasterSpeed.ToString(),
                        MasterVolume = MasterVolume.ToString(),
                        MasterPitch = MasterPitch.ToString()
                    });
                }

                Body = "";
            }
        }


        private async void RenameProfile()
        {
            var result = await _dialogCoordinator.ShowInputAsync(this, "Rename Profile", "Enter Profile name:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                Profile.ProfileName = result;
                ProfileName = result;
                using (var db = new TTSContext())
                {
                    db.Entry(Profile).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public string ErrorText
        {
            get => _errorText;
            set => SetProperty(ref _errorText, value);
        }

        private void ActivateProfile()
        {
            var voice = _voiceInfos.FirstOrDefault(v => v.VoiceId == Profile.VoiceId);
            if (voice != null)
            {
                Language = voice.Language;
            }
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
                    using (var db = new TTSContext())
                    {
                        var updatedProfile = db.Profiles.Find(Id);
                        _mapper.Map(updatedProfile, this);
                        var exisitingProfile = Profiles.FirstOrDefault(p => p.Id == updatedProfile.Id);
                        if (exisitingProfile != null) _mapper.Map(updatedProfile, exisitingProfile);
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
            _timer?.Stop();
            _timer = null;

            AutoSending = state;

            if (AutoSending)
            {
                _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(AutoSendMillis), DispatcherPriority.Normal,
                    AutoSendHandler, ThisDispatcher);
            }
        }

        private Key ToKeyCode(string key)
        {
            var vk = (Key)Enum.Parse(typeof(Key), key);
            return vk;
        }

        private void InitHotkeys()
        {
            _hkManager.Remove(nameof(Profile.KeyAppendDefault));
            _hkManager.Remove(nameof(Profile.KeyAppendBreathing));
            _hkManager.Remove(nameof(Profile.KeyAppendSoft));
            _hkManager.Remove(nameof(Profile.KeyAppendWhispered));
            _hkManager.Remove(nameof(Profile.KeyAppendConversational));
            _hkManager.Remove(nameof(Profile.KeyAppendNews));
            _hkManager.Remove(nameof(Profile.KeyAppendCustomerSupport));
            _hkManager.Remove(nameof(Profile.KeyAppendAssistant));
            _hkManager.Remove(nameof(Profile.KeyAppendHappy));
            _hkManager.Remove(nameof(Profile.KeyAppendEmphatic));
            _hkManager.Remove(nameof(Profile.KeyAppendClam));
            _hkManager.Remove(nameof(Profile.KeyAppendSad));
            _hkManager.Remove(nameof(Profile.KeyAppendAngry));
            _hkManager.Remove(nameof(Profile.KeyAppendExcited));
            _hkManager.Remove(nameof(Profile.KeyAppendFriendly));
            _hkManager.Remove(nameof(Profile.KeyAppendHopeful));
            _hkManager.Remove(nameof(Profile.KeyAppendShouting));
            _hkManager.Remove(nameof(Profile.KeyAppendTerrified));
            _hkManager.Remove(nameof(Profile.KeyAppendUnFriendly));
            _hkManager.Remove(nameof(Profile.KeyManualSend));
            _hkManager.Remove(nameof(Profile.KeyVolumeUp));
            _hkManager.Remove(nameof(Profile.KeyVolumeDown));
            _hkManager.Remove(nameof(Profile.KeyPitchDown));
            _hkManager.Remove(nameof(Profile.KeyPitchUp));
            _hkManager.Remove(nameof(Profile.KeySpeedUp));
            _hkManager.Remove(nameof(Profile.KeySpeedDown));
            _hkManager.Remove(nameof(Profile.KeyAppendPause));
            _hkManager.Remove(nameof(Profile.KeyAppendEmphasis));
            _hkManager.Remove(nameof(Profile.KeyAppendSpeed));
            _hkManager.Remove(nameof(Profile.KeyAppendPitch));
            _hkManager.Remove(nameof(Profile.KeyAppendVolume));
            _hkManager.Remove(nameof(Profile.KeyAppendSayAs));
            _hkManager.Remove(nameof(Profile.KeyAppRestart));

            SetKeyForEffect(Profile.KeyAppendSad, nameof(Profile.KeyAppendSad));
            SetKeyForEffect(Profile.KeyAppendAngry, nameof(Profile.KeyAppendAngry));
            SetKeyForEffect(Profile.KeyAppendExcited, nameof(Profile.KeyAppendExcited));
            SetKeyForEffect(Profile.KeyAppendFriendly, nameof(Profile.KeyAppendFriendly));
            SetKeyForEffect(Profile.KeyAppendHopeful, nameof(Profile.KeyAppendHopeful));
            SetKeyForEffect(Profile.KeyAppendShouting, nameof(Profile.KeyAppendShouting));
            SetKeyForEffect(Profile.KeyAppendTerrified, nameof(Profile.KeyAppendTerrified));
            SetKeyForEffect(Profile.KeyAppendUnFriendly, nameof(Profile.KeyAppendUnFriendly));
            SetKeyForEffect(Profile.KeyAppendDefault, nameof(Profile.KeyAppendDefault));
            SetKeyForEffect(Profile.KeyAppendBreathing, nameof(Profile.KeyAppendBreathing));
            SetKeyForEffect(Profile.KeyAppendSoft, nameof(Profile.KeyAppendSoft));
            SetKeyForEffect(Profile.KeyAppendWhispered, nameof(Profile.KeyAppendWhispered));
            SetKeyForEffect(Profile.KeyAppendConversational, nameof(Profile.KeyAppendConversational));
            SetKeyForEffect(Profile.KeyAppendNews, nameof(Profile.KeyAppendNews));
            SetKeyForEffect(Profile.KeyAppendCustomerSupport, nameof(Profile.KeyAppendCustomerSupport));
            SetKeyForEffect(Profile.KeyAppendAssistant, nameof(Profile.KeyAppendAssistant));
            SetKeyForEffect(Profile.KeyAppendHappy, nameof(Profile.KeyAppendHappy));
            SetKeyForEffect(Profile.KeyAppendEmphatic, nameof(Profile.KeyAppendEmphatic));
            SetKeyForEffect(Profile.KeyAppendClam, nameof(Profile.KeyAppendClam));

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

        private void SetKeyForEffect(string keyValue, string keyName)
        {
            var keyLabel = keyName.ToLower().Replace("keyappend", "");
            if (!string.IsNullOrWhiteSpace(keyValue))
            {
                _hkManager.AddOrReplace(keyName, ToKeyCode(keyValue), ModifierKeys.Control | ModifierKeys.Shift, (s, e) => OnAppendEffect(keyLabel));
            }
        }

        private void OnAppendEffect(string v)
        {
            Effect = v.ToLower();
        }


        private void OnHotKeyAppendDefault(object sender, HotkeyEventArgs e)
        {
            Effect = "default";
        }

        private void OnHotKeyAppendKeyClam(object sender, HotkeyEventArgs e)
        {
            Effect = "clam";
        }

        private void OnHotKeyAppendKeyEmphatic(object sender, HotkeyEventArgs e)
        {
            Effect = "emphatic";
        }

        private void OnHotKeyAppendKeyHappy(object sender, HotkeyEventArgs e)
        {
            Effect = "happy";
        }

        private void OnHotKeyAppendKeyAssistant(object sender, HotkeyEventArgs e)
        {
            Effect = "assistant";
        }

        private void OnHotKeyAppendKeyCustomerSupport(object sender, HotkeyEventArgs e)
        {
            Effect = "customersupport";
        }

        private void OnHotKeyAppendKeyAppendNews(object sender, HotkeyEventArgs e)
        {
            Effect = "news";
        }

        private void OnHotKeyAppendKeyAppendConversational(object sender, HotkeyEventArgs e)
        {
            Effect = "conversational";
        }

        private void OnHotKeyAppendKeyAppendWhispered(object sender, HotkeyEventArgs e)
        {
            Effect = "whispered";
        }

        #region "Hotkey Handlers"
        private void OnHotKeyAppendSoft(object sender, HotkeyEventArgs e)
        {
            Effect = "soft";
        }

        private void OnHotKeyAppendBreathing(object sender, HotkeyEventArgs e)
        {
            Effect = "breathing";
        }

        #endregion
    }
}