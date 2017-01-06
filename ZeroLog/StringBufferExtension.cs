using System;
using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    public static unsafe class StringBufferExtension
    {
        public static int Append(this StringBuffer stringBuffer, byte* dataPointer, StringView format, List<string> strings, List<IntPtr> argPointers)
        {
            var argumentType = *(ArgumentType*)dataPointer;
            dataPointer += sizeof(byte);

            switch (argumentType)
            {
                case ArgumentType.String:
                    var stringIndex = *dataPointer;
                    stringBuffer.Append(strings[stringIndex]);
                    return 2 * sizeof(byte);

                case ArgumentType.BooleanTrue:
                    stringBuffer.Append(true);
                    return sizeof(byte);

                case ArgumentType.BooleanFalse:
                    stringBuffer.Append(false);
                    return sizeof(byte);

                case ArgumentType.Byte:
                    stringBuffer.Append(*dataPointer, format);
                    return 2 * sizeof(byte);

                case ArgumentType.Char:
                    stringBuffer.Append(*(char*)dataPointer);
                    return sizeof(byte) + sizeof(char);

                case ArgumentType.Int16:
                    stringBuffer.Append(*(short*)dataPointer, format);
                    return sizeof(byte) + sizeof(short);

                case ArgumentType.Int32:
                    stringBuffer.Append(*(int*)dataPointer, format);
                    return sizeof(byte) + sizeof(int);

                case ArgumentType.Int64:
                    stringBuffer.Append(*(long*)dataPointer, format);
                    return sizeof(byte) + sizeof(long);

                case ArgumentType.Single:
                    stringBuffer.Append(*(float*)dataPointer, format);
                    return sizeof(byte) + sizeof(float);

                case ArgumentType.Double:
                    stringBuffer.Append(*(double*)dataPointer, format);
                    return sizeof(byte) + sizeof(double);

                case ArgumentType.Decimal:
                    stringBuffer.Append(*(decimal*)dataPointer, format);
                    return sizeof(byte) + sizeof(decimal);

                case ArgumentType.Guid:
                    stringBuffer.Append(*(Guid*)dataPointer, format);
                    return sizeof(byte) + sizeof(Guid);

                case ArgumentType.DateTime:
                    var dateTime = ReadDateTime(dataPointer);
                    stringBuffer.Append(dateTime, format);
                    return sizeof(byte) + sizeof(ulong);

                case ArgumentType.TimeSpan:
                    var timeSpan = ReadTimeSpan(dataPointer);
                    stringBuffer.Append(timeSpan, format);
                    return sizeof(byte) + sizeof(long);

                case ArgumentType.Format:
                    var argSet = new ArgSet(argPointers, strings);
                    stringBuffer.AppendArgSet(strings[*dataPointer], ref argSet);
                    return 2 * sizeof(byte) + argSet.ReadBytes;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static DateTime ReadDateTime(byte* buffer)
        {
            var dateData = *(ulong*)buffer;
            var ticks = (long)(dateData & 0x3FFFFFFFFFFFFFFF);
            var kind = (DateTimeKind)(dateData & 0xC000000000000000);
            return new DateTime(ticks, kind);
        }

        private static TimeSpan ReadTimeSpan(byte* buffer)
        {
            var ticks = *(long*)buffer;
            return new TimeSpan(ticks);
        }
    }
}