using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Util
{
    /// <summary>
    /// Class for implicitly converting arrays of structures to unmanaged objects addressed by IntPtr.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnmanagedArray<T> : UnmanagedBase<T[]>
    {
        public UnmanagedArray(IEnumerable<T> array)
        {
            ElementSize = Marshal.SizeOf(typeof(T));
            if (array != null)
            {
                var arrayVal = array as IList<T> ?? array.ToList();

                Length = arrayVal.Count;
                Size = ElementSize * Length;
                Pointer = Marshal.AllocHGlobal(Size);

                // Populate the contents of the unmanaged array
                for (int i = 0; i < Length; ++i)
                {
                    Marshal.StructureToPtr(arrayVal[i], this[i], false);
                }
            }
        }

        /// <summary>
        /// Gets the size of each element in the unmanaged array in bytes.
        /// </summary>
        public int ElementSize { get; }

        /// <summary>
        /// Gets the number of elements in the unmanaged array.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the pointer offset to access the element of the unmanaged array at the given index.
        /// </summary>
        /// <param name="index">The index of the element to access.</param>
        /// <returns>The offset, relative to the Pointer.</returns>
        public int Offset(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return index * ElementSize;
        }

        /// <summary>
        /// Gets the pointer to the element of the unmanaged array at the given index.
        /// </summary>
        /// <param name="index">The index of the item to access.</param>
        /// <returns>The pointer to the element.</returns>
        public IntPtr PointerWithOffset(int index)
        {
            return Pointer + Offset(index);
        }

        protected override T[] GetValue()
        {
            var result = new T[Length];
            for (int i = 0; i < Length; ++i)
            {
                result[i] = (T)Marshal.PtrToStructure(this[i], typeof(T));
            }
            return result;
        }

        protected override void DestroyStructures()
        {
            for (int i = 0; i < Length; ++i)
            {
                Marshal.DestroyStructure(this[i], typeof(T));
            }
        }

        /// <summary>
        /// Gets the pointer to the element of the unmanaged array at the given index.
        /// </summary>
        /// <param name="index">The index of the item to access.</param>
        /// <returns>The pointer to the element.</returns>
        public IntPtr this[int index] => PointerWithOffset(index);
    }
}