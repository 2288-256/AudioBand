using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AudioBand.AudioSource;
using AudioBand.Commands;
using AudioBand.Messages;
using AudioBand.Models;
using AudioBand.Settings;

namespace AudioBand.UI
{
    /// <summary>
    /// View model for the volume button.
    /// </summary>
    public class VolumeButtonViewModel : ButtonViewModelBase<VolumeButton>
    {
        private readonly IAppSettings _appSettings;
        private readonly IAudioSession _audioSession;
        private bool _isVolumePopupOpen;
        private double _volume;

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeButtonViewModel"/> class.
        /// </summary>
        /// <param name="appSettings">App settings.</param>
        /// <param name="dialogService">The dialog service.</param>
        /// <param name="audioSession">The audio session.</param>
        /// <param name="messageBus">The message bus.</param>
        public VolumeButtonViewModel(IAppSettings appSettings, IDialogService dialogService, IAudioSession audioSession, IMessageBus messageBus)
            : base(appSettings.CurrentProfile.VolumeButton, dialogService, messageBus)
        {
            _appSettings = appSettings;
            _audioSession = audioSession;
            _audioSession.PropertyChanged += AudioSessionOnPropertyChanged;
            _appSettings.ProfileChanged += AppSettingsOnProfileChanged;

            VolumePopupCommand = new RelayCommand<object>(OpenVolumePopupCommandOnExecute);
            MouseLeftCommand = new RelayCommand<object>(MouseLeftCommandOnExecute);
            VolumeMouseScrollCommand = new RelayCommand<MouseWheelEventArgs>(VolumeMouseScrollCommandOnExecute);

            InitializeButtonContents();
        }

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        [TrackState]
        public Color VolumeBarForegroundColor
        {
            get => Model.VolumeBarForegroundColor;
            set => SetProperty(Model, nameof(Model.VolumeBarForegroundColor), value);
        }

        /// <summary>
        /// Gets or sets the volume bar background color.
        /// </summary>
        [TrackState]
        public Color VolumeBarBackgroundColor
        {
            get => Model.VolumeBarBackgroundColor;
            set => SetProperty(Model, nameof(Model.VolumeBarBackgroundColor), value);
        }

        /// <summary>
        /// Gets or sets the Popup's BackgroundColor.
        /// </summary>
        [TrackState]
        public Color PopupBackgroundColor
        {
            get => Model.PopupBackgroundColor;
            set => SetProperty(Model, nameof(Model.PopupBackgroundColor), value);
        }

        /// <summary>
        /// Gets or sets the Popup's width.
        /// </summary>
        [TrackState]
        public double PopupWidth
        {
            get => Model.PopupWidth;
            set => SetProperty(Model, nameof(Model.PopupWidth), value);
        }

        /// <summary>
        /// Gets or sets the Popup's height.
        /// </summary>
        [TrackState]
        public double PopupHeight
        {
            get => Model.PopupHeight;
            set => SetProperty(Model, nameof(Model.PopupHeight), value);
        }

        /// <summary>
        /// Gets or sets the Popup's X offset.
        /// </summary>
        [TrackState]
        public double XPopupOffset
        {
            get => Model.XPopupOffset;
            set => SetProperty(Model, nameof(Model.XPopupOffset), value);
        }

        /// <summary>
        /// Gets or sets the Popup's Y offset.
        /// </summary>
        [TrackState]
        public double YPopupOffset
        {
            get => Model.YPopupOffset;
            set => SetProperty(Model, nameof(Model.YPopupOffset), value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the popup is vertical or horizontal.
        /// </summary>
        [TrackState]
        public bool IsHorizontal
        {
            get => Model.IsHorizontal;
            set => SetProperty(Model, nameof(Model.IsHorizontal), value);
        }

        /// <summary>
        /// Gets or sets the view model for the button when there is no volume.
        /// </summary>
        public ButtonContentViewModel NoVolumeContent { get; set; }

        /// <summary>
        /// Gets or sets the view model for the button with low volume.
        /// </summary>
        public ButtonContentViewModel LowVolumeContent { get; set; }

        /// <summary>
        /// Gets or sets the view model for the button with mid volume.
        /// </summary>
        public ButtonContentViewModel MidVolumeContent { get; set; }

        /// <summary>
        /// Gets or sets the view model for the button with high volume.
        /// </summary>
        public ButtonContentViewModel HighVolumeContent { get; set; }

        /// <summary>
        /// Gets the VolumePopupCommand.
        /// </summary>
        public ICommand VolumePopupCommand { get; }

        /// <summary>
        /// Gets the MouseLeftCommand.
        /// </summary>
        public ICommand MouseLeftCommand { get; }

        /// <summary>
        /// Gets the Volume Mouse Scroll Command.
        /// </summary>
        public ICommand VolumeMouseScrollCommand { get; }

        /// <summary>
        /// Gets the current VolumeState.
        /// </summary>
        public VolumeState CurrentVolumeState
        {
            get
            {
                if (_volume == 0)
                {
                    return VolumeState.Off;
                }
                else if (_volume <= 33)
                {
                    return VolumeState.Low;
                }
                else if (_volume <= 66)
                {
                    return VolumeState.Mid;
                }
                else
                {
                    return VolumeState.High;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Volume Popup is open.
        /// </summary>
        public bool IsVolumePopupOpen
        {
            get => _isVolumePopupOpen;
            private set => SetProperty(ref _isVolumePopupOpen, value);
        }

        /// <summary>
        /// Gets or sets the current Volume.
        /// </summary>
        [AlsoNotify(nameof(CurrentVolumeState))]
        public double Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, value))
                {
                    _audioSession.CurrentAudioSource?.SetVolumeAsync((int)value);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnEndEdit()
        {
            base.OnEndEdit();
            MapSelf(Model, _appSettings.CurrentProfile.VolumeButton);
        }

        private void AppSettingsOnProfileChanged(object sender, EventArgs e)
        {
            Debug.Assert(IsEditing == false, "Should not be editing");
            MapSelf(_appSettings.CurrentProfile.VolumeButton, Model);

            InitializeButtonContents();
            RaisePropertyChangedAll();
        }

        private void AudioSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IAudioSession.Volume))
            {
                return;
            }

            OnVolumeChanged(_audioSession.Volume);
        }

        private void OnVolumeChanged(int newVolume)
        {
            Volume = newVolume;
        }

        private void OpenVolumePopupCommandOnExecute(object arg)
        {
            IsVolumePopupOpen = !IsVolumePopupOpen;
        }

        private void MouseLeftCommandOnExecute(object obj)
        {
            IsVolumePopupOpen = false;
        }

        private void VolumeMouseScrollCommandOnExecute(MouseWheelEventArgs args)
        {
            Volume += args.Delta / 75;
        }

        private void InitializeButtonContents()
        {
            var resetBase = new VolumeButton();
            NoVolumeContent = new ButtonContentViewModel(Model.NoVolumeContent, resetBase.NoVolumeContent, DialogService);
            LowVolumeContent = new ButtonContentViewModel(Model.LowVolumeContent, resetBase.LowVolumeContent, DialogService);
            MidVolumeContent = new ButtonContentViewModel(Model.MidVolumeContent, resetBase.MidVolumeContent, DialogService);
            HighVolumeContent = new ButtonContentViewModel(Model.HighVolumeContent, resetBase.HighVolumeContent, DialogService);

            TrackContentViewModel(NoVolumeContent);
            TrackContentViewModel(LowVolumeContent);
            TrackContentViewModel(MidVolumeContent);
            TrackContentViewModel(HighVolumeContent);
        }
    }
}
