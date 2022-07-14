﻿using AudioBand.AudioSource;
using AudioBand.Commands;
using AudioBand.Messages;
using AudioBand.Models;
using AudioBand.Settings;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AudioBand.UI
{
    /// <summary>
    /// View model for the next button.
    /// </summary>
    public class NextButtonViewModel : ButtonViewModelBase<NextButton>
    {
        private readonly IAppSettings _appSettings;
        private readonly IAudioSession _audioSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextButtonViewModel"/> class.
        /// </summary>
        /// <param name="appSettings">The appSettings.</param>
        /// <param name="dialogService">The dialog service.</param>
        /// <param name="audioSession">The audio session.</param>
        /// <param name="messageBus">The message bus.</param>
        public NextButtonViewModel(IAppSettings appSettings, IDialogService dialogService, IAudioSession audioSession, IMessageBus messageBus)
            : base(appSettings.CurrentProfile.NextButton, dialogService, messageBus)
        {
            _appSettings = appSettings;
            _audioSession = audioSession;
            _appSettings.ProfileChanged += AppSettingsOnProfileChanged;

            InitializeButtonContents();
            NextTrackCommand = new AsyncRelayCommand<object>(NextTrackCommandOnExecute);
        }

        /// <summary>
        /// Gets or sets the button content.
        /// </summary>
        public ButtonContentViewModel Content { get; set; }

        /// <summary>
        /// Gets the next track command.
        /// </summary>
        public IAsyncCommand NextTrackCommand { get; }

        /// <inheritdoc />
        protected override void OnEndEdit()
        {
            base.OnEndEdit();
            MapSelf(Model, _appSettings.CurrentProfile.NextButton);
        }

        private async Task NextTrackCommandOnExecute(object arg)
        {
            if (_audioSession.CurrentAudioSource == null)
            {
                return;
            }

            await _audioSession.CurrentAudioSource.NextTrackAsync();
        }

        private void AppSettingsOnProfileChanged(object sender, EventArgs e)
        {
            Debug.Assert(IsEditing == false, "Should not be editing");
            MapSelf(_appSettings.CurrentProfile.NextButton, Model);

            InitializeButtonContents();
            RaisePropertyChangedAll();
        }

        private void InitializeButtonContents()
        {
            Content = new ButtonContentViewModel(Model.Content, new NextButton().Content, DialogService);
            TrackContentViewModel(Content);
        }
    }
}
