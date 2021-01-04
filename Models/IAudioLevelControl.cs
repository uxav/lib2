namespace UX.Lib2.Models
{
    /// <summary>
    /// An interface for controlling audio levels on devices
    /// </summary>
    public interface IAudioLevelControl
    {
        /// <summary>
        /// Get the control Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the audio level control type. Default is NotDefined.
        /// </summary>
        AudioLevelType ControlType { get; }

        /// <summary>
        /// True if this control supports Level control
        /// </summary>
        bool SupportsLevel { get; }

        /// <summary>
        /// Set or Get the Audio Level / Volume Control
        /// </summary>
        ushort Level { get; set; }

        /// <summary>
        /// Get the Audio / Volume level as a string description
        /// </summary>
        string LevelString { get; }

        /// <summary>
        /// True if this control supports Mute control
        /// </summary>
        bool SupportsMute { get; }

        /// <summary>
        /// Set of Get the Audio Mute status
        /// </summary>
        bool Muted { get; set; }

        /// <summary>
        /// Mute the Audio
        /// </summary>
        void Mute();

        /// <summary>
        /// Unmute the Audio
        /// </summary>
        void Unmute();

        /// <summary>
        /// Set the default volume level
        /// </summary>
        void SetDefaultLevel();

        /// <summary>
        /// Mute change event for IButton interface
        /// </summary>
        event AudioMuteChangeEventHandler MuteChange;

        /// <summary>
        /// Level change event for IGauge interface
        /// </summary>
        event AudioLevelChangeEventHandler LevelChange;
    }

    /// <summary>
    /// Handler delegate to match IDigitalJoin.SetFeedback method
    /// </summary>
    /// <param name="muted">true if level is muted</param>
    public delegate void AudioMuteChangeEventHandler(bool muted);

    /// <summary>
    /// Handler delegate to match IGauge.SetFeedback method
    /// </summary>
    /// <param name="control">IAudioLevelControl interface instance</param>
    /// <param name="level">The value of the level</param>
    public delegate void AudioLevelChangeEventHandler(IAudioLevelControl control, ushort level);

    /// <summary>
    /// Audio control level or mute event type
    /// </summary>
    public enum AudioLevelControlChangeEventType
    {
        /// <summary>
        /// Default event type... not sure what has changed
        /// </summary>
        NotDefined,
        /// <summary>
        /// Audio Level has changed
        /// </summary>
        Level,
        /// <summary>
        /// Audio Mute status has changed
        /// </summary>
        Mute
    }

    /// <summary>
    /// A flag to set the type of level for an IAudioLevelControl object
    /// </summary>
    public enum AudioLevelType
    {
        /// <summary>
        /// Default
        /// </summary>
        NotDefined,
        /// <summary>
        /// Used for program / source volume control
        /// </summary>
        Source,
        /// <summary>
        /// Used for audio conference levels
        /// </summary>
        Conference,
        /// <summary>
        /// Mic mute group or individual mic level control
        /// </summary>
        Microphone,
        /// <summary>
        /// Other type of audio level
        /// </summary>
        Other
    }
}