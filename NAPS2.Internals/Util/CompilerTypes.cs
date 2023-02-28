// ReSharper disable once CheckNamespace

#if !NET6_0_OR_GREATER
namespace System
{
    using System.Runtime.CompilerServices;
    
    public readonly struct Index : IEquatable<Index>
    {
        private readonly int _value;
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Index(int value, bool fromEnd = false)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
 
            if (fromEnd)
                _value = ~value;
            else
                _value = value;
        }
 
        private Index(int value)
        {
            _value = value;
        }
 
        public static Index Start => new Index(0);
 
        public static Index End => new Index(~0);
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromStart(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
 
            return new Index(value);
        }
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromEnd(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
 
            return new Index(~value);
        }
 
        public int Value
        {
            get
            {
                if (_value < 0)
                    return ~_value;
                else
                    return _value;
            }
        }
 
        public bool IsFromEnd => _value < 0;
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int length)
        {
            int offset = _value;
            if (IsFromEnd)
            {
                offset += length + 1;
            }
            return offset;
        }
 
        public override bool Equals(object? value) => value is Index && _value == ((Index)value)._value;
        public bool Equals(Index other) => _value == other._value;
        public override int GetHashCode() => _value;
        public static implicit operator Index(int value) => FromStart(value);
    }
}
#endif