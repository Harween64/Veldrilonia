using System.Drawing;
using System.Numerics;
using LibTessDotNet;
using Svg;
using Svg.Pathing;

namespace Veldridonia.Core.Svg;

/// <summary>
/// Tesselle les elements SVG en maillages triangules via LibTessDotNet.
/// Gere les couleurs solides et les degradees (lineaires et radiaux).
/// </summary>
public static class SvgTessellator
{
    private const float CurveFlatteningTolerance = 1.0f;
    private const int CurveSegments = 16;

    /// <summary>
    /// Tesselle un document SVG complet en un seul maillage fusionne.
    /// </summary>
    public static SvgMeshData Tessellate(SvgDocument document)
    {
        var meshes = new List<SvgMeshData>();
        TessellateElement(document, meshes);

        if (meshes.Count == 0)
            return new SvgMeshData([], []);

        return SvgMeshData.Merge(meshes);
    }

    private static void TessellateElement(SvgElement element, List<SvgMeshData> meshes)
    {
        switch (element)
        {
            case SvgPath path:
                TessellatePath(path, meshes);
                break;
            case SvgRectangle rect:
                TessellateRectangle(rect, meshes);
                break;
            case SvgCircle circle:
                TessellateCircle(circle, meshes);
                break;
            case SvgEllipse ellipse:
                TessellateEllipse(ellipse, meshes);
                break;
            case SvgPolygon polygon:
                TessellatePolygon(polygon, meshes);
                break;
            case SvgLine line:
                TessellateLine(line, meshes);
                break;
        }

        foreach (var child in element.Children)
        {
            TessellateElement(child, meshes);
        }
    }

    private static void TessellatePath(SvgPath path, List<SvgMeshData> meshes)
    {
        if (path.PathData?.Count is null or 0)
            return;

        var contours = ExtractContoursFromPath(path.PathData);
        if (contours.Count == 0)
            return;

        // Remplissage
        if (path.Fill is not null and not SvgColourServer { Colour.IsEmpty: true })
        {
            var fillMesh = TessellateContours(contours, element: path, useStroke: false);
            if (fillMesh != null)
                meshes.Add(fillMesh);
        }

        // Contour (stroke)
        if (path.Stroke is not null and not SvgColourServer { Colour.IsEmpty: true } && path.StrokeWidth.Value > 0)
        {
            var strokeMesh = TessellateStroke(contours, path);
            if (strokeMesh != null)
                meshes.Add(strokeMesh);
        }
    }

    private static void TessellateRectangle(SvgRectangle rect, List<SvgMeshData> meshes)
    {
        float x = rect.X.Value;
        float y = rect.Y.Value;
        float w = rect.Width.Value;
        float h = rect.Height.Value;

        var contour = new List<Vector2>
        {
            new(x, y),
            new(x + w, y),
            new(x + w, y + h),
            new(x, y + h)
        };

        var contours = new List<List<Vector2>> { contour };

        if (rect.Fill is not null and not SvgColourServer { Colour.IsEmpty: true })
        {
            var fillMesh = TessellateContours(contours, element: rect, useStroke: false);
            if (fillMesh != null)
                meshes.Add(fillMesh);
        }

        if (rect.Stroke is not null and not SvgColourServer { Colour.IsEmpty: true } && rect.StrokeWidth.Value > 0)
        {
            var strokeMesh = TessellateStroke(contours, rect);
            if (strokeMesh != null)
                meshes.Add(strokeMesh);
        }
    }

    private static void TessellateCircle(SvgCircle circle, List<SvgMeshData> meshes)
    {
        float cx = circle.CenterX.Value;
        float cy = circle.CenterY.Value;
        float r = circle.Radius.Value;

        var contour = GenerateEllipseContour(cx, cy, r, r);
        var contours = new List<List<Vector2>> { contour };

        if (circle.Fill is not null and not SvgColourServer { Colour.IsEmpty: true })
        {
            var fillMesh = TessellateContours(contours, element: circle, useStroke: false);
            if (fillMesh != null)
                meshes.Add(fillMesh);
        }

        if (circle.Stroke is not null and not SvgColourServer { Colour.IsEmpty: true } && circle.StrokeWidth.Value > 0)
        {
            var strokeMesh = TessellateStroke(contours, circle);
            if (strokeMesh != null)
                meshes.Add(strokeMesh);
        }
    }

