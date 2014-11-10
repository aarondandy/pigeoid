using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Vertesaur;

namespace Pigeoid.GoldData.Test
{
	public class GeoTransGoldDataReader : IDisposable
	{

		private readonly bool _ownStream;
		private TextReader _reader;
		private string _curLine;
		private bool _inHeader;
		private Dictionary<string, string> _header;

		private static readonly Regex HeaderLineRegex = new Regex(@"^\s*([^#]+)\s*[:]\s*([^#]+)\s*(?:[#].*)?$", RegexOptions.Compiled);
		private static readonly Regex Point2Regex = new Regex(@"^\s*([^#]+)\s*[:,]\s*([^#]+)\s*(?:[#].*)?$", RegexOptions.Compiled);
		private static readonly Regex Point3Regex = new Regex(@"^\s*([^#]+)\s*[,]\s*([^#]+)\s*[,]\s*([^#]+)\s*(?:[#].*)?$", RegexOptions.Compiled);
		private static readonly Regex CommentRegex = new Regex(@"^\s*[#].*", RegexOptions.Compiled);


		public GeoTransGoldDataReader(TextReader reader)
		{
			if (null == reader) throw new ArgumentNullException("reader");

			_reader = reader;
			_inHeader = true;
			_curLine = null;
			_ownStream = false;
		}

		private bool ReadHeader()
		{
			_header = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			do
			{
				_curLine = _reader.ReadLine();
				if (null == _curLine)
				{
					return false;
				}
				_curLine = _curLine.Trim();
				Match m = HeaderLineRegex.Match(_curLine);
				if (m.Groups.Count >= 3)
				{
					_header[m.Groups[1].Value.Trim()] = m.Groups[2].Value.Trim();
				}
			} while (!"END OF HEADER".Equals(_curLine.Trim()));
			_curLine = null;
			_inHeader = false;
			return true;
		}

		private bool ReadLine() {
			do {
				_curLine = _reader.ReadLine();
				if (null == _curLine)
					return false;

			} while (String.IsNullOrEmpty(_curLine) || CommentRegex.IsMatch(_curLine));
			return true;
		}

		public bool Read()
		{
			if (_inHeader)
				return ReadHeader();

			return ReadLine();
		}

		public string this[string key]
		{
			get
			{
				string value;
				return (
					(_header.TryGetValue(key, out value))
					? value
					: null
				);
			}
		}

		public Point2 CurrentPoint2D()
		{
			if (null != _curLine)
			{
				var m = Point2Regex.Match(_curLine);
				if (m.Groups.Count >= 3)
				{
					return new Point2(
						Double.Parse(m.Groups[1].Value.Trim()),
						Double.Parse(m.Groups[2].Value.Trim())
					);
				}
			}
			return new Point2(Double.NaN, Double.NaN);
		}

		public void Dispose()
		{
			if (null != _reader && _ownStream)
			{
				_reader.Dispose();
				_reader = null;
			}
		}


		public GeographicCoordinate CurrentLatLon()
		{
			if (null != _curLine)
			{
				var m = Point2Regex.Match(_curLine);
				if (m.Groups.Count >= 3)
				{
					return new GeographicCoordinate(
						Double.Parse(m.Groups[1].Value.Trim()),
						Double.Parse(m.Groups[2].Value.Trim())
					);
				}
			}
			return new GeographicCoordinate(Double.NaN, Double.NaN);
		}

		public GeographicCoordinate CurrentLatLonRadians()
		{
			var coordinate = CurrentLatLon();
			return new GeographicCoordinate(coordinate.Latitude * Math.PI / 180.0, coordinate.Longitude * Math.PI / 180.0);
		}

	}
}
