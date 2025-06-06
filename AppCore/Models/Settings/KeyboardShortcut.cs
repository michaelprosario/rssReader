namespace AppCore.Models.Settings
{
    /// <summary>
    /// Represents a keyboard shortcut for an action in the application
    /// </summary>
    public class KeyboardShortcut : BaseEntity
    {
        /// <summary>
        /// Name of the action
        /// </summary>
        public string ActionName { get; set; } = string.Empty;

        /// <summary>
        /// Description of the action
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Key combination (e.g., "Ctrl+R", "Alt+S")
        /// </summary>
        public string KeyCombination { get; set; } = string.Empty;

        /// <summary>
        /// Whether the shortcut is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Whether this is a default shortcut or user-defined
        /// </summary>
        public bool IsDefault { get; set; } = true;
    }
}
