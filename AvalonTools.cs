using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace ESPEDfGK
{
    public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
    {
        private TextEditor _editor;
        // public List<DiffPiece> Lines { get; set; } /* DiffPlex model's lines */
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
                Brush highlight = Brushes.LightGreen; //Brushes.Transparent;
                var currentLine = _editor.Document.GetLineByNumber(LineNumber);
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
                {
                    drawingContext.DrawRectangle(highlight,
                        null, new Rect(rect.Location, new Size(rect.Width, rect.Height)));
                }
            }

            /*
            for (var i = 0; i < Lines.Count; i++)
            {
                var diffLine = Lines[i];
                if (!string.IsNullOrEmpty(diffLine.Text))
                {
                    var highlight = GetLineBackgroundBrush(diffLine);
                    var currentLine = _editor.Document.GetLineByNumber(i + 1);
                    foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
                    {
                        drawingContext.DrawRectangle(highlight, null, new Rect(rect.Location, new Size(rect.Width, rect.Height)));
                    }
                }
            }
            */
        }

        /*
        private Brush GetLineBackgroundBrush(DiffPiece line)
        {
            var highlight = Brushes.Transparent;
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    highlight = Brushes.Green;
                    break;
                case ChangeType.Deleted:
                    highlight = Brushes.Red;
                    break;
                case ChangeType.Modified:
                    highlight = Brushes.Yellow;
                    break;
            }

            return highlight;
        }
        */

    }
}