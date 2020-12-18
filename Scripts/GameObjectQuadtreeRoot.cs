using Quadtree.Items;
using UnityEngine;

namespace Quadtree
{
    [ExecuteInEditMode]
    [AddComponentMenu("Spatial partitioning/Quadtree/Root node (for GameObjects)")]
    public class GameObjectQuadtreeRoot : QuadtreeMonoRoot<GameObjectItem, Node<GameObjectItem>>
    {
    }
}