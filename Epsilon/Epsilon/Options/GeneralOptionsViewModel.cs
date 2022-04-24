using EpsilonLib.Options;
using EpsilonLib.Settings;
using System.IO;
using System.ComponentModel.Composition;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Epsilon.Options
{
    enum Theme
    {
        Default,
        Solid,
        Frosted,
        Transparent,
        Steam,
        HotWheels,
        Dark1,
        Dark2,
        Dark3
    }

    enum Accent
    {
        Lochmara,
        Steel,
        Silver,
        White,
        Red,
        Mauve,
        Salmon,
        Orange,
        Coral,
        Peach,
        Gold,
        Yellow,
        Pale,
        Sage,
        Green,
        Olive,
        Teal,
        Aqua,
        Cyan,
        Blue,
        Cobalt,
        Sapphire,
        Violet,
        Crimson,
        Orchid,
        Lavender,
        Rubine,
        Pink,
        Brown,
        Tan,
        Khaki
    }

    [Export(typeof(IOptionsPage))]
    class GeneralOptionsViewModel : OptionPageBase
    {
        public readonly ISettingsCollection _settings;
        private Theme _theme;
        private Accent _accentColor;
        private string _defaultCachePath;
        private bool _defaultCachePathIsValid;
        private bool _alwaysOnTop;

        [ImportingConstructor]
        public GeneralOptionsViewModel(ISettingsService settingsService) : base("General", "General")
        {
            _settings = settingsService.GetCollection(GeneralSettings.CollectionKey);
        }

        public Theme EpsilonTheme
        {
            get => _theme;
            set
            {
                SetOptionAndNotify(ref _theme, value, "EpsilonTheme");
            }
        }

        public Accent AccentColor
        {
            get => _accentColor;
            set
            {
                SetOptionAndNotify(ref _accentColor, value, "AccentColor");
                //OnPropertyChanged("Accent");
                //UpdateAppearance(@"AccentColors/" + AccentColor.ToString());
            }
        }

        public string DefaultCachePath
        {
            get => _defaultCachePath;
            set => SetOptionAndNotify(ref _defaultCachePath, value);
        }

        public bool PathIsValid
        {
            get
            {
                _defaultCachePathIsValid = File.Exists(@_defaultCachePath) && @_defaultCachePath.EndsWith(".dat");
                return _defaultCachePathIsValid;
            }
            set => SetOptionAndNotify(ref _defaultCachePathIsValid, value);
        }

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetOptionAndNotify(ref _alwaysOnTop, value);
        }

        public override void Apply()
        {
            _settings.Set<Theme>(GeneralSettings.ThemeSetting.Key, EpsilonTheme);
            _settings.Set<Accent>(GeneralSettings.AccentColorSetting.Key, AccentColor);
            if (PathIsValid)
                _settings.Set(GeneralSettings.DefaultTagCacheSetting.Key, DefaultCachePath);
            _settings.Set(GeneralSettings.AlwaysOnTopSetting.Key, AlwaysOnTop);
        }

        public override void Load()
        {
            EpsilonTheme = _settings.Get(GeneralSettings.ThemeSetting.Key, Theme.Default);
            AccentColor = _settings.Get(GeneralSettings.AccentColorSetting.Key, Accent.Lochmara);
            DefaultCachePath = _settings.Get(GeneralSettings.DefaultTagCacheSetting.Key, "");
            AlwaysOnTop = _settings.Get(GeneralSettings.AlwaysOnTopSetting.Key, false);
        }
    }
}
