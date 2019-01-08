using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace importfromcsv
{
    static class EnumerableDataReader
    {
        public static EnumerableDataReader<T> AsDataReader<T>(this IEnumerable<T> enumerable) =>
            new EnumerableDataReader<T>(enumerable);
    }

    sealed class EnumerableDataReader<T> : IDataReader
    {
        private static readonly PropertyInfo[] fields =
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        private static readonly Dictionary<string, int> ordinals =
            fields.
            Select((field, index) => (field, index)).
            ToDictionary(entry => entry.field.Name, entry => entry.Item2);
        private static readonly Func<object, object>[] getters =
            fields.
            Select(field => new Func<object, object>(instance => field.GetValue(instance))).
            ToArray();
        private static readonly Dictionary<string, Func<object, object>> namedGetters =
            fields.
            Select((field, index) => (field, getters[index])).
            ToDictionary(entry => entry.field.Name, entry => entry.Item2);

        private readonly IEnumerator enumerator;

        public EnumerableDataReader(IEnumerable<T> enumerable)
        {
            enumerator = enumerable.GetEnumerator();
        }

        public object this[int i] => getters[i](enumerator.Current);
        public object this[string name] => namedGetters[name](enumerator.Current);

        public bool IsClosed => enumerator.Current == null;
        public int FieldCount => getters.Length;

        public bool Read() => enumerator.MoveNext();
        public bool NextResult() => false;
        public void Close() => (enumerator as IDisposable)?.Dispose();
        public void Dispose() => this.Close();

        public Type GetFieldType(int i) => fields[i].PropertyType;
        public string GetName(int i) => fields[i].Name;
        public int GetOrdinal(string name) => ordinals[name];

        public bool GetBoolean(int i) => (bool)getters[i](enumerator.Current);
        public byte GetByte(int i) => (byte)getters[i](enumerator.Current);
        public char GetChar(int i) => (char)getters[i](enumerator.Current);
        public DateTime GetDateTime(int i) => (DateTime)getters[i](enumerator.Current);
        public decimal GetDecimal(int i) => (decimal)getters[i](enumerator.Current);
        public double GetDouble(int i) => (double)getters[i](enumerator.Current);
        public float GetFloat(int i) => (float)getters[i](enumerator.Current);
        public Guid GetGuid(int i) => (Guid)getters[i](enumerator.Current);
        public short GetInt16(int i) => (short)getters[i](enumerator.Current);
        public int GetInt32(int i) => (int)getters[i](enumerator.Current);
        public long GetInt64(int i) => (long)getters[i](enumerator.Current);
        public string GetString(int i) => (string)getters[i](enumerator.Current);
        public object GetValue(int i) => getters[i](enumerator.Current);
        public bool IsDBNull(int i) => DBNull.Value == getters[i](enumerator.Current);

        public int GetValues(object[] values) =>
            Enumerable.Range(0, values.Length).
            Zip(getters, (index, getter) => values[index] = getter(enumerator.Current)).
            Count();

        public int Depth => throw new NotImplementedException();
        public int RecordsAffected => throw new NotImplementedException();

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
            throw new NotImplementedException();

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) =>
            throw new NotImplementedException();

        public IDataReader GetData(int i) =>
            throw new NotImplementedException();

        public string GetDataTypeName(int i) =>
            throw new NotImplementedException();

        public DataTable GetSchemaTable() =>
            throw new NotImplementedException();
    }
}