    private static void TessellateEllipse(SvgEllipse ellipse, List<SvgMeshData> meshes)
    {
        float cx = ellipse.CenterX.Value;
        float cy = ellipse.CenterY.Value;
        float rx = ellipse.RadiusX.Value;
        float ry = ellipse.RadiusY.Value;

        var contour = GenerateEllipseContour(cx, cy, rx, ry);
        var contours = new List<List<Vector2>> { contour };

        if (ellipse.Fill is not null and not SvgColourServer { Colour.IsEmpty: true })
        {
            var fillMesh = TessellateContours(contours, element: ellipse, useStroke: false);
            if (fillMesh != null)
                meshes.Add(fillMesh);
        }

        if (ellipse.Stroke is not null and not SvgColourServer { Colour.IsEmpty: true } && ellipse.StrokeWidth.Value > 0)
        {
            var strokeMesh = TessellateStroke(contours, ellipse);
            if (strokeMesh != null)
                meshes.Add(strokeMesh);
        }
    }

    private static void TessellatePolygon(SvgPolygon polygon, List<SvgMeshData> meshes)
    {
        if (polygon.Points.Count < 6) // Besoin d'au moins 3 points (x,y paires)
            return;

        var contour = new List<Vector2>();
        for (int i = 0; i < polygon.Points.Count - 1; i += 2)
        {
            contour.Add(new Vector2(polygon.Points[i], polygon.Points[i + 1]));
        }

        var contours = new List<List<Vector2>> { contour };

        if (polygon.Fill is not null and not SvgColourServer { Colour.IsEmpty: true })
        {
            var fillMesh = TessellateContours(contours, element: polygon, useStroke: false);
            if (fillMesh != null)
                meshes.Add(fillMesh);
        }

        if (polygon.Stroke is not null and not SvgColourServer { Colour.IsEmpty: true } && polygon.StrokeWidth.Value > 0)
        {
            var strokeMesh = TessellateStroke(contours, polygon);
            if (strokeMesh != null)
                meshes.Add(strokeMesh);
        }
    }

    private static void TessellateLine(SvgLine line, List<SvgMeshData> meshes)
    {
        if (line.Stroke is null or SvgColourServer { Colour.IsEmpty: true } || line.StrokeWidth.Value <= 0)
            return;

        var contour = new List<Vector2>
        {
            new(line.StartX.Value, line.StartY.Value),
            new(line.EndX.Value, line.EndY.Value)
        };

        var strokeMesh = TessellateStroke([contour], line);
        if (strokeMesh != null)
            meshes.Add(strokeMesh);
    }

    private static List<List<Vector2>> ExtractContoursFromPath(SvgPathSegmentList segments)
    {
        var contours = new List<List<Vector2>>();
        var currentContour = new List<Vector2>();
        var currentPos = Vector2.Zero;

        foreach (var segment in segments)
        {
            switch (segment)
            {
                case SvgMoveToSegment moveTo:
                    if (currentContour.Count > 0)
                    {
                        contours.Add(currentContour);
                        currentContour = [];
                    }
                    currentPos = ToVector2(moveTo.End);
                    currentContour.Add(currentPos);
                    break;

                case SvgLineSegment lineTo:
                    currentPos = ToVector2(lineTo.End);
                    currentContour.Add(currentPos);
                    break;

                case SvgCubicCurveSegment cubic:
                    var cubicPoints = FlattenCubicBezier(
                        currentPos,
                        ToVector2(cubic.FirstControlPoint),
                        ToVector2(cubic.SecondControlPoint),
                        ToVector2(cubic.End),
                        CurveSegments
                    );
                    currentContour.AddRange(cubicPoints);
                    currentPos = ToVector2(cubic.End);
                    break;

                case SvgQuadraticCurveSegment quad:
                    var quadPoints = FlattenQuadraticBezier(
                        currentPos,
                        ToVector2(quad.ControlPoint),
                        ToVector2(quad.End),
                        CurveSegments
                    );
                    currentContour.AddRange(quadPoints);
                    currentPos = ToVector2(quad.End);
                    break;

                case SvgArcSegment arc:
                    var arcPoints = FlattenArc(currentPos, arc);
                    currentContour.AddRange(arcPoints);
                    currentPos = ToVector2(arc.End);
                    break;

                case SvgClosePathSegment:
                    if (currentContour.Count > 0)
                    {
                        contours.Add(currentContour);
                        currentContour = [];
                        // Revenir au debut du contour
                        if (contours[^1].Count > 0)
                            currentPos = contours[^1][0];
                    }
                    break;
            }
        }

        if (currentContour.Count > 0)
            contours.Add(currentContour);

        return contours;
    }

