using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tests.DataStructures
{
    public class Stack<T>
    {
        #region Members

        StackNode<T> root = null;
        int size;

        #endregion

        #region Constructors

        public Stack(T root)
        {
            if (root is null)
                throw new Exception("Invalid data");

            size = 1;
            this.root = new StackNode<T>(root);
        }

        #endregion

        #region Methods
        public void Pop()
        {
            if (root is null)
                return;

            root = root.GetNext;
            size--;
        }

        public void Push(T data)
        {
            var newNode = new StackNode<T>(data);
            var prevRoot = root;
            root = newNode;
            root.SetNext(prevRoot);
            size++;
        }

        public T Top
        {
            get
            {
                return root.GetData;
            }
        }

        public bool isEmpty
        {
            get
            {
                return size == 0;
            }
        }

        #endregion
    }

    class StackNode<T>
    {
        T data = default(T);
        StackNode<T> next = null;
        StackNode<T> prev = null;

        public StackNode(T data)
        {
            this.data = data;
        }

        #region Methods
        public void SetData(T data)
        {
            this.data = data;
        }

        public void SetNext(StackNode<T> next)
        {
            if (next is null)
                return;
            this.next = next;
        }

        public void SetPrev(StackNode<T> prev)
        {
            if (prev is null)
                return;
            this.prev = prev;
        }
        #endregion

        #region Get/Set Definitions
        public T GetData
        {
            get
            {
                return data;
            }
        }

        public StackNode<T> GetNext
        {
            get
            {
                return next;
            }
        }

        public StackNode<T> GetPrev
        {
            get
            {
                return prev;
            }
        }
        #endregion
    }
}
