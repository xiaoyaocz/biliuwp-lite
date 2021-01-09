using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontAwesome5
{
    /// <summary>
    /// Represents a rotatable control
    /// </summary>
    public interface IRotatable
    {
        /// <summary>
        /// Gets or sets the current rotation (angle).
        /// </summary>
        double Rotation { get; set; }
    }
}