    /// <summary>
    /// Tesselle des contours en triangles via LibTessDotNet.
    /// </summary>
    private static SvgMeshData? TessellateContours(
        List<List<Vector2>> contours,
        SvgElement element,
        bool useStroke)
    {
        var tess = new Tess();

        foreach (var contour in contours)
        {
            if (contour.Count < 3)
                continue;

            var tessContour = new ContourVertex[contour.Count];
            for (int i = 0; i < contour.Count; i++)
            {
                tessContour[i] = new ContourVertex
                {
                    Position = new Vec3(contour[i].X, contour[i].Y, 0)
                };
            }
            tess.AddContour(tessContour);
        }

        tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

        if (tess.ElementCount == 0)
            return null;

        var paintServer = useStroke ? element.Stroke : element.Fill;
        float opacity = useStroke ? element.StrokeOpacity : element.FillOpacity;

        // Calculer les bornes pour les degradees
        var bounds = ComputeBounds(contours);

        var vertices = new SvgVertex[tess.VertexCount];
        for (int i = 0; i < tess.VertexCount; i++)
        {
            var pos = new Vector2(tess.Vertices[i].Position.X, tess.Vertices[i].Position.Y);
            var color = ResolveColor(paintServer, pos, bounds, opacity);
            vertices[i] = new SvgVertex(pos, color);
        }

        var indices = new uint[tess.ElementCount * 3];
        for (int i = 0; i < tess.ElementCount; i++)
        {
            indices[i * 3] = (uint)tess.Elements[i * 3];
            indices[i * 3 + 1] = (uint)tess.Elements[i * 3 + 1];
            indices[i * 3 + 2] = (uint)tess.Elements[i * 3 + 2];
        }

        return new SvgMeshData(vertices, indices);
    }

    /// <summary>
    /// Genere un maillage de stroke en expandant chaque segment en un quad.
    /// </summary>
    private static SvgMeshData? TessellateStroke(List<List<Vector2>> contours, SvgElement element)
    {
        float strokeWidth = element.StrokeWidth.Value;
        float halfWidth = strokeWidth / 2.0f;

        var paintServer = element.Stroke;
        float opacity = element.StrokeOpacity;
        var bounds = ComputeBounds(contours);

        var vertices = new List<SvgVertex>();
        var indices = new List<uint>();

        foreach (var contour in contours)
        {
            if (contour.Count < 2)
                continue;

            for (int i = 0; i < contour.Count - 1; i++)
            {
                var p0 = contour[i];
                var p1 = contour[i + 1];

                var dir = Vector2.Normalize(p1 - p0);
                var normal = new Vector2(-dir.Y, dir.X) * halfWidth;

                var color0 = ResolveColor(paintServer, p0, bounds, opacity);
                var color1 = ResolveColor(paintServer, p1, bounds, opacity);

                uint baseIndex = (uint)vertices.Count;

                vertices.Add(new SvgVertex(p0 + normal, color0));
                vertices.Add(new SvgVertex(p0 - normal, color0));
                vertices.Add(new SvgVertex(p1 + normal, color1));
                vertices.Add(new SvgVertex(p1 - normal, color1));

                // Deux triangles pour le quad du segment
                indices.Add(baseIndex);
                indices.Add(baseIndex + 1);
                indices.Add(baseIndex + 2);
                indices.Add(baseIndex + 1);
                indices.Add(baseIndex + 3);
                indices.Add(baseIndex + 2);
            }
        }

        if (vertices.Count == 0)
            return null;

        return new SvgMeshData([.. vertices], [.. indices]);
    }

    /// <summary>
    /// Resout la couleur d'un point en fonction du PaintServer (solide, degrade lineaire, degrade radial).
    /// </summary>
    private static Vector4 ResolveColor(SvgPaintServer? paintServer, Vector2 point, (Vector2 Min, Vector2 Max) bounds, float opacity)
    {
        switch (paintServer)
        {
            case SvgColourServer colorServer:
                return ColorToVector4(colorServer.Colour, opacity);

            case SvgLinearGradientServer linear:
                return ResolveLinearGradient(linear, point, bounds, opacity);

            case SvgRadialGradientServer radial:
                return ResolveRadialGradient(radial, point, bounds, opacity);

            default:
                // Couleur noire par defaut
                return new Vector4(0, 0, 0, opacity);
        }
    }

