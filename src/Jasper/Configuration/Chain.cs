using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Baseline;

namespace Jasper.Configuration
{


    public abstract class Chain<T, TChain> : INode<T>, IEnumerable<T>
            where T : Node<T, TChain>
            where TChain : Chain<T, TChain>
    {
        /// <summary>
        ///   Adds a new Node to the very end of this behavior chain
        /// </summary>
        /// <param name = "node"></param>
        public void AddToEnd(T node)
        {
            if (Top == null)
            {
                SetTop(node);
                return;
            }

            Top.AddToEnd(node);
        }

        /// <summary>
        ///   Adds a new Node of type T to the very end of this
        ///   behavior chain
        /// </summary>
        /// <typeparam name = "T"></typeparam>
        /// <typeparam name="TNode"></typeparam>
        /// <returns></returns>
        public TNode AddToEnd<TNode>() where TNode : T, new()
        {
            var node = new TNode();
            AddToEnd(node);
            return node;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Top == null) yield break;

            yield return Top;

            foreach (var node in Top)
            {
                yield return node;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        internal void SetTop(T node)
        {
            if (node == null)
            {
                Top = null;
            }
            else
            {
                node.Previous = null;

                if (Top != null)
                {
                    Top.Chain = null;
                }

                Top = node;
                node.Chain = this.As<TChain>();
            }
        }

        public void InsertFirst(T node)
        {
            var previousTop = Top;

            SetTop(node);

            if (previousTop != null)
            {
                Top.Next = previousTop;
            }
        }

        /// <summary>
        /// The outermost Node in the chain
        /// </summary>
        public T Top { get; private set; }

        /// <summary>
        /// Sets the specified Node as the outermost node
        /// in this chain
        /// </summary>
        /// <param name="node"></param>
        public void Prepend(T node)
        {
            var next = Top;
            SetTop(node);

            if (next != null)
            {
                Top.Next = next;
            }
        }


        void INode<T>.AddAfter(T node)
        {
            AddToEnd(node);
        }

        void INode<T>.AddBefore(T node)
        {
            throw new NotSupportedException();
        }
    }
}