using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace importfromcsv
{
    static class Program
    {
        private static double? ParseDouble(this string str) =>
            double.TryParse(str, out var value) ? (double?)value : null;
        private static DateTimeOffset? ParseDateTimeOffset(this string str) =>
            DateTimeOffset.TryParse(str, out var value) ? (DateTimeOffset?)value : null;

        // 名前からSQLスクリプトを取得する
        private static async Task<string> GetScriptTextAsync(string name)
        {
            using (var s = typeof(Program).Assembly.GetManifestResourceStream("importfromcsv.scripts." + name))
            {
                var tr = new StreamReader(s, Encoding.UTF8);
                return await tr.ReadToEndAsync();
            }
        }

        // CSV駅情報のエンティティ
        private sealed class StationOriginal
        {
            public StationOriginal()
            {
            }

            public long id { get; set; }
            public double lon { get; set; }
            public double lat { get; set; }
            public string station_name { get; set; }
            public string add { get; set; }
        }

        private static async Task<int> MainAsync(string[] args)
        {
            // GPSロガーの内容を読み取る
            var kml = XDocument.Load(args[0]);
            var ns = kml.Root.Name.Namespace;

            // データベースに接続する
            var dataBasePath = args[2];
            var connectionString =
                $@"Data Source=(localdb)\MSSQLLocalDB;AttachDBFilename={dataBasePath};Integrated Security=True;Connect Timeout=30";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                ////////////////////////////////////////////////////////////////////////////////
                // Step 1: 準備

                // activity_originalテーブル生成
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = await GetScriptTextAsync("activity_original.sql");

                    await command.ExecuteNonQueryAsync();
                }

                // activity_original (GPSデータの時系列座標データ / KML / 自分が自転車で記録したもの)
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "activity_original";

                    await bulkCopy.WriteToServerAsync((
                        from folder in kml.Descendants(ns + "Folder")
                        from name in folder.Elements(ns + "name")
                        where name.Value == "Track Points"
                        from placeMark in folder.Elements(ns + "Placemark")
                        from timeSpan in placeMark.Elements(ns + "TimeSpan")
                        from begin in timeSpan.Elements(ns + "begin")
                        from point in placeMark.Elements(ns + "Point")
                        from coordinates in point.Elements(ns + "coordinates")
                        let items = coordinates.Value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0]
                        where items.Length == 3
                        let lon = items[0].ParseDouble()
                        let lat = items[1].ParseDouble()
                        let alt = items[2].ParseDouble()
                        let dateTime = begin.Value.ParseDateTimeOffset()
                        where dateTime.HasValue && lon.HasValue && lat.HasValue && alt.HasValue
                        select new { dateTime = dateTime.Value, lon = lon.Value, lat = lat.Value, alt = alt.Value }).
                        Select((entry, index) => new { id = index, entry.dateTime, entry.lon, entry.lat, entry.alt }).
                        AsDataReader());
                }

                // activity_pointテーブル生成 (geography座標生成)
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = await GetScriptTextAsync("activity_point.sql");

                    await command.ExecuteNonQueryAsync();
                }

                /////////////////////////////////////////////////

                // station_originalテーブル生成
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = await GetScriptTextAsync("station_original.sql");

                    await command.ExecuteNonQueryAsync();
                }

                // station_original (CSV駅情報: 日本の駅データ (Author: Hiroaki Hattori / CC-BY)
                // http://linkdata.org/work/rdf1s4125i
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "station_original";

                    var reader = CsvReader.Create<StationOriginal>(args[1]);

                    await bulkCopy.WriteToServerAsync(reader.AsDataReader());
                }

                // station_pointテーブル生成 (geography座標生成)
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = await GetScriptTextAsync("station_point.sql");

                    await command.ExecuteNonQueryAsync();
                }

                ////////////////////////////////////////////////////////////////////////////////
                // Step 2: 抽出

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = await GetScriptTextAsync("analyze_activity.sql");

                    await WriteAnalyzedDataAsync(command, args[3]);
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = await GetScriptTextAsync("analyze_station.sql");

                    await WriteAnalyzedDataAsync(command, args[4]);
                }
            }

            return 0;
        }

        private static async Task WriteAnalyzedDataAsync(SqlCommand command, string path)
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                using (var fs = new FileStream(
                    path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 65536, true))
                {
                    var tw = new StreamWriter(fs, Encoding.UTF8);

                    if (await reader.ReadAsync())
                    {
                        // ヘッダ
                        await tw.WriteLineAsync(
                            string.Join(",",
                                Enumerable.Range(0, reader.FieldCount).
                                Select(reader.GetName)));

                        var data = new object[reader.FieldCount];

                        reader.GetValues(data);
                        await tw.WriteLineAsync(string.Join(",", data));

                        while (await reader.ReadAsync())
                        {
                            reader.GetValues(data);
                            await tw.WriteLineAsync(string.Join(",", data));
                        }
                    }

                    await tw.FlushAsync();
                }
            }
        }

        static int Main(string[] args) =>
            MainAsync(args).Result;
    }
}
