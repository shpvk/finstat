using Finstat.Desktop.Core.ViewModels;

namespace Finstat.Desktop.Controls;

public class DonutChart : GraphicsView, IDrawable
{
    public static readonly BindableProperty SegmentsProperty = BindableProperty.Create(
        nameof(Segments), typeof(List<ChartSegment>), typeof(DonutChart), new List<ChartSegment>(), propertyChanged: OnSegmentsChanged);

    public static readonly BindableProperty ActiveSegmentProperty = BindableProperty.Create(
        nameof(ActiveSegment), typeof(string), typeof(DonutChart), null, BindingMode.TwoWay, propertyChanged: OnActiveChanged);

    private const float ViewBox = 200f;
    private const float RingRadius = 80f;
    private const float StrokeWidth = 22f;
    private const float GrowWidth = 3f;
    private const float GapDegrees = 1.44f;

    private readonly Dictionary<string, float> dimmed = new();
    private readonly Dictionary<string, float> grown = new();
    private readonly Dictionary<string, float> dimmedFrom = new();
    private readonly Dictionary<string, float> grownFrom = new();
    private float reveal = 1f;

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

        canvas.StrokeLineCap = LineCap.Butt;

        var limit = 360f * reveal;
        var start = 90f;
        var done = 0f;
        foreach (var segment in Segments)
        {
            var sweep = 360f * segment.Tenths / total;
            var visible = sweep;
            if (Segments.Count > 1)
                visible = Math.Max(sweep - GapDegrees, 1.2f);
            var shown = Math.Min(visible, limit - done);
            done += sweep;

            var drawStart = start;
            start -= sweep;
            if (shown <= 0f)
                continue;

            var color = ChartColorConverter.ColorAt(segment.ColorIndex);
            color = color.WithAlpha(1f - 0.7f * Get(dimmed, segment.Key));
            canvas.StrokeColor = color;
            canvas.StrokeSize = (StrokeWidth + GrowWidth * Get(grown, segment.Key)) * scale;

            if (visible >= 359.9f && shown >= 359.9f)
                canvas.DrawCircle(centerX, centerY, radius);
            else
                canvas.DrawArc(centerX - radius, centerY - radius, radius * 2, radius * 2, drawStart, drawStart - shown, true, false);
        }
    }

    private static float Get(Dictionary<string, float> values, string key)
    {
        return values.TryGetValue(key, out var value) ? value : 0f;
    }

    private void AnimateFocus()
    {
        this.AbortAnimation("focus");
        foreach (var segment in Segments)
        {
            dimmedFrom[segment.Key] = Get(dimmed, segment.Key);
            grownFrom[segment.Key] = Get(grown, segment.Key);
        }

        if (!Motion.Enabled)
        {
            ApplyFocus(1);
            return;
        }

        new Animation(ApplyFocus, 0, 1, Easing.CubicOut).Commit(this, "focus", 16, Motion.Fast);
    }

    private void ApplyFocus(double progress)
    {
        var t = (float)progress;
        foreach (var segment in Segments)
        {
            var dimTarget = ActiveSegment is not null && ActiveSegment != segment.Key ? 1f : 0f;
            var growTarget = ActiveSegment == segment.Key ? 1f : 0f;
            dimmed[segment.Key] = dimmedFrom[segment.Key] + (dimTarget - dimmedFrom[segment.Key]) * t;
            grown[segment.Key] = grownFrom[segment.Key] + (growTarget - grownFrom[segment.Key]) * t;
        }
        Invalidate();
    }

    public void Reveal()
    {
        this.AbortAnimation("reveal");
        this.AbortAnimation("focus");
        dimmed.Clear();
        grown.Clear();
        if (!Motion.Enabled)
        {
            reveal = 1f;
            Invalidate();
            return;
        }

        reveal = 0f;
        new Animation(t =>
        {
            reveal = (float)t;
            Invalidate();
        }, 0, 1, Easing.CubicOut).Commit(this, "reveal", 16, Motion.Ring);
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

    private static void OnSegmentsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((DonutChart)bindable).Invalidate();
    }

    private static void OnActiveChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((DonutChart)bindable).AnimateFocus();
    }
}
