#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using System.Globalization;
#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE)
using System.Threading.Tasks;
#endif
using Newtonsoft.Json.Utilities;
using System.Xml;
using Newtonsoft.Json.Converters;
using System.Text;
#if !NET20 && (!SILVERLIGHT || WINDOWS_PHONE) && !PORTABLE
using System.Xml.Linq;
#endif
#if NETFX_CORE
using IConvertible = Newtonsoft.Json.Utilities.Convertible;
#endif

namespace Newtonsoft.Json
{
  /// <summary>
  /// Provides methods for converting between common language runtime types and JSON types.
  /// </summary>
  /// <example>
  ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="SerializeObject" title="Serializing and Deserializing JSON with JsonConvert" />
  /// </example>
  public static class JsonConvert
  {
    /// <summary>
    /// Represents JavaScript's boolean value true as a string. This field is read-only.
    /// </summary>
    public static readonly string True = "true";

    /// <summary>
    /// Represents JavaScript's boolean value false as a string. This field is read-only.
    /// </summary>
    public static readonly string False = "false";

    /// <summary>
    /// Represents JavaScript's null as a string. This field is read-only.
    /// </summary>
    public static readonly string Null = "null";

    /// <summary>
    /// Represents JavaScript's undefined as a string. This field is read-only.
    /// </summary>
    public static readonly string Undefined = "undefined";

    /// <summary>
    /// Represents JavaScript's positive infinity as a string. This field is read-only.
    /// </summary>
    public static readonly string PositiveInfinity = "Infinity";

    /// <summary>
    /// Represents JavaScript's negative infinity as a string. This field is read-only.
    /// </summary>
    public static readonly string NegativeInfinity = "-Infinity";

    /// <summary>
    /// Represents JavaScript's NaN as a string. This field is read-only.
    /// </summary>
    public static readonly string NaN = "NaN";

    internal static readonly long InitialJavaScriptDateTicks = 621355968000000000;

    /// <summary>
    /// Converts the <see cref="DateTime"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="DateTime"/>.</returns>
    public static string ToString(DateTime value)
    {
      return ToString(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind);
    }

    /// <summary>
    /// Converts the <see cref="DateTime"/> to its JSON string representation using the <see cref="DateFormatHandling"/> specified.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="format">The format the date will be converted to.</param>
    /// <param name="timeZoneHandling">The time zone handling when the date is converted to a string.</param>
    /// <returns>A JSON string representation of the <see cref="DateTime"/>.</returns>
    public static string ToString(DateTime value, DateFormatHandling format, DateTimeZoneHandling timeZoneHandling)
    {
      DateTime updatedDateTime = EnsureDateTime(value, timeZoneHandling);

      using (StringWriter writer = StringUtils.CreateStringWriter(64))
      {
        WriteDateTimeString(writer, updatedDateTime, updatedDateTime.GetUtcOffset(), updatedDateTime.Kind, format);
        return writer.ToString();
      }
    }

