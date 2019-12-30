using UnityEngine;

namespace Quadtree.Items
{
    /// <summary>
    /// Custom item interface for GameObject quadtree items.
    /// </summary>
    public interface IGameObjectItem : IItem
    {
        /// <summary>
        /// Returns reference to corresponding game object.
        /// </summary>
        /// 
        /// <returns>Game object instance.</returns>
        GameObject GetGameObject();
    }
}
