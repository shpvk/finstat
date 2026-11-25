using Finstat.Desktop.Core.ViewModels;

namespace Finstat.Desktop.Controls;

public class DonutChart : GraphicsView, IDrawable
{
    public static readonly BindableProperty SegmentsProperty = BindableProperty.Create(
        nameof(Segments), typeof(List<ChartSegment>), typeof(DonutChart), new List<ChartSegment>(), propertyChanged: OnChartChanged);

    public static readonly BindableProperty ActiveSegmentProperty = BindableProperty.Create(
        nameof(ActiveSegment), typeof(string), typeof(DonutChart), null, BindingMode.TwoWay, propertyChanged: OnChartChanged);

    private const float ViewBox = 200f;
    private const float RingRadius = 80f;
    private const float StrokeWidth = 22f;
    private const float GapDegrees = 1.44f;

    public DonutChart()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
        var pointer = new PointerGestureRecognizer();
        pointer.PointerMoved += (sender, args) => UpdateActive(args.GetPosition(this));
        pointer.PointerExited += (sender, args) => ActiveSegment = null;
        GestureRecognizers.Add(pointer);
    }

    public List<ChartSegment> Segments
    {
        get => (List<ChartSegment>)GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    public string? ActiveSegment
    {
        get => (string?)GetValue(ActiveSegmentProperty);
        set => SetValue(ActiveSegmentProperty, value);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var scale = Math.Min(dirtyRect.Width, dirtyRect.Height) / ViewBox;
        var radius = RingRadius * scale;
        var centerX = dirtyRect.Center.X;
        var centerY = dirtyRect.Center.Y;
        var total = TotalTenths();
        if (total == 0)
            return;

        canvas.StrokeSize = StrokeWidth * scale;
        canvas.StrokeLineCap = LineCap.Butt;

        var start = 90f;
        foreach (var segment in Segments)
        {
            var sweep = 360f * segment.Tenths / total;
            var visible = sweep;
            if (Segments.Count > 1)
                visible = Math.Max(sweep - GapDegrees, 1.2f);

            var color = ChartColorConverter.ColorAt(segment.ColorIndex);
            if (ActiveSegment is not null && ActiveSegment != segment.Key)
                color = color.WithAlpha(0.3f);
            canvas.StrokeColor = color;

            if (visible >= 359.9f)
                canvas.DrawCircle(centerX, centerY, radius);
            else
                canvas.DrawArc(centerX - radius, centerY - radius, radius * 2, radius * 2, start, start - visible, true, false);
            start -= sweep;
        }
    }

    private int TotalTenths()
    {
        var total = 0;
        foreach (var segment in Segments)
            total += segment.Tenths;
        return total;
    }

    private void UpdateActive(Point? position)
    {
        if (position is null || Width <= 0)
            return;
        var dx = position.Value.X - Width / 2;
        var dy = position.Value.Y - Height / 2;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        var scale = Math.Min(Width, Height) / ViewBox;
        var radius = RingRadius * scale;
        var halfStroke = StrokeWidth * scale / 2;
        if (distance < radius - halfStroke - 4 || distance > radius + halfStroke + 4)
        {
            ActiveSegment = null;
            return;
        }

        var angle = Math.Atan2(dx, -dy) * 180 / Math.PI;
        if (angle < 0)
            angle += 360;
        var total = TotalTenths();
        var passed = 0.0;
        foreach (var segment in Segments)
        {
            passed += 360.0 * segment.Tenths / total;
            if (angle <= passed)
            {
                ActiveSegment = segment.Key;
                return;
            }
        }
        ActiveSegment = null;
    }

    private static void OnChartChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((DonutChart)bindable).Invalidate();
    }
}
