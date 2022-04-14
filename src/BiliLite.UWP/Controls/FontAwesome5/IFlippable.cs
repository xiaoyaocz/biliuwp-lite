using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FontAwesome5
{
    /// <summary>
    /// Defines the different flip orientations that a icon can have.
    /// </summary>
    #if !WINDOWS_UWP
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    #endif
    public enum EFlipOrientation
    {
        /// <summary>
        /// Default
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Flip horizontally (on x-achsis)
        /// </summary>
        Horizontal,
        /// <summary>
        /// Flip vertically (on y-achsis)
        /// </summary>
        Vertical,
    }

    /// <summary>
    /// Represents a flippable control
    /// </summary>
    public interface IFlippable
    {
        /// <summary>
        /// Gets or sets the current orientation (horizontal, vertical).
        /// </summary>
        EFlipOrientation FlipOrientation { get; set; }
    }
}
