using Quadtree.Items;
using UnityEngine;

namespace Quadtree
{
    /// <inheritdoc cref="RootNode{TItem}"/>
    [ExecuteInEditMode]
    [AddComponentMenu("Spatial partitioning/Quadtree/Root node (for GameObjects)")]
    public class GameObjectRootNode : QuadtreeMonoRoot<GameObjectItem>
    {
    }
}