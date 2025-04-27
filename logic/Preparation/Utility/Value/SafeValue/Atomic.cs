﻿using Preparation.Interface;
using Preparation.Utility.Value.SafeValue.LockedValue;
using Preparation.Utility.Value.SafeValue.SafeMethod;
using System;
using System.Threading;

namespace Preparation.Utility.Value.SafeValue.Atomic
{
    public abstract class Atomic
    {
    }

    public class AtomicT<T>(T? x) : Atomic, IReturnClass<T>
        where T : class?
    {
        protected T? v = x;

        public override string ToString() => Interlocked.CompareExchange(ref v, null, null)?.ToString() ?? "NULL";
        public T? Get() => Interlocked.CompareExchange(ref v, null, null);
        public static implicit operator T?(AtomicT<T> aint) => Interlocked.CompareExchange(ref aint.v, null, null);
        public override bool Equals(object? obj)
        {
            T? g = Get();
            if (g == null) return obj == null;
            if (g == obj) return true;
            return obj != null && (obj is IReturnClass<T> k) && g == k.Get();
        }
        public override int GetHashCode()
        {
            return Get()?.GetHashCode() ?? 0;
        }
        /// <returns>返回操作前的值</returns>
        public virtual T? SetROri(T? value) => Interlocked.Exchange(ref v, value);
        /// <returns>返回操作前的值</returns>
        public virtual T? CompareExROri(T? newV, T? compareTo) => Interlocked.CompareExchange(ref v, newV, compareTo);
    }

    public class AtomicTNotNull<T>(T x) : Atomic, IReturnClass<T> where T : class
    {
        protected T v = x;

        public override string ToString() => Interlocked.CompareExchange(ref v!, null, null).ToString()!;
        public T Get() => Interlocked.CompareExchange(ref v!, null, null);
        public static implicit operator T(AtomicTNotNull<T> aint) => Interlocked.CompareExchange(ref aint.v!, null, null);
        public override bool Equals(object? obj)
        {
            T? g = Get();
            if (g == null) return obj == null;
            if (g == obj) return true;
            return obj != null && (obj is IReturnClass<T> k) && g == k.Get();
        }
        public override int GetHashCode()
        {
            return Get().GetHashCode();
        }
        /// <returns>返回操作前的值</returns>
        public virtual T SetROri(T value) => Interlocked.Exchange(ref v, value);
        /// <returns>返回操作前的值</returns>
        public virtual T CompareExROri(T newV, T compareTo) => Interlocked.CompareExchange(ref v, newV, compareTo);
    }

    public class AtomicDouble(double x) : Atomic, IDouble, IConvertible
    {
        private double v = x;

        #region 实现IConvertible接口

        public TypeCode GetTypeCode()
        {
            return TypeCode.Double;
        }

        public bool ToBoolean(IFormatProvider? provider)
        {
            return Convert.ToBoolean(Get(), provider);
        }

        public char ToChar(IFormatProvider? provider)
        {
            return Convert.ToChar(Get(), provider);
        }

        public sbyte ToSByte(IFormatProvider? provider)
        {
            return Convert.ToSByte(Get(), provider);
        }

        public byte ToByte(IFormatProvider? provider)
        {
            return Convert.ToByte(Get(), provider);
        }

        public short ToInt16(IFormatProvider? provider)
        {
            return Convert.ToInt16(Get(), provider);
        }

        public ushort ToUInt16(IFormatProvider? provider)
        {
            return Convert.ToUInt16(Get(), provider);
        }

        public int ToInt32(IFormatProvider? provider)
        {
            return Convert.ToInt32(Get(), provider);
        }

        public uint ToUInt32(IFormatProvider? provider)
        {
            return Convert.ToUInt32(Get(), provider);
        }

        public long ToInt64(IFormatProvider? provider)
        {
            return Convert.ToInt64(Get(), provider);
        }

        public ulong ToUInt64(IFormatProvider? provider)
        {
            return Convert.ToUInt64(Get(), provider);
        }

        public float ToSingle(IFormatProvider? provider)
        {
            return Convert.ToSingle(Get(), provider);
        }

        public double ToDouble(IFormatProvider? provider)
        {
            return Get();
        }

        public decimal ToDecimal(IFormatProvider? provider)
        {
            return Convert.ToDecimal(Get(), provider);
        }

        public DateTime ToDateTime(IFormatProvider? provider)
        {
            return Convert.ToDateTime(Get(), provider);
        }

        public string ToString(IFormatProvider? provider)
        {
            return Get().ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            return Convert.ChangeType(Get(), conversionType, provider);
        }
        #endregion

        public override string ToString() => Interlocked.CompareExchange(ref v, -2.0, -2.0).ToString();
        public double Get() => Interlocked.CompareExchange(ref v, -2.0, -2.0);
        public double ToDouble() => Get();
        public static implicit operator double(AtomicDouble adouble) => Interlocked.CompareExchange(ref adouble.v, -2.0, -2.0);
        public override bool Equals(object? obj)
        {
            return obj != null && (obj is IConvertible k) && ToDouble(null) == k.ToDouble(null);
        }
        public override int GetHashCode()
        {
            return Get().GetHashCode();
        }
        /// <returns>返回操作前的值</returns>
        public double SetROri(double value) => Interlocked.Exchange(ref v, value);
        public void Set(double value) => Interlocked.Exchange(ref v, value);
        /// <returns>返回操作前的值</returns>
        public double CompareExROri(double newV, double compareTo) => Interlocked.CompareExchange(ref v, newV, compareTo);
    }