    private static Vector4 ResolveLinearGradient(SvgLinearGradientServer gradient, Vector2 point, (Vector2 Min, Vector2 Max) bounds, float opacity)
    {
        var size = bounds.Max - bounds.Min;
        if (size.X == 0) size.X = 1;
        if (size.Y == 0) size.Y = 1;

        // Coordonnees normalisees du degrade
        float x1 = gradient.X1.Value;
        float y1 = gradient.Y1.Value;
        float x2 = gradient.X2.Value;
        float y2 = gradient.Y2.Value;

        // Si les unites sont en pourcentage (objectBoundingBox), convertir en coordonnees absolues
        if (gradient.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
        {
            x1 = bounds.Min.X + x1 * size.X;
            y1 = bounds.Min.Y + y1 * size.Y;
            x2 = bounds.Min.X + x2 * size.X;
            y2 = bounds.Min.Y + y2 * size.Y;
        }

        var gradientDir = new Vector2(x2 - x1, y2 - y1);
        float gradientLength = gradientDir.Length();
        if (gradientLength < 0.001f)
            return SampleGradientStops(gradient.Stops, 0, opacity);

        // Projeter le point sur l'axe du degrade
        float t = Vector2.Dot(point - new Vector2(x1, y1), gradientDir) / (gradientLength * gradientLength);
        t = Math.Clamp(t, 0, 1);

        return SampleGradientStops(gradient.Stops, t, opacity);
    }

    private static Vector4 ResolveRadialGradient(SvgRadialGradientServer gradient, Vector2 point, (Vector2 Min, Vector2 Max) bounds, float opacity)
    {
        var size = bounds.Max - bounds.Min;
        if (size.X == 0) size.X = 1;
        if (size.Y == 0) size.Y = 1;

        float cx = gradient.CenterX.Value;
        float cy = gradient.CenterY.Value;
        float r = gradient.Radius.Value;

        if (gradient.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
        {
            cx = bounds.Min.X + cx * size.X;
            cy = bounds.Min.Y + cy * size.Y;
            r = r * Math.Max(size.X, size.Y);
        }

        if (r < 0.001f)
            return SampleGradientStops(gradient.Stops, 1, opacity);

        float dist = Vector2.Distance(point, new Vector2(cx, cy));
        float t = Math.Clamp(dist / r, 0, 1);

        return SampleGradientStops(gradient.Stops, t, opacity);
    }

    private static Vector4 SampleGradientStops(List<SvgGradientStop> stops, float t, float opacity)
    {
        if (stops.Count == 0)
            return new Vector4(0, 0, 0, opacity);

        if (stops.Count == 1)
            return ColorToVector4(GetStopColor(stops[0]), opacity * stops[0].Opacity);

        // Trouver les deux stops encadrant t
        for (int i = 0; i < stops.Count - 1; i++)
        {
            float offset0 = stops[i].Offset.Value;
            float offset1 = stops[i + 1].Offset.Value;

            if (t <= offset1 || i == stops.Count - 2)
            {
                float range = offset1 - offset0;
                float localT = range > 0.001f ? (t - offset0) / range : 0;
                localT = Math.Clamp(localT, 0, 1);

                var c0 = ColorToVector4(GetStopColor(stops[i]), opacity * stops[i].Opacity);
                var c1 = ColorToVector4(GetStopColor(stops[i + 1]), opacity * stops[i + 1].Opacity);

                return Vector4.Lerp(c0, c1, localT);
            }
        }

        return ColorToVector4(GetStopColor(stops[^1]), opacity * stops[^1].Opacity);
    }

    private static Color GetStopColor(SvgGradientStop stop)
    {
        return stop.StopColor is SvgColourServer colorServer
            ? colorServer.Colour
            : Color.Black;
    }

    private static (Vector2 Min, Vector2 Max) ComputeBounds(List<List<Vector2>> contours)
    {
        var min = new Vector2(float.MaxValue);
        var max = new Vector2(float.MinValue);

        foreach (var contour in contours)
        {
            foreach (var point in contour)
            {
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
        }

        return (min, max);
    }

    private static Vector4 ColorToVector4(Color color, float opacity)
    {
        return new Vector4(
            color.R / 255f,
            color.G / 255f,
            color.B / 255f,
            (color.A / 255f) * opacity
        );
    }

    private static Vector2 ToVector2(System.Drawing.PointF point)
    {
        return new Vector2(point.X, point.Y);
    }

    private static List<Vector2> GenerateEllipseContour(float cx, float cy, float rx, float ry)
    {
        int segments = Math.Max(CurveSegments, (int)(MathF.Max(rx, ry) * 2));
        var points = new List<Vector2>(segments);

        for (int i = 0; i < segments; i++)
        {
            float angle = MathF.Tau * i / segments;
            points.Add(new Vector2(
                cx + rx * MathF.Cos(angle),
                cy + ry * MathF.Sin(angle)
            ));
        }

        return points;
    }

    private static List<Vector2> FlattenCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int segments)
    {
        var points = new List<Vector2>(segments);
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float u = 1 - t;
            var point = u * u * u * p0
                      + 3 * u * u * t * p1
                      + 3 * u * t * t * p2
                      + t * t * t * p3;
            points.Add(point);
        }
        return points;
    }

    private static List<Vector2> FlattenQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, int segments)
    {
        var points = new List<Vector2>(segments);
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float u = 1 - t;
            var point = u * u * p0
                      + 2 * u * t * p1
                      + t * t * p2;
            points.Add(point);
        }
        return points;
    }

    private static List<Vector2> FlattenArc(Vector2 currentPos, SvgArcSegment arc)
    {
        // Approximation de l'arc par des segments de courbe
        var end = ToVector2(arc.End);
        float rx = arc.RadiusX;
        float ry = arc.RadiusY;

        if (rx < 0.001f || ry < 0.001f)
            return [end];

        // Approximation simplifiee : interpolation sur l'arc
        int segments = CurveSegments;
        var points = new List<Vector2>(segments);

        // Calculer le centre et les angles de l'arc via la parametrisation endpoint->center
        var (center, startAngle, sweepAngle) = EndpointToCenterArc(
            currentPos, end, rx, ry, arc.Angle, arc.Size == SvgArcSize.Large, arc.Sweep == SvgArcSweep.Positive);

        float cosRot = MathF.Cos(arc.Angle * MathF.PI / 180f);
        float sinRot = MathF.Sin(arc.Angle * MathF.PI / 180f);

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = startAngle + sweepAngle * t;

            float x = rx * MathF.Cos(angle);
            float y = ry * MathF.Sin(angle);

            // Appliquer la rotation
            float xr = x * cosRot - y * sinRot + center.X;
            float yr = x * sinRot + y * cosRot + center.Y;

            points.Add(new Vector2(xr, yr));
        }

        return points;
    }

    private static (Vector2 Center, float StartAngle, float SweepAngle) EndpointToCenterArc(
        Vector2 p1, Vector2 p2, float rx, float ry, float rotationDeg, bool largeArc, bool sweepPositive)
    {
        float phi = rotationDeg * MathF.PI / 180f;
        float cosPhi = MathF.Cos(phi);
        float sinPhi = MathF.Sin(phi);

        float dx = (p1.X - p2.X) / 2f;
        float dy = (p1.Y - p2.Y) / 2f;

        float x1p = cosPhi * dx + sinPhi * dy;
        float y1p = -sinPhi * dx + cosPhi * dy;

        float x1pSq = x1p * x1p;
        float y1pSq = y1p * y1p;
        float rxSq = rx * rx;
        float rySq = ry * ry;

        // Corriger les rayons si necessaire
        float lambda = x1pSq / rxSq + y1pSq / rySq;
        if (lambda > 1)
        {
            float sqrtLambda = MathF.Sqrt(lambda);
            rx *= sqrtLambda;
            ry *= sqrtLambda;
            rxSq = rx * rx;
            rySq = ry * ry;
        }

        float num = rxSq * rySq - rxSq * y1pSq - rySq * x1pSq;
        float den = rxSq * y1pSq + rySq * x1pSq;

        float sq = MathF.Max(0, num / den);
        float root = MathF.Sqrt(sq);
        if (largeArc == sweepPositive)
            root = -root;

        float cxp = root * rx * y1p / ry;
        float cyp = -root * ry * x1p / rx;

        float cx = cosPhi * cxp - sinPhi * cyp + (p1.X + p2.X) / 2f;
        float cy = sinPhi * cxp + cosPhi * cyp + (p1.Y + p2.Y) / 2f;

        float startAngle = MathF.Atan2((y1p - cyp) / ry, (x1p - cxp) / rx);
        float endAngle = MathF.Atan2((-y1p - cyp) / ry, (-x1p - cxp) / rx);

        float sweepAngle = endAngle - startAngle;

        if (sweepPositive && sweepAngle < 0)
            sweepAngle += MathF.Tau;
        else if (!sweepPositive && sweepAngle > 0)
            sweepAngle -= MathF.Tau;

        return (new Vector2(cx, cy), startAngle, sweepAngle);
    }
}
