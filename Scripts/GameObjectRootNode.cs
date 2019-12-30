using Quadtree.Items;
using UnityEngine;

namespace Quadtree
{
    /// <inheritdoc cref="RootNode{TItem}"/>
    [ExecuteInEditMode]
    [AddComponentMenu("Spatial partitioning/Quadtree/Root node (for GameObjects)")]
    public class GameObjectRootNode : RootNode<IGameObjectItem>
    {
        /// <inheritdoc cref="RootNode{TItem}.Rebuild"/>
        [ContextMenu("Rebuild Quadtrees")]
        protected new void Rebuild() => base.Rebuild();
    }
}