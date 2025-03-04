﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utilizr.WPF.Util;

namespace Utilizr.WPF.Controls
{
    public class Bitmap : FrameworkElement
    {
        public Bitmap()
        {
            _sourceDownloaded = new EventHandler(OnSourceDownloaded);
            _sourceFailed = new EventHandler<ExceptionEventArgs>(OnSourceFailed);

            LayoutUpdated += new EventHandler(OnLayoutUpdated);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(ImageSource),
            typeof(Bitmap),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsParentArrange |
                FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                new PropertyChangedCallback(Bitmap.OnSourceChanged)));

        public ImageSource Source
        {
            get
            {
                return (ImageSource)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        public static readonly DependencyProperty DebugModeProperty = DependencyProperty.Register(
            "DebugMode", typeof(bool), typeof(Bitmap), new PropertyMetadata(default(bool)));

        public bool DebugMode
        {
            get { return (bool) GetValue(DebugModeProperty); }
            set { SetValue(DebugModeProperty, value); }
        }

        
        public event EventHandler<ExceptionEventArgs> BitmapFailed;

        private void DebugPrint(string message)
        {
#if DEBUG
            if (!DebugMode)
                return;
            
            Debug.WriteLine(message);
#endif
        }

        // Return our measure size to be the size needed to display the bitmap pixels.
        protected override Size MeasureOverride(Size availableSize)
        {

            Size measureSize = new Size();

            BitmapSource bitmapSource = (BitmapSource) Source;
            if (bitmapSource != null)
            {
                measureSize = new Size(
                    bitmapSource.Width,
                    bitmapSource.Height
                );
            }

            DebugPrint($"measureSize={measureSize}");

            return measureSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            BitmapSource bitmapSource = (BitmapSource) this.Source;

            //TODO: removing this null check causes weird layout issues, however having the null check causes high cpu usage if no Source is set.. need to solve the root issue :/
            if (bitmapSource != null)
            {
                _pixelOffset = GetPixelOffset();
                var size = new Size(
                    ActualWidth,
                    ActualHeight
                    );
                // Render the bitmap offset by the needed amount to align to pixels.
                DebugPrint($"drawing bitmap at pos:[{_pixelOffset}] with size:[{size}]");
                dc.DrawImage(bitmapSource, new Rect(_pixelOffset, size));
            }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Bitmap bitmap))
                return;

            if (e.OldValue is BitmapSource oldValue && bitmap._sourceDownloaded != null && !oldValue.IsFrozen)
            {
                oldValue.DownloadCompleted -= bitmap._sourceDownloaded;
                oldValue.DownloadFailed -= bitmap._sourceFailed;
                //newValue.DecodeFailed -= bitmap._sourceFailed; // 3.5
            }
            if (e.NewValue is BitmapSource newValue && !newValue.IsFrozen)
            {
                newValue.DownloadCompleted += bitmap._sourceDownloaded;
                newValue.DownloadFailed += bitmap._sourceFailed;
                //newValue.DecodeFailed += bitmap._sourceFailed; // 3.5
            }
        }

        private void OnSourceDownloaded(object sender, EventArgs e)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }

        private void OnSourceFailed(object sender, ExceptionEventArgs e)
        {
            Source = null; // setting a local value seems scetchy...

            BitmapFailed(this, e);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            // This event just means that layout happened somewhere.  However, this is
            // what we need since layout anywhere could affect our pixel positioning.
            Point pixelOffset = GetPixelOffset();
            if (!AreClose(pixelOffset, _pixelOffset))
            {
                InvalidateVisual();
            }
        }

        // Gets the matrix that will convert a point from "above" the
        // coordinate space of a visual into the the coordinate space
        // "below" the visual.
        private Matrix GetVisualTransform(Visual v)
        {
            if (v != null)
            {
                Matrix m = Matrix.Identity;

                Transform transform = VisualTreeHelper.GetTransform(v);
                if (transform != null)
                {
                    Matrix cm = transform.Value;
                    m = Matrix.Multiply(m, cm);
                }

                Vector offset = VisualTreeHelper.GetOffset(v);
                m.Translate(offset.X, offset.Y);

                return m;
            }

            return Matrix.Identity;
        }

        private Point TryApplyVisualTransform(Point point, Visual v, bool inverse, bool throwOnError, out bool success)
        {
            success = true;
            if (v != null)
            {
                Matrix visualTransform = GetVisualTransform(v);
                if (inverse)
                {
                    if (!throwOnError && !visualTransform.HasInverse)
                    {
                        success = false;
                        return new Point(0, 0);
                    }
                    visualTransform.Invert();
                }
                point = visualTransform.Transform(point);
            }
            return point;
        }

        private Point ApplyVisualTransform(Point point, Visual v, bool inverse)
        {
            return TryApplyVisualTransform(point, v, inverse, true, out bool _);
        }

        private bool? _layoutRounding;

        private Point GetPixelOffset()
        {
            Point pixelOffset = new Point();
            return pixelOffset;
            //if (!_layoutRounding.HasValue)
            //{
            //    _layoutRounding = SafeSetEx.GetLayoutRounding(this);
            //}

            //if (_layoutRounding.Value)
            //{
            //    return pixelOffset;
            //}

#if DEBUG
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return pixelOffset;
#endif


            try
            {
                var ps = PresentationSource.FromVisual(this);
                if (ps != null)
                {
                    Visual rootVisual = ps.RootVisual;

                    // Transform (0,0) from this element up to pixels.
                    pixelOffset = this.TransformToAncestor(rootVisual).Transform(pixelOffset);
                    pixelOffset = ApplyVisualTransform(pixelOffset, rootVisual, false);
                    pixelOffset = ps.CompositionTarget.TransformToDevice.Transform(pixelOffset);

                    // Round the origin to the nearest whole pixel.
                    pixelOffset.X = Math.Round(pixelOffset.X);
                    pixelOffset.Y = Math.Round(pixelOffset.Y);

                    // Transform the whole-pixel back to this element.
                    pixelOffset = ps.CompositionTarget.TransformFromDevice.Transform(pixelOffset);
                    pixelOffset = ApplyVisualTransform(pixelOffset, rootVisual, true);

                    var descendant = rootVisual.TransformToDescendant(this);
                    if (descendant == null && (Math.Abs(pixelOffset.X) > 1 || Math.Abs(pixelOffset.Y) > 1))
                    {
                        pixelOffset = new Point();
                    }
                    else
                    {
                        pixelOffset = descendant.Transform(pixelOffset);
                    }
                }
            }
            catch
            {

            }            

            return pixelOffset;
        }

        private bool AreClose(Point point1, Point point2)
        {
            return AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y);
        }

        private bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
            {
                return true;
            }
            double delta = value1 - value2;
            return ((delta < 1.53E-06) && (delta > -1.53E-06));
        }

        private EventHandler _sourceDownloaded;
        private EventHandler<ExceptionEventArgs> _sourceFailed;
        private Point _pixelOffset;
    }
}
