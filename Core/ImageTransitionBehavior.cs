using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace PonyvilleSchool2._0.Core
{
    public enum ImageTransitionKind
    {
        FadeOutIn,
        Crossfade,
        Instant
    }
    public class ImageTransitionBehavior : Behavior<Image>
    {
        public static readonly DependencyProperty BoundSourceProperty =
           DependencyProperty.Register(nameof(BoundSource), typeof(string), typeof(ImageTransitionBehavior),
               new PropertyMetadata(null, OnBoundSourceChanged));

        public string BoundSource
        {
            get => (string)GetValue(BoundSourceProperty);
            set => SetValue(BoundSourceProperty, value);
        }

        public ImageTransitionKind TransitionKind { get; set; } = ImageTransitionKind.FadeOutIn;
        public double DurationSeconds { get; set; } = 0.3;

        private static void OnBoundSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ImageTransitionBehavior)?.AnimateTransition((string)e.NewValue);
        }

        private void AnimateTransition(string newSource)
        {
            if (AssociatedObject == null) return;

            // Если это первая загрузка (Source ещё не задан) — просто устанавливаем картинку без анимации,
            // чтобы избежать артефактов с позиционированием.
            if (AssociatedObject.Source == null && TransitionKind != ImageTransitionKind.Instant)
            {
                SetImageSource(newSource);
                return;
            }

            switch (TransitionKind)
            {
                case ImageTransitionKind.Instant:
                    SetImageSource(newSource);
                    break;
                case ImageTransitionKind.FadeOutIn:
                    FadeOutInTransition(newSource);
                    break;
                case ImageTransitionKind.Crossfade:
                    CrossfadeTransition(newSource);
                    break;
            }
        }

        private void SetImageSource(string source)
        {
            AssociatedObject.Source = string.IsNullOrEmpty(source) ? null :
                new BitmapImage(new Uri(source, UriKind.RelativeOrAbsolute));
        }

        private void FadeOutInTransition(string newSource)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(DurationSeconds))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (_, _) =>
            {
                SetImageSource(newSource);
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(DurationSeconds))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                AssociatedObject.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            AssociatedObject.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void CrossfadeTransition(string newSource)
        {
            var parent = AssociatedObject.Parent as Panel;
            if (parent == null)
            {
                SetImageSource(newSource);
                return;
            }

            // Создаём временное изображение с теми же параметрами расположения, что и у основного
            var tempImage = new Image
            {
                Source = new BitmapImage(new Uri(newSource, UriKind.RelativeOrAbsolute)),
                Stretch = AssociatedObject.Stretch,
                Opacity = 0,
                // Копируем все свойства, влияющие на положение и размер
                HorizontalAlignment = AssociatedObject.HorizontalAlignment,
                VerticalAlignment = AssociatedObject.VerticalAlignment,
                Margin = AssociatedObject.Margin,
                // Если ширина/высота заданы явно (не NaN), копируем их
                Width = double.IsNaN(AssociatedObject.Width) ? double.NaN : AssociatedObject.Width,
                Height = double.IsNaN(AssociatedObject.Height) ? double.NaN : AssociatedObject.Height
            };

            // Дополнительно можно скопировать RenderTransform, если используется
            // Также нужно учесть возможный параметр SnapsToDevicePixels
            tempImage.SnapsToDevicePixels = AssociatedObject.SnapsToDevicePixels;

            parent.Children.Add(tempImage);
            // Размещаем временное изображение над основным (больший ZIndex)
            Panel.SetZIndex(tempImage, Panel.GetZIndex(AssociatedObject) + 1);

            // Анимация появления нового изображения
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(DurationSeconds))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            fadeIn.Completed += (_, _) =>
            {
                // По окончании заменяем источник основного изображения и убираем временное
                SetImageSource(newSource);
                parent.Children.Remove(tempImage);
                AssociatedObject.Opacity = 1;
            };
            tempImage.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }
    }
}
