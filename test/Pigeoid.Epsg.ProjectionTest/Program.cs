using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Pigeoid.CoordinateOperation;

namespace Pigeoid.Epsg.ProjectionTest
{
    class Program
    {
        static void Main(string[] args) {
            //Test();

            //for (int i = 0; i < 10; i++)
            //	Test();

            var test = new CrsTest();
            test.Run(SaveResultsToHtml);

            Console.WriteLine("Press the [Any] key to close.");
            Console.ReadKey();
        }

        static void Test() {
            var generator = new EpsgCrsCoordinateOperationPathGenerator();
            var from = EpsgCrs.Get(2003); // EpsgCrs.Get(4267);
            var to = EpsgCrs.Get(2007);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var path = generator.Generate(from, to);
            stopwatch.Stop();
            Console.WriteLine("{0} ({1})", stopwatch.Elapsed, stopwatch.Elapsed.TotalMilliseconds);
        }

        static void SaveToDb(IEnumerable<CrsTestCase> results) {
            var dbFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "result.db");
            if (File.Exists(dbFilePath))
                File.Delete(dbFilePath);

            using (var conn = new SQLiteConnection(String.Format("Data Source={0}", dbFilePath))) {
                conn.Open();
                using (var cmd = conn.CreateCommand()) {
                    cmd.CommandText = "CREATE TABLE paths(f INTEGER, t INTEGER)";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand()) {
                    cmd.Transaction = conn.BeginTransaction();
                    int counter = 0;
                    foreach (var result in results.Where(x => x.Operations != null)) {
                        cmd.CommandText = "INSERT INTO paths VALUES(@fromCode,@toCode)";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("fromCode", DbType.Int32).Value = ((EpsgCrs)result.From).Code;
                        cmd.Parameters.Add("toCode", DbType.Int32).Value = ((EpsgCrs)result.To).Code;

                        counter++;
                        if (counter > 512) {
                            counter = 0;
                            cmd.Transaction.Commit();
                            cmd.Transaction = conn.BeginTransaction();
                        }
                    }
                    cmd.Transaction.Commit();
                }
            }
        }

        [Obsolete]
        static void SaveResultsToHtml(IEnumerable<CrsTestCase> results) {
            var desiredFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "result.htm");
            using (var fileOut = File.Open(desiredFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(fileOut)) {
                writer.WriteLine("<html><head><title>EPSG Projection Report</title>");
                writer.WriteLine("<style type=\"text/css\">");
                writer.WriteLine(".op{border: solid 1px #bbbbff;}");
                writer.WriteLine(".crs{font-weight: bold;}");
                writer.WriteLine("</style>");
                writer.WriteLine("</head><body>");
                writer.WriteLine("<table width=\"100%\">");
                foreach (var result in results.Where(x => x.Operations != null)) {
                    writer.Write("<tr><td>");
                    writer.Write(result.From);
                    writer.Write("</td><td>");
                    writer.Write(result.To);
                    writer.Write("</td><td>");
                    WriteOperationAsHtml(writer, result.Operations);
                    writer.WriteLine("</td></tr>");
                }
                writer.WriteLine("</table>");
                writer.WriteLine("</body></html>");
            }
        }

        [Obsolete]
        private static void WriteOperationAsHtml(StreamWriter writer, ICoordinateOperationCrsPathInfo op) {
            if (null == op) {
                writer.Write("noop");
                return;
            }

            using (var nodeEnum = op.CoordinateReferenceSystems.GetEnumerator())
            using (var edgeEnum = op.CoordinateOperations.GetEnumerator()) {
                while (nodeEnum.MoveNext() && edgeEnum.MoveNext()) {
                    WriteCrsAsHtml(writer, nodeEnum.Current);
                    writer.Write(' ');
                    WriteCoordOpAsHtml(writer, edgeEnum.Current);
                    writer.Write(' ');
                }
                WriteCrsAsHtml(writer, nodeEnum.Current);
            }

        }

        [Obsolete]
        private static void WriteCoordOpAsHtml(StreamWriter writer, ICoordinateOperationInfo op) {
            if (null == op) {
                writer.Write("ERROR");
                return;
            }
            writer.Write("<span class=\"op\">");
            WriteCoordOpCore(writer, op);
            writer.Write("</span>");
        }

        [Obsolete]
        private static void WriteCoordOpCore(StreamWriter writer, ICoordinateOperationInfo op) {
            if (op is IConcatenatedCoordinateOperationInfo) {
                bool first = true;
                foreach (var subOp in ((IConcatenatedCoordinateOperationInfo)op).Steps) {
                    if (first)
                        first = false;
                    else
                        writer.Write(';');
                    WriteCoordOpCore(writer, subOp);
                }
            }
            else if (op is EpsgCoordinateOperationInfo) {
                var epsgOp = (EpsgCoordinateOperationInfo)op;
                writer.Write(epsgOp.Code);
            }
            else if (op is EpsgCoordinateOperationInverse) {
                var invOp = (EpsgCoordinateOperationInverse)op;
                writer.Write("Inv ");
                WriteCoordOpCore(writer, invOp.Core);
            }
            else {
                writer.Write(op.Name);
            }
        }

        [Obsolete]
        private static void WriteCrsAsHtml(StreamWriter writer, ICrs crs) {
            if (null == crs) {
                writer.Write("ERROR");
                return;
            }
            writer.Write("<span class=\"crs\">");
            if (crs is EpsgCrs)
                writer.Write((crs as EpsgCrs).Code);
            else
                writer.Write(crs.Name);
            writer.Write("</span>");
        }

    }
}
