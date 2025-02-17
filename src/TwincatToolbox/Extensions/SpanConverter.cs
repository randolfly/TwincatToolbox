using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

public static class SpanConverter
{
    public static T ConvertTo<T>(ReadOnlySpan<byte> span) where T : struct {
        if (span.Length < Marshal.SizeOf<T>())
        {
            throw new ArgumentException("The span is too small to contain the specified type.");
        }

        return MemoryMarshal.Read<T>(span);
    }

    public static object ConvertTo(ReadOnlySpan<byte> span, Type targetType) {
        if (span == null || span.Length == 0)
        {
            throw new ArgumentException("Span is null or empty.", nameof(span));
        }

        if (targetType == null)
        {
            throw new ArgumentNullException(nameof(targetType));
        }

        switch (Type.GetTypeCode(targetType))
        {
            case TypeCode.Boolean:
                if (span.Length >= sizeof(Boolean))
                {
                    return BitConverter.ToBoolean(span);
                }
                break;
            case TypeCode.UInt16:
                if (span.Length >= sizeof(UInt16))
                {
                    return BitConverter.ToUInt16(span);
                }
                break;
            case TypeCode.UInt32:
                if (span.Length >= sizeof(UInt32))
                {
                    return BitConverter.ToUInt32(span);
                }
                break;
            case TypeCode.UInt64:
                if (span.Length >= sizeof(UInt64))
                {
                    return BitConverter.ToUInt64(span);
                }
                break;
            case TypeCode.Int16:
                if (span.Length >= sizeof(Int16))
                {
                    return BitConverter.ToInt16(span);
                }
                break;
            case TypeCode.Int32:
                if (span.Length >= sizeof(Int32))
                {
                    return BitConverter.ToInt32(span);
                }
                break;
            case TypeCode.Int64:
                if (span.Length >= sizeof(Int64))
                {
                    return BitConverter.ToInt64(span);
                }
                break;
            case TypeCode.Single:
                if (span.Length >= sizeof(Single))
                {
                    return BitConverter.ToSingle(span);
                }
                break;
            case TypeCode.Double:
                if (span.Length >= sizeof(Double))
                {
                    return BitConverter.ToDouble(span);
                }
                break;
            // todo: add more cases
            default:
                throw new NotSupportedException($"Conversion to type '{targetType}' is not supported.");
        }

        throw new ArgumentException("Span does not contain enough data to convert to the specified type.", nameof(span));
    }
}


