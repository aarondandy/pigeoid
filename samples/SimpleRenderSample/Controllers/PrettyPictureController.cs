using DotSpatial.Data;
using Pigeoid;
using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using Vertesaur;
using Vertesaur.Transformation;

namespace SimpleRenderSample.Controllers
{
    public class PrettyPictureController : ApiController
    {

        private static IEnumerable<List<GeographicCoordinate>> ReadPagesToGeographicCoordinate(IEnumerator<Dictionary<int, Shape>> pages) {
            while (pages.MoveNext()) {
                var page = pages.Current;
                var points = new List<GeographicCoordinate>(page.Count);
                foreach (var shape in page.Values) {
                    var p = new GeographicCoordinate(shape.Vertices[1], shape.Vertices[0]);
                    points.Add(p);
                }
                yield return points;
            }
        }

        [HttpGet]
        public HttpResponseMessage Draw(double? minLon, double? maxLon, double? minLat, double? maxLat, int? maxImgWidth, int? maxImgHeight) {
            var shapefilePath = HostingEnvironment.MapPath("~/App_Data/builtupp_usa/builtupp_usa.shp");
            
            ITransformation<GeographicCoordinate, Point2> transformation = null;
            var targetCrs = EpsgMicroDatabase.Default.GetCrs(3005);
            LongitudeDegreeRange dataLongitudeRange;
            Range dataLatitudeRange;
            using (var shapeFile = Shapefile.Open(shapefilePath)) {
                dataLongitudeRange = new LongitudeDegreeRange(shapeFile.Extent.MinX, shapeFile.Extent.MaxX);
                dataLatitudeRange = new Range(shapeFile.Extent.MinY, shapeFile.Extent.MaxY);

                var dataCrs = EpsgMicroDatabase.Default.GetCrs(4326);
                var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
                var paths = pathGenerator.Generate(dataCrs, targetCrs);
                var compiler = new StaticCoordinateOperationCompiler();
                var firstTransfom = paths.Select(p => {
                    return compiler.Compile(p);
                }).First(x => x != null);

                transformation = firstTransfom as ITransformation<GeographicCoordinate, Point2>;
                if (transformation == null && firstTransfom is IEnumerable<ITransformation>)
                    transformation = new CompiledConcatenatedTransformation<GeographicCoordinate, Point2>((IEnumerable<ITransformation>)firstTransfom);

            }

            var geoMbrMin = new GeographicCoordinate(minLat ?? dataLatitudeRange.Low, minLon ?? dataLongitudeRange.Start);
            var geoMbrMax = new GeographicCoordinate(maxLat ?? dataLatitudeRange.High, maxLon ?? dataLongitudeRange.End);
            var geoMbrTL = new GeographicCoordinate(geoMbrMax.Latitude, geoMbrMin.Longitude);
            var geoMbrTR = new GeographicCoordinate(geoMbrMin.Latitude, geoMbrMax.Longitude);

            var projectedMbrPoints = new[] {
                    geoMbrMin,
                    geoMbrMax,
                    geoMbrTL,
                    geoMbrTR,
                    new GeographicCoordinate(geoMbrMin.Latitude, Math.Abs(geoMbrMin.Longitude + geoMbrMax.Longitude) / 2.0)
                }
                .Select(transformation.TransformValue)
                .ToArray();


            var projectedExtent = new Mbr(
                new Point2(projectedMbrPoints.Min(x => x.X), projectedMbrPoints.Min(x => x.Y)),
                new Point2(projectedMbrPoints.Max(x => x.X), projectedMbrPoints.Max(x => x.Y))
            );

            var geogMapOrigin = new GeographicCoordinate(dataLatitudeRange.Mid, dataLongitudeRange.Mid);
            var mapOrigin = transformation.TransformValue(geogMapOrigin);

            var mapOffset = new Vector2(0/*-(mapOrigin.X - projectedExtent.X.Mid)*/, projectedExtent.Height / 2.0);

            var imageSizeLimits = new Vector2(maxImgWidth ?? 300, maxImgHeight ?? 300);
            if (imageSizeLimits.X > 4096 || imageSizeLimits.Y > 4096)
                throw new ArgumentException("Image size too large");

            var dataRatio = new Vector2(projectedExtent.Width / imageSizeLimits.X, projectedExtent.Height / imageSizeLimits.Y);
            var lowCorner = projectedExtent.Min;
            Vector2 desiredImageSize;
            double imageScaleFactor;
            if (dataRatio.Y < dataRatio.X) {
                imageScaleFactor = imageSizeLimits.X / projectedExtent.Width;
                desiredImageSize = new Vector2(imageSizeLimits.X, (int)(projectedExtent.Height * imageScaleFactor));
            }
            else {
                imageScaleFactor = imageSizeLimits.Y / projectedExtent.Height;
                desiredImageSize = new Vector2((int)(projectedExtent.Width * imageScaleFactor), imageSizeLimits.Y);
            }

            using (var image = new System.Drawing.Bitmap((int)desiredImageSize.X, (int)desiredImageSize.Y))
            using (var graphics = Graphics.FromImage(image)) {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                var shapeSource = new PointShapefileShapeSource(shapefilePath);
                var shapeReader = new ShapeReader(shapeSource);
                using (var shapeEnumerator = shapeReader.GetEnumerator()) {
                    var sourceCoordinates = ReadPagesToGeographicCoordinate(shapeEnumerator).SelectMany(x => x);
                    var pointColor = Color.Black;
                    var circleFillBrush = new SolidBrush(Color.FromArgb(64,16,64,128));
                    var featureRadius = 3.0;
                    var featureDiameter = featureRadius * 2;
                    var featureDiameterFloat = (float)featureDiameter;
                    var topLeftOffset = new Vector2(-featureRadius,-featureRadius);

                    foreach (var transformedPoint in transformation.TransformValues(sourceCoordinates)) {
                        var offsetPoint = transformedPoint.Difference(lowCorner).Add(mapOffset);



                        var scaledPoint = offsetPoint.GetScaled(imageScaleFactor);
                        var screenPoint = new Point2(scaledPoint.X, image.Height - scaledPoint.Y);
                        var drawTopLeft = screenPoint.Add(topLeftOffset);

                        graphics.FillEllipse(circleFillBrush, (float)drawTopLeft.X, (float)drawTopLeft.Y, featureDiameterFloat, featureDiameterFloat);
                    }
                }
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                byte[] imageBytes;
                using (var memoryStream = new MemoryStream()) {
                    image.Save(memoryStream, ImageFormat.Png);
                    imageBytes = memoryStream.ToArray();
                }
                result.Content = new ByteArrayContent(imageBytes);
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                return result;
            }
        }

    }
}
