using Quadtree.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Quadtree
{
    /// <summary>
    /// Main class of the Quadtree structure - it represents the root of the tree.
    /// </summary>
    public class QuadtreeRoot<TItem, TNode> : IQuadtreeRoot<TItem, TNode>
        where TItem : IItem<TItem, TNode>
        where TNode : INode<TItem, TNode>, new()
    {
        public bool Initialized { get; set; }

        public TNode CurrentRootNode { get; internal set; }

        public float MinimumPossibleNodeSize => _minimumPossibleNodeSize;

        /// <inheritdoc cref="IQuadtreeRoot{TItem}.MinimumPossibleNodeSize"/>
        protected float _minimumPossibleNodeSize = 1f;

        public bool DisplayNumberOfItemsInGizmos => _displayNumberOfItemsInGizmos;

        /// <inheritdoc cref="IQuadtreeRoot{TItem}.DisplayNumberOfItemsInGizmos"/>
        protected bool _displayNumberOfItemsInGizmos = false;

        /// <summary>
        /// Determines side of root node expansion if necessary.
        /// </summary>
        protected bool ExpansionRight = true;

        /// <summary>
        /// Initializes Quadtree - creates initial root node and builds the tree (if allowed).
        /// </summary>
        public QuadtreeRoot(Vector3 center, Vector3 size)
        {
            CurrentRootNode = new TNode
            {
                TreeRoot = this,
                ParentNode = default,
                Bounds = new Bounds(center, size),
            };
            Initialized = true;
        }

        public void Insert(TItem item)
        {
            // get item bounds
            var itemBounds = item.GetBounds();

            // expand root node if necessary
            while (!CurrentRootNode.Contains(itemBounds))
                Expand();

            // insert item into the tree
            CurrentRootNode.Insert(item);
        }

        public void Expand()
        {
            // the subnodes will be of the same size as current root node
            var subBoundsSize = CurrentRootNode.Bounds.size;
            var centerOffset = subBoundsSize * .5f;

            // center if expanding to left
            var center = CurrentRootNode.Bounds.min;
            if (ExpansionRight)
            {
                // center if expanding to right
                center = CurrentRootNode.Bounds.max;
            }

            var subNodes = new List<TNode>(4);
            var newRootNode = new TNode
            {
                TreeRoot = this,
                ParentNode = default,
                Bounds = new Bounds(center, subBoundsSize * 2f),
                SubNodes = subNodes,
            };
            CurrentRootNode.ParentNode = newRootNode;

            // top left node [-x +y]
            centerOffset.x *= -1f;
            subNodes.Insert((int)IntraLocation.UPPER_LEFT, new TNode
            {
                TreeRoot = this,
                ParentNode = newRootNode,
                Bounds = new Bounds(center + centerOffset, subBoundsSize),
            });

            // top right node [+x +y]
            centerOffset.x *= -1f;
            subNodes.Insert((int)IntraLocation.UPPER_RIGHT, !ExpansionRight
                ? CurrentRootNode
                : new TNode
                {
                    TreeRoot = this,
                    ParentNode = newRootNode,
                    Bounds = new Bounds(center + centerOffset, subBoundsSize),
                });

            // bottom right node [+x -y]
            centerOffset.z *= -1f;
            subNodes.Insert((int)IntraLocation.LOWER_RIGHT, new TNode
            {
                TreeRoot = this,
                ParentNode = newRootNode,
                Bounds = new Bounds(center + centerOffset, subBoundsSize),
            });

            // bottom left node [-x -y]
            centerOffset.x *= -1f;
            subNodes.Insert((int)IntraLocation.LOWER_LEFT, ExpansionRight
                ? CurrentRootNode
                : new TNode
                {
                    TreeRoot = this,
                    ParentNode = newRootNode,
                    Bounds = new Bounds(center + centerOffset, subBoundsSize),
                });

            // assign new root node
            CurrentRootNode = newRootNode;
            // toggle expansion side for next expansion
            ExpansionRight = !ExpansionRight;
        }

        public List<TItem> Find(Bounds bounds)
        {
            IList<TItem> itemList = new List<TItem>();
            CurrentRootNode.FindAndAddItems(bounds, ref itemList);

            return (List<TItem>)itemList;
        }

        public void Remove(TItem item)
        {
            CurrentRootNode.Remove(item);
        }

        public void Clear()
        {
            CurrentRootNode.Clear();
        }
    }
}