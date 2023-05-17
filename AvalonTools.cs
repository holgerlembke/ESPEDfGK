using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit;
using System.Windows.Media;
using System.Windows;

namespace ESPEDfGK
{
    public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
    {
        private TextEditor _editor;
        public int LineNumber;

        public HighlightCurrentLineBackgroundRenderer(TextEditor editor)
        {
            _editor = editor;
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Selection; }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_editor.Document == null)
                return;

            textView.EnsureVisualLines();

            if (LineNumber > 0)
            {
                Brush highlight = Brushes.LightGreen; 
                var currentLine = _editor.Document.GetLineByNumber(LineNumber);
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
                {
                    drawingContext.DrawRectangle(highlight,
                        null, new Rect(rect.Location, new Size(rect.Width, rect.Height)));
                }
            }
        }
    }
}
