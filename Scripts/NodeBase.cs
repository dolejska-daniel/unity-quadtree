using Quadtree.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Quadtree
{
    /// <summary>
    /// Base quadtree node implementation.
    /// </summary>
    public abstract class NodeBase<TItem, TNode> : INode<TItem, TNode>
        where TItem : IItem<TItem, TNode>
        where TNode : NodeBase<TItem, TNode>, new()
    {
        public Bounds Bounds { get; set; }

        public TNode ParentNode { get; set; }

        public IList<TNode> SubNodes { get; set; }

        public IQuadtreeRoot<TItem, TNode> TreeRoot { get; set; }

        /// <summary>
        /// List of inserted items.
        /// </summary>
        private readonly HashSet<TItem> _items;

        public NodeBase()
        {
            SubNodes = new List<TNode>(4);
            _items = new HashSet<TItem>();
        }

        /// <summary>
        /// Verifies whether provided boundaries (<paramref name="bounds"/>) are fully contained within the boundaries of the node.
        /// </summary>
        /// 
        /// <param name="bounds">Boundaries of an object</param>
        /// <returns><c>True</c> if object is fully contained within the node, <c>False</c> otherwise</returns>
        public bool Contains(Bounds bounds) =>
            bounds.min.x >= Bounds.min.x
            && bounds.min.z >= Bounds.min.z
            && bounds.max.x < Bounds.max.x
            && bounds.max.z < Bounds.max.z;

        public IntraLocation Location(Bounds bounds)
        {
            if (bounds.min.z >= Bounds.center.z)
            {
                // items are located in top sub-nodes
                if (bounds.max.x < Bounds.center.x)
                {
                    // items are located in top left sub-node
                    return IntraLocation.UPPER_LEFT;
                }
                else if (bounds.min.x >= Bounds.center.x)
                {
                    // items are located in top right sub-node
                    return IntraLocation.UPPER_RIGHT;
                }
                else
                {
                    // item does not fit to either one, but is top
                    // (max.x is right, min.x is left)
                    return IntraLocation.SPANNING_UPPER;
                }
            }
            else if (bounds.max.z < Bounds.center.z)
            {
                // items are located in bottom sub-nodes
                if (bounds.max.x < Bounds.center.x)
                {
                    // items are located in bottom left sub-node
                    return IntraLocation.LOWER_LEFT;
                }
                else if (bounds.min.x >= Bounds.center.x)
                {
                    // items are located in bottom right sub-node
                    return IntraLocation.LOWER_RIGHT;
                }
                else
                {
                    // item does not fit to either one, but is bottom
                    // (max.x is right, min.x is left)
                    return IntraLocation.SPANNING_LOWER;
                }
            }
            else
            {
                // item does not fit to any sub-node
                // (max.z is top, min.z is bottom)
                if (bounds.min.x >= Bounds.center.x)
                {
                    // bounds span over top right and bottom right nodes
                    return IntraLocation.SPANNING_RIGHT;
                }
                else if (bounds.max.x < Bounds.center.x)
                {
                    // bounds span over top left and bottom left nodes
                    return IntraLocation.SPANNING_LEFT;
                }
                else
                {
                    // bounds span over all sub-nodes
                    return IntraLocation.SPANNING;
                }
            }
        }

        public void Insert(TItem item)
        {
            // create new sub-nodes
            if (SubNodes.Count == 0)
                CreateSubNodes();

            // sub-nodes can not be created anymore
            if (SubNodes.Count == 0)
            {
                // insert item into this node
                _items.Add(item);
                // and designate this node its parent
                item.ParentNode = (TNode)this;

                return;
            }

            var itemBounds = item.GetBounds();
            var itemBoundsLocation = Location(itemBounds);
            switch (itemBoundsLocation)
            {
                // boundaries are contained within one of the subnodes
                case IntraLocation.UPPER_LEFT:
                case IntraLocation.UPPER_RIGHT:
                case IntraLocation.LOWER_RIGHT:
                case IntraLocation.LOWER_LEFT:
                    SubNodes[(int)itemBoundsLocation].Insert(item);
                    break;

                // boundaries are spanning over 2 or more subnodes
                default:
                    _items.Add(item);
                    item.ParentNode = (TNode)this;
                    break;
            }
        }

        public void Remove(TItem item)
        {
            var itemBounds = item.GetBounds();
            var itemBoundsLocation = Location(itemBounds);
            switch (itemBoundsLocation)
            {
                // boundaries are contained within one of the subnodes
                case IntraLocation.UPPER_LEFT:
                case IntraLocation.UPPER_RIGHT:
                case IntraLocation.LOWER_RIGHT:
                case IntraLocation.LOWER_LEFT:
                    SubNodes[(int)itemBoundsLocation].Remove(item);
                    break;

                // boundaries are spanning over 2 or more subnodes
                default:
                    RemoveOwnItem(item);
                    break;
            }
        }

        /// <summary>
        /// Removes provided item (<paramref name="item"/>) from the node.
        /// </summary>
        /// 
        /// <param name="item">Item to be removed from the node</param>
        /// 
        /// <seealso cref="INode{TItem, TNode}.Clear"/>
        protected internal void RemoveOwnItem(TItem item)
        {
            // remove the item from the node
            _items.Remove(item);
            // update its parent node
            item.ParentNode = null;

            if (IsEmpty())
            {
                // remove subnodes if subtree of this node is empty
                SubNodes.Clear();
            }
        }

        public bool IsEmpty()
        {
            if (_items.Count > 0)
                return false;

            foreach (var subNode in SubNodes)
                if (!subNode.IsEmpty())
                    return false;

            return true;
        }

        public void Update(TItem item, bool forceInsertionEvaluation = true, bool hasOriginallyContainedItem = true)
        {
            if (Contains(item.GetBounds()))
            {
                // item is contained by this node
                if (hasOriginallyContainedItem)
                {
                    // ...and this node has originally contained the item
                    if (forceInsertionEvaluation)
                    {
                        // ...and insertion evaluation is forced
                        // this checks whether the item hasn't moved into any of the subnodes
                        RemoveOwnItem(item);
                        Insert(item);
                    }

                    // item is still contained by its original node, no action necessary
                    return;
                }

                // ...but this node is not its original container
                // insert item either to this node or any of its children
                Insert(item);

                // update has been successful
                return;
            }

            // the item is not contained by this node
            if (ParentNode == null)
            {
                // ...and this node does not have any parent - the tree must be expanded
                TreeRoot.Expand();
                if (ParentNode == null)
                {
                    // the expansion has failed for some reason
                    Debug.LogError("Tree root expansion failed for item " + item.ToString());
                    return;
                }
            }

            // the item is not contained by this node
            if (hasOriginallyContainedItem)
            {
                // ...and this node has originally contained the item - it must be removed
                RemoveOwnItem(item);
            }

            // parent is (now) available
            ParentNode.Update(item, forceInsertionEvaluation, false);
            // the item is now contained by another node, update has been successful
        }

        /// <summary>
        /// Creates sub-nodes for the node.
        /// </summary>
        protected internal void CreateSubNodes()
        {
            var subBoundsSize = Bounds.size * .5f;
            if (subBoundsSize.x < TreeRoot.MinimumPossibleNodeSize
                || subBoundsSize.z < TreeRoot.MinimumPossibleNodeSize)
            {
                // new sub-node bounds are too small
                return;
            }

            var centerOffset = subBoundsSize * .5f;

            // top left node [-x +z]
            centerOffset.x *= -1f;
            SubNodes.Insert((int)IntraLocation.UPPER_LEFT, new TNode()
            {
                TreeRoot = TreeRoot,
                ParentNode = (TNode)this,
                Bounds = new Bounds(Bounds.center + centerOffset, subBoundsSize),
            });

            // top right node [+x +z]
            centerOffset.x *= -1f;
            SubNodes.Insert((int)IntraLocation.UPPER_RIGHT, new TNode()
            {
                TreeRoot = TreeRoot,
                ParentNode = (TNode)this,
                Bounds = new Bounds(Bounds.center + centerOffset, subBoundsSize),
            });

            // bottom right node [+x -z]
            centerOffset.z *= -1f;
            SubNodes.Insert((int)IntraLocation.LOWER_RIGHT, new TNode()
            {
                TreeRoot = TreeRoot,
                ParentNode = (TNode)this,
                Bounds = new Bounds(Bounds.center + centerOffset, subBoundsSize),
            });

            // bottom left node [-x -z]
            centerOffset.x *= -1f;
            SubNodes.Insert((int)IntraLocation.LOWER_LEFT, new TNode()
            {
                TreeRoot = TreeRoot,
                ParentNode = (TNode)this,
                Bounds = new Bounds(Bounds.center + centerOffset, subBoundsSize),
            });
        }

        public void FindAndAddItems(Bounds bounds, ref IList<TItem> items)
        {
            if (SubNodes.Count == 0)
            {
                // no sub-nodes exist
                AddOwnItems(ref items);
                return;
            }

            // always add any items in this node intersecting with the boundaries
            AddOwnItems(ref items, bounds);

            var boundsLocation = Location(bounds);
            switch (boundsLocation)
            {
                // boundaries are contained within one of the subnodes
                case IntraLocation.UPPER_LEFT:
                case IntraLocation.UPPER_RIGHT:
                case IntraLocation.LOWER_RIGHT:
                case IntraLocation.LOWER_LEFT:
                    SubNodes[(int)boundsLocation].FindAndAddItems(bounds, ref items);
                    break;

                // boundaries are spanning over left subnodes
                case IntraLocation.SPANNING_LEFT:
                    SubNodes[(int)IntraLocation.UPPER_LEFT].AddItems(ref items, bounds);
                    SubNodes[(int)IntraLocation.LOWER_LEFT].AddItems(ref items, bounds);
                    break;

                // boundaries are spanning over right subnodes
                case IntraLocation.SPANNING_RIGHT:
                    SubNodes[(int)IntraLocation.UPPER_RIGHT].AddItems(ref items, bounds);
                    SubNodes[(int)IntraLocation.LOWER_RIGHT].AddItems(ref items, bounds);
                    break;

                // boundaries are spanning over upper subnodes
                case IntraLocation.SPANNING_UPPER:
                    SubNodes[(int)IntraLocation.UPPER_LEFT].AddItems(ref items, bounds);
                    SubNodes[(int)IntraLocation.UPPER_RIGHT].AddItems(ref items, bounds);
                    break;

                // boundaries are spanning over lower subnodes
                case IntraLocation.SPANNING_LOWER:
                    SubNodes[(int)IntraLocation.LOWER_LEFT].AddItems(ref items, bounds);
                    SubNodes[(int)IntraLocation.LOWER_RIGHT].AddItems(ref items, bounds);
                    break;

                // boundaries are spanning over all subnodes
                case IntraLocation.SPANNING:
                default:
                    AddSubNodeItems(ref items, bounds);
                    break;
            }
        }

        public void AddItems(ref IList<TItem> items, Bounds? bounds = null)
        {
            AddOwnItems(ref items, bounds);
            AddSubNodeItems(ref items, bounds);
        }

        /// <summary>
        /// Adds all items belonging to this node (ignoring sub-nodes) to the provided list of items (<paramref name="items"/>).
        /// If boundaries (<paramref name="bounds"/>) are provided then only items intersecting with them will be added.
        /// </summary>
        /// 
        /// <param name="items">Output list for found items</param>
        /// <param name="bounds">Boundaries to look for items within</param>
        protected internal void AddOwnItems(ref IList<TItem> items, Bounds? bounds = null)
        {
            var itemSource = bounds != null
                ? _items.Where(item => item.GetBounds().Intersects((Bounds)bounds))
                : _items;

            foreach (var item in itemSource)
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// Adds all items belonging to sub-nodes (ignoring own items) to the provided list of items (<paramref name="items"/>).
        /// If boundaries (<paramref name="bounds"/>) are provided then only items intersecting with them will be added.
        /// </summary>
        /// 
        /// <param name="items">Output list for found items</param>
        /// <param name="bounds">Boundaries to look for items within</param>
        protected internal void AddSubNodeItems(ref IList<TItem> items, Bounds? bounds = null)
        {
            foreach (var subNode in SubNodes)
                subNode.AddItems(ref items, bounds);
        }

        public void Clear()
        {
            _items.Clear();
            SubNodes.Clear();
        }

        public void DrawBounds(bool displayNumberOfItems = false)
        {
            if (displayNumberOfItems)
                Handles.Label(Bounds.center, _items.Count.ToString());

            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
            foreach (var subNode in SubNodes)
                subNode.DrawBounds();
        }
    }
}