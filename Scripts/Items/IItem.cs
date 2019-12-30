using UnityEngine;

namespace Quadtree.Items
{
    /// <summary>
    /// Mandatory interface of any quadtree item.
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// Returns object bounds.
        /// </summary>
        /// 
        /// <returns>Object box bounds.</returns>
        Bounds GetBounds();
    }
}