    //许多情况下，volatile即可
    public class AtomicBool(bool x) : Atomic, IAddable<bool>, IConvertible
    {
        private int v = x ? 1 : 0;//v&1==0为false,v&1==1为true

        #region 实现IConvertible接口
        public TypeCode GetTypeCode()
        {
            return TypeCode.Boolean;
        }

        public bool ToBoolean(IFormatProvider? provider)
        {
            return Get();
        }

        public char ToChar(IFormatProvider? provider)
        {
            return Convert.ToChar(Get(), provider);
        }

        public sbyte ToSByte(IFormatProvider? provider)
        {
            return Convert.ToSByte(Get(), provider);
        }

        public byte ToByte(IFormatProvider? provider)
        {
            return Convert.ToByte(Get(), provider);
        }

        public short ToInt16(IFormatProvider? provider)
        {
            return Convert.ToInt16(Get(), provider);
        }

        public ushort ToUInt16(IFormatProvider? provider)
        {
            return Convert.ToUInt16(Get(), provider);
        }

        public int ToInt32(IFormatProvider? provider)
        {
            return Convert.ToInt32(Get(), provider);
        }

        public uint ToUInt32(IFormatProvider? provider)
        {
            return Convert.ToUInt32(Get(), provider);
        }

        public long ToInt64(IFormatProvider? provider)
        {
            return Convert.ToInt64(Get(), provider);
        }

        public ulong ToUInt64(IFormatProvider? provider)
        {
            return Convert.ToUInt64(Get(), provider);
        }

        public float ToSingle(IFormatProvider? provider)
        {
            return Convert.ToSingle(Get(), provider);
        }

        public double ToDouble(IFormatProvider? provider)
        {
            return Convert.ToDouble(Get(), provider);
        }

        public decimal ToDecimal(IFormatProvider? provider)
        {
            return Convert.ToDecimal(Get(), provider);
        }

        public DateTime ToDateTime(IFormatProvider? provider)
        {
            throw new InvalidCastException("Double to DateTime conversion is not supported.");
        }

        public string ToString(IFormatProvider? provider)
        {
            return Get().ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            return Convert.ChangeType(Get(), conversionType, provider);
        }
        #endregion

        public override string ToString() => (Interlocked.CompareExchange(ref v, -2, -2) & 1) == 0 ? "false" : "true";
        public bool Get() => (Interlocked.CompareExchange(ref v, -2, -2) & 1) == 1;
        public static implicit operator bool(AtomicBool abool) => abool.Get();
        public override bool Equals(object? obj)
        {
            return obj != null && (obj is IConvertible k) && ToDouble(null) == k.ToDouble(null);
        }
        public override int GetHashCode()
        {
            return Get().GetHashCode();
        }

        /// <returns>返回操作前的值</returns>
        public bool SetROri(bool value) => (Interlocked.Exchange(ref v, value ? 1 : 0) & 1) == 1;
        public void Set(bool value) => Interlocked.Exchange(ref v, value ? 1 : 0);

        /// <returns>赋值前的值是否与将赋予的值不相同</returns>
        public bool TrySet(bool value)
        {
            return (Interlocked.Exchange(ref v, value ? 1 : 0) & 1) != (value ? 1 : 0);
        }

        public bool And(bool x) => (Interlocked.And(ref v, x ? 1 : 0) & 1) == 1;
        public bool Or(bool x) => (Interlocked.Or(ref v, x ? 1 : 0) & 1) == 1;
        /// <returns>返回操作后的值</returns>
        public bool Reverse() => (Interlocked.Increment(ref v) & 1) == 1;
        public bool Xor(bool x) => (Interlocked.Add(ref v, x ? 1 : 0) & 1) == 1;

        /// <returns>等价于异或</returns>
        public void Add(bool x) => Xor(x);
        /// <returns>等价于异或</returns>
        public void Sub(bool x) => Xor(x);
    }

    public class AtomicEnum<T>(T x) : Atomic
        where T : struct, System.Enum
    {
        protected T v = x;

        public T Get() => InterlockedEx.ReadEnum(ref v);
        public override string ToString() => Get().ToString();
        public override bool Equals(object? obj)
        {
            return obj != null && (obj is AtomicEnum<T> k) && Get().Equals(k.Get());
        }
        public override int GetHashCode()
        {
            return Get().GetHashCode();
        }

        public virtual void Set(T value) => InterlockedEx.ExchangeEnum(ref v, value);
        /// <returns>返回操作前的值</returns>
        public virtual T SetROri(T value) => InterlockedEx.ExchangeEnum(ref v, value);

        /// <returns>返回操作前的值</returns>
        public virtual T CompareExROri(T newV, T compareTo) => InterlockedEx.CompareExchangeEnum(ref v, newV, compareTo);
    }
}