    internal static DateTime EnsureDateTime(DateTime value, DateTimeZoneHandling timeZone)
    {
      switch (timeZone)
      {
        case DateTimeZoneHandling.Local:
          value = SwitchToLocalTime(value);
          break;
        case DateTimeZoneHandling.Utc:
          value = SwitchToUtcTime(value);
          break;
        case DateTimeZoneHandling.Unspecified:
          value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
          break;
        case DateTimeZoneHandling.RoundtripKind:
          break;
        default:
          throw new ArgumentException("Invalid date time handling value.");
      }

      return value;
    }

#if !PocketPC && !NET20
    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="DateTimeOffset"/>.</returns>
    public static string ToString(DateTimeOffset value)
    {
      return ToString(value, DateFormatHandling.IsoDateFormat);
    }

    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to its JSON string representation using the <see cref="DateFormatHandling"/> specified.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="format">The format the date will be converted to.</param>
    /// <returns>A JSON string representation of the <see cref="DateTimeOffset"/>.</returns>
    public static string ToString(DateTimeOffset value, DateFormatHandling format)
    {
      using (StringWriter writer = StringUtils.CreateStringWriter(64))
      {
        WriteDateTimeString(writer, (format == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, format);
        return writer.ToString();
      }
    }
#endif

    internal static void WriteDateTimeString(TextWriter writer, DateTime value, DateFormatHandling format)
    {
      WriteDateTimeString(writer, value, value.GetUtcOffset(), value.Kind, format);
    }

    internal static void WriteDateTimeString(TextWriter writer, DateTime value, TimeSpan offset, DateTimeKind kind, DateFormatHandling format)
    {
      if (format == DateFormatHandling.MicrosoftDateFormat)
      {
        long javaScriptTicks = ConvertDateTimeToJavaScriptTicks(value, offset);

        writer.Write(@"""\/Date(");
        writer.Write(javaScriptTicks);

        switch (kind)
        {
          case DateTimeKind.Unspecified:
            if (value != DateTime.MaxValue && value != DateTime.MinValue)
              WriteDateTimeOffset(writer, offset, format);
            break;
          case DateTimeKind.Local:
            WriteDateTimeOffset(writer, offset, format);
            break;
        }

        writer.Write(@")\/""");
      }
      else
      {
        writer.Write(@"""");
        writer.Write(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF", CultureInfo.InvariantCulture));

        switch (kind)
        {
          case DateTimeKind.Local:
            WriteDateTimeOffset(writer, offset, format);
            break;
          case DateTimeKind.Utc:
            writer.Write("Z");
            break;
        }


        writer.Write(@"""");
      }
    }

    internal static void WriteDateTimeOffset(TextWriter writer, TimeSpan offset, DateFormatHandling format)
    {
      writer.Write((offset.Ticks >= 0L) ? "+" : "-");

      int absHours = Math.Abs(offset.Hours);
      if (absHours < 10)
        writer.Write(0);
      writer.Write(absHours);

      if (format == DateFormatHandling.IsoDateFormat)
        writer.Write(':');

      int absMinutes = Math.Abs(offset.Minutes);
      if (absMinutes < 10)
        writer.Write(0);
      writer.Write(absMinutes);
    }

    private static long ToUniversalTicks(DateTime dateTime)
    {
      if (dateTime.Kind == DateTimeKind.Utc)
        return dateTime.Ticks;

      return ToUniversalTicks(dateTime, dateTime.GetUtcOffset());
    }

    private static long ToUniversalTicks(DateTime dateTime, TimeSpan offset)
    {
      // special case min and max value
      // they never have a timezone appended to avoid issues
      if (dateTime.Kind == DateTimeKind.Utc || dateTime == DateTime.MaxValue || dateTime == DateTime.MinValue)
        return dateTime.Ticks;

      long ticks = dateTime.Ticks - offset.Ticks;
      if (ticks > 3155378975999999999L)
        return 3155378975999999999L;

      if (ticks < 0L)
        return 0L;

      return ticks;
    }

    internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, TimeSpan offset)
    {
      long universialTicks = ToUniversalTicks(dateTime, offset);

      return UniversialTicksToJavaScriptTicks(universialTicks);
    }

    internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
    {
      return ConvertDateTimeToJavaScriptTicks(dateTime, true);
    }

    internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, bool convertToUtc)
    {
      long ticks = (convertToUtc) ? ToUniversalTicks(dateTime) : dateTime.Ticks;

      return UniversialTicksToJavaScriptTicks(ticks);
    }

    private static long UniversialTicksToJavaScriptTicks(long universialTicks)
    {
      long javaScriptTicks = (universialTicks - InitialJavaScriptDateTicks)/10000;

      return javaScriptTicks;
    }

    internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
    {
      DateTime dateTime = new DateTime((javaScriptTicks*10000) + InitialJavaScriptDateTicks, DateTimeKind.Utc);

      return dateTime;
    }

    private static DateTime SwitchToLocalTime(DateTime value)
    {
      switch (value.Kind)
      {
        case DateTimeKind.Unspecified:
          return new DateTime(value.Ticks, DateTimeKind.Local);

        case DateTimeKind.Utc:
          return value.ToLocalTime();

        case DateTimeKind.Local:
          return value;
      }
      return value;
    }

    private static DateTime SwitchToUtcTime(DateTime value)
    {
      switch (value.Kind)
      {
        case DateTimeKind.Unspecified:
          return new DateTime(value.Ticks, DateTimeKind.Utc);

        case DateTimeKind.Utc:
          return value;

        case DateTimeKind.Local:
          return value.ToUniversalTime();
      }
      return value;
    }

    /// <summary>
    /// Converts the <see cref="Boolean"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Boolean"/>.</returns>
    public static string ToString(bool value)
    {
      return (value) ? True : False;
    }

