using UnityEngine;

namespace Quadtree.Items
{
    /// <summary>
    /// 
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// Returns object bounds.
        /// </summary>
        /// 
        /// <returns>Object box bounds.</returns>
        Bounds GetBounds();

        /// <summary>
        /// Returns reference to corresponding game object.
        /// </summary>
        /// 
        /// <returns>Game object instance.</returns>
        GameObject GetGameObject();
    }
}