    /// <summary>
    /// Converts the <see cref="Char"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Char"/>.</returns>
    public static string ToString(char value)
    {
      return ToString(char.ToString(value));
    }

    /// <summary>
    /// Converts the <see cref="Enum"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Enum"/>.</returns>
    public static string ToString(Enum value)
    {
      return value.ToString("D");
    }

    /// <summary>
    /// Converts the <see cref="Int32"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Int32"/>.</returns>
    public static string ToString(int value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Int16"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Int16"/>.</returns>
    public static string ToString(short value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt16"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="UInt16"/>.</returns>
    [CLSCompliant(false)]
    public static string ToString(ushort value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt32"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="UInt32"/>.</returns>
    [CLSCompliant(false)]
    public static string ToString(uint value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Int64"/>  to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Int64"/>.</returns>
    public static string ToString(long value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt64"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="UInt64"/>.</returns>
    [CLSCompliant(false)]
    public static string ToString(ulong value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Single"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Single"/>.</returns>
    public static string ToString(float value)
    {
      return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts the <see cref="Double"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Double"/>.</returns>
    public static string ToString(double value)
    {
      return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
    }

    private static string EnsureDecimalPlace(double value, string text)
    {
      if (double.IsNaN(value) || double.IsInfinity(value) || text.IndexOf('.') != -1 || text.IndexOf('E') != -1 || text.IndexOf('e') != -1)
        return text;

      return text + ".0";
    }

    private static string EnsureDecimalPlace(string text)
    {
      if (text.IndexOf('.') != -1)
        return text;

      return text + ".0";
    }

    /// <summary>
    /// Converts the <see cref="Byte"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Byte"/>.</returns>
    public static string ToString(byte value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="SByte"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="SByte"/>.</returns>
    [CLSCompliant(false)]
    public static string ToString(sbyte value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Decimal"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="SByte"/>.</returns>
    public static string ToString(decimal value)
    {
      return EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts the <see cref="Guid"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Guid"/>.</returns>
    public static string ToString(Guid value)
    {
      string text = null;

#if !(NETFX_CORE || PORTABLE)
      text = value.ToString("D", CultureInfo.InvariantCulture);
#else
      text = value.ToString("D");
#endif

      return '"' + text + '"';
    }

    /// <summary>
    /// Converts the <see cref="TimeSpan"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="TimeSpan"/>.</returns>
    public static string ToString(TimeSpan value)
    {
      return '"' + value.ToString() + '"';
    }

    /// <summary>
    /// Converts the <see cref="Uri"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Uri"/>.</returns>
    public static string ToString(Uri value)
    {
      if (value == null)
        return Null;

      return ToString(value.ToString());
    }

    /// <summary>
    /// Converts the <see cref="String"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="String"/>.</returns>
    public static string ToString(string value)
    {
      return ToString(value, '"');
    }

    /// <summary>
    /// Converts the <see cref="String"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="delimter">The string delimiter character.</param>
    /// <returns>A JSON string representation of the <see cref="String"/>.</returns>
    public static string ToString(string value, char delimter)
    {
      return JavaScriptUtils.ToEscapedJavaScriptString(value, delimter, true);
    }

    /// <summary>
    /// Converts the <see cref="Object"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Object"/>.</returns>
    public static string ToString(object value)
    {
      if (value == null)
        return Null;

      IConvertible convertible = ConvertUtils.ToConvertible(value);

      if (convertible != null)
      {
        switch (convertible.GetTypeCode())
        {
          case TypeCode.String:
            return ToString(convertible.ToString(CultureInfo.InvariantCulture));
          case TypeCode.Char:
            return ToString(convertible.ToChar(CultureInfo.InvariantCulture));
          case TypeCode.Boolean:
            return ToString(convertible.ToBoolean(CultureInfo.InvariantCulture));
          case TypeCode.SByte:
            return ToString(convertible.ToSByte(CultureInfo.InvariantCulture));
          case TypeCode.Int16:
            return ToString(convertible.ToInt16(CultureInfo.InvariantCulture));
          case TypeCode.UInt16:
            return ToString(convertible.ToUInt16(CultureInfo.InvariantCulture));
          case TypeCode.Int32:
            return ToString(convertible.ToInt32(CultureInfo.InvariantCulture));
          case TypeCode.Byte:
            return ToString(convertible.ToByte(CultureInfo.InvariantCulture));
          case TypeCode.UInt32:
            return ToString(convertible.ToUInt32(CultureInfo.InvariantCulture));
          case TypeCode.Int64:
            return ToString(convertible.ToInt64(CultureInfo.InvariantCulture));
          case TypeCode.UInt64:
            return ToString(convertible.ToUInt64(CultureInfo.InvariantCulture));
          case TypeCode.Single:
            return ToString(convertible.ToSingle(CultureInfo.InvariantCulture));
          case TypeCode.Double:
            return ToString(convertible.ToDouble(CultureInfo.InvariantCulture));
          case TypeCode.DateTime:
            return ToString(convertible.ToDateTime(CultureInfo.InvariantCulture));
          case TypeCode.Decimal:
            return ToString(convertible.ToDecimal(CultureInfo.InvariantCulture));
#if !(NETFX_CORE || PORTABLE)
          case TypeCode.DBNull:
            return Null;
#endif
        }
      }
#if !PocketPC && !NET20
      else if (value is DateTimeOffset)
      {
        return ToString((DateTimeOffset) value);
      }
#endif
      else if (value is Guid)
      {
        return ToString((Guid) value);
      }
      else if (value is Uri)
      {
        return ToString((Uri) value);
      }
      else if (value is TimeSpan)
      {
        return ToString((TimeSpan) value);
      }

      throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
    }

    private static bool IsJsonPrimitiveTypeCode(TypeCode typeCode)
    {
      switch (typeCode)
      {
        case TypeCode.String:
        case TypeCode.Char:
        case TypeCode.Boolean:
        case TypeCode.SByte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.Byte:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
        case TypeCode.Single:
        case TypeCode.Double:
        case TypeCode.DateTime:
        case TypeCode.Decimal:
#if !(NETFX_CORE || PORTABLE)
        case TypeCode.DBNull:
#endif
          return true;
        default:
          return false;
      }
    }

    internal static bool IsJsonPrimitiveType(Type type)
    {
      if (ReflectionUtils.IsNullableType(type))
        type = Nullable.GetUnderlyingType(type);

#if !PocketPC && !NET20
      if (type == typeof (DateTimeOffset))
        return true;
#endif
      if (type == typeof (byte[]))
        return true;
      if (type == typeof (Uri))
        return true;
      if (type == typeof (TimeSpan))
        return true;
      if (type == typeof (Guid))
        return true;

      return IsJsonPrimitiveTypeCode(ConvertUtils.GetTypeCode(type));
    }

    #region Serialize
    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value)
    {
      return SerializeObject(value, Formatting.None, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>
    /// A JSON string representation of the object.
    /// </returns>
    public static string SerializeObject(object value, Formatting formatting)
    {
      return SerializeObject(value, formatting, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="converters">A collection converters used while serializing.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value, params JsonConverter[] converters)
    {
      return SerializeObject(value, Formatting.None, converters);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="converters">A collection converters used while serializing.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value, Formatting formatting, params JsonConverter[] converters)
    {
      JsonSerializerSettings settings = (converters != null && converters.Length > 0)
                                          ? new JsonSerializerSettings {Converters = converters}
                                          : null;

      return SerializeObject(value, formatting, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is null, default serialization settings will be is used.</param>
    /// <returns>
    /// A JSON string representation of the object.
    /// </returns>
    public static string SerializeObject(object value, JsonSerializerSettings settings)
    {
      return SerializeObject(value, Formatting.None, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is null, default serialization settings will be is used.</param>
    /// <returns>
    /// A JSON string representation of the object.
    /// </returns>
    public static string SerializeObject(object value, Formatting formatting, JsonSerializerSettings settings)
    {
      JsonSerializer jsonSerializer = JsonSerializer.Create(settings);

      StringBuilder sb = new StringBuilder(256);
      StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = formatting;

        jsonSerializer.Serialize(jsonWriter, value);
      }

      return sw.ToString();
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE)
    /// <summary>
    /// Asynchronously serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>
    /// A task that represents the asynchronous serialize operation. The value of the <c>TResult</c> parameter contains a JSON string representation of the object.
    /// </returns>
    public static Task<string> SerializeObjectAsync(object value)
    {
      return SerializeObjectAsync(value, Formatting.None, null);
    }

    /// <summary>
    /// Asynchronously serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>
    /// A task that represents the asynchronous serialize operation. The value of the <c>TResult</c> parameter contains a JSON string representation of the object.
    /// </returns>
    public static Task<string> SerializeObjectAsync(object value, Formatting formatting)
    {
      return SerializeObjectAsync(value, formatting, null);
    }

    /// <summary>
    /// Asynchronously serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is null, default serialization settings will be is used.</param>
    /// <returns>
    /// A task that represents the asynchronous serialize operation. The value of the <c>TResult</c> parameter contains a JSON string representation of the object.
    /// </returns>
    public static Task<string> SerializeObjectAsync(object value, Formatting formatting, JsonSerializerSettings settings)
    {
      return Task.Factory.StartNew(() => SerializeObject(value, formatting, settings));
    }
#endif
    #endregion

    #region Deserialize
    /// <summary>
    /// Deserializes the JSON to a .NET object.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static object DeserializeObject(string value)
    {
      return DeserializeObject(value, null, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Deserializes the JSON to a .NET object.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static object DeserializeObject(string value, JsonSerializerSettings settings)
    {
      return DeserializeObject(value, null, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static object DeserializeObject(string value, Type type)
    {
      return DeserializeObject(value, type, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static T DeserializeObject<T>(string value)
    {
      return DeserializeObject<T>(value, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Deserializes the JSON to the given anonymous type.
    /// </summary>
    /// <typeparam name="T">
    /// The anonymous type to deserialize to. This can't be specified
    /// traditionally and must be infered from the anonymous type passed
    /// as a parameter.
    /// </typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="anonymousTypeObject">The anonymous type object.</param>
    /// <returns>The deserialized anonymous type from the JSON string.</returns>
    public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
    {
      return DeserializeObject<T>(value);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static T DeserializeObject<T>(string value, params JsonConverter[] converters)
    {
      return (T) DeserializeObject(value, typeof (T), converters);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static T DeserializeObject<T>(string value, JsonSerializerSettings settings)
    {
      return (T) DeserializeObject(value, typeof (T), settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static object DeserializeObject(string value, Type type, params JsonConverter[] converters)
    {
      JsonSerializerSettings settings = (converters != null && converters.Length > 0)
                                          ? new JsonSerializerSettings {Converters = converters}
                                          : null;

      return DeserializeObject(value, type, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static object DeserializeObject(string value, Type type, JsonSerializerSettings settings)
    {
      ValidationUtils.ArgumentNotNull(value, "value");

      StringReader sr = new StringReader(value);
      JsonSerializer jsonSerializer = JsonSerializer.Create(settings);

      // by default DeserializeObject should check for additional content
      if (!jsonSerializer.IsCheckAdditionalContentSet())
        jsonSerializer.CheckAdditionalContent = true;

      return jsonSerializer.Deserialize(new JsonTextReader(sr), type);
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE)
    /// <summary>
    /// Asynchronously deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>
    /// A task that represents the asynchronous deserialize operation. The value of the <c>TResult</c> parameter contains the deserialized object from the JSON string.
    /// </returns>
    public static Task<T> DeserializeObjectAsync<T>(string value)
    {
      return DeserializeObjectAsync<T>(value, null);
    }

    /// <summary>
    /// Asynchronously deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous deserialize operation. The value of the <c>TResult</c> parameter contains the deserialized object from the JSON string.
    /// </returns>
    public static Task<T> DeserializeObjectAsync<T>(string value, JsonSerializerSettings settings)
    {
      return Task.Factory.StartNew(() => DeserializeObject<T>(value, settings));
    }

    /// <summary>
    /// Asynchronously deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>
    /// A task that represents the asynchronous deserialize operation. The value of the <c>TResult</c> parameter contains the deserialized object from the JSON string.
    /// </returns>
    public static Task<object> DeserializeObjectAsync(string value)
    {
      return DeserializeObjectAsync(value, null, null);
    }

    /// <summary>
    /// Asynchronously deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous deserialize operation. The value of the <c>TResult</c> parameter contains the deserialized object from the JSON string.
    /// </returns>
    public static Task<object> DeserializeObjectAsync(string value, Type type, JsonSerializerSettings settings)
    {
      return Task.Factory.StartNew(() => DeserializeObject(value, type, settings));
    }
#endif
    #endregion

    /// <summary>
    /// Populates the object with values from the JSON string.
    /// </summary>
    /// <param name="value">The JSON to populate values from.</param>
    /// <param name="target">The target object to populate values onto.</param>
    public static void PopulateObject(string value, object target)
    {
      PopulateObject(value, target, null);
    }

    /// <summary>
    /// Populates the object with values from the JSON string.
    /// </summary>
    /// <param name="value">The JSON to populate values from.</param>
    /// <param name="target">The target object to populate values onto.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    public static void PopulateObject(string value, object target, JsonSerializerSettings settings)
    {
      StringReader sr = new StringReader(value);
      JsonSerializer jsonSerializer = JsonSerializer.Create(settings);

      using (JsonReader jsonReader = new JsonTextReader(sr))
      {
        jsonSerializer.Populate(jsonReader, target);

        if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
          throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
      }
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE)
    /// <summary>
    /// Asynchronously populates the object with values from the JSON string.
    /// </summary>
    /// <param name="value">The JSON to populate values from.</param>
    /// <param name="target">The target object to populate values onto.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous populate operation.
    /// </returns>
    public static Task PopulateObjectAsync(string value, object target, JsonSerializerSettings settings)
    {
      return Task.Factory.StartNew(() => PopulateObject(value, target, settings));
    }
#endif

#if !(SILVERLIGHT || PORTABLE || NETFX_CORE)
    /// <summary>
    /// Serializes the XML node to a JSON string.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <returns>A JSON string of the XmlNode.</returns>
    public static string SerializeXmlNode(XmlNode node)
    {
      return SerializeXmlNode(node, Formatting.None);
    }

    /// <summary>
    /// Serializes the XML node to a JSON string.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>A JSON string of the XmlNode.</returns>
    public static string SerializeXmlNode(XmlNode node, Formatting formatting)
    {
      XmlNodeConverter converter = new XmlNodeConverter();

      return SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Serializes the XML node to a JSON string.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="omitRootObject">Omits writing the root object.</param>
    /// <returns>A JSON string of the XmlNode.</returns>
    public static string SerializeXmlNode(XmlNode node, Formatting formatting, bool omitRootObject)
    {
      XmlNodeConverter converter = new XmlNodeConverter {OmitRootObject = omitRootObject};

      return SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized XmlNode</returns>
    public static XmlDocument DeserializeXmlNode(string value)
    {
      return DeserializeXmlNode(value, null);
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string nested in a root elment.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized XmlNode</returns>
    public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName)
    {
      return DeserializeXmlNode(value, deserializeRootElementName, false);
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string nested in a root elment.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A flag to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <returns>The deserialized XmlNode</returns>
    public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
    {
      XmlNodeConverter converter = new XmlNodeConverter();
      converter.DeserializeRootElementName = deserializeRootElementName;
      converter.WriteArrayAttribute = writeArrayAttribute;

      return (XmlDocument) DeserializeObject(value, typeof (XmlDocument), converter);
    }
#endif

#if !NET20 && (!(SILVERLIGHT || PORTABLE) || WINDOWS_PHONE)
    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string.
    /// </summary>
    /// <param name="node">The node to convert to JSON.</param>
    /// <returns>A JSON string of the XNode.</returns>
    public static string SerializeXNode(XObject node)
    {
      return SerializeXNode(node, Formatting.None);
    }

    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string.
    /// </summary>
    /// <param name="node">The node to convert to JSON.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>A JSON string of the XNode.</returns>
    public static string SerializeXNode(XObject node, Formatting formatting)
    {
      return SerializeXNode(node, formatting, false);
    }

    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="omitRootObject">Omits writing the root object.</param>
    /// <returns>A JSON string of the XNode.</returns>
    public static string SerializeXNode(XObject node, Formatting formatting, bool omitRootObject)
    {
      XmlNodeConverter converter = new XmlNodeConverter {OmitRootObject = omitRootObject};

      return SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized XNode</returns>
    public static XDocument DeserializeXNode(string value)
    {
      return DeserializeXNode(value, null);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string nested in a root elment.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized XNode</returns>
    public static XDocument DeserializeXNode(string value, string deserializeRootElementName)
    {
      return DeserializeXNode(value, deserializeRootElementName, false);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string nested in a root elment.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A flag to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <returns>The deserialized XNode</returns>
    public static XDocument DeserializeXNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
    {
      XmlNodeConverter converter = new XmlNodeConverter();
      converter.DeserializeRootElementName = deserializeRootElementName;
      converter.WriteArrayAttribute = writeArrayAttribute;

      return (XDocument) DeserializeObject(value, typeof (XDocument), converter);
    }
#endif
  }
}