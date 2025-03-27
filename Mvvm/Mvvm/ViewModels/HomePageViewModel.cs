using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mvvm.ViewModels
{
    public class HomePageViewModel : IDropTarget
    {
        public ObservableCollection<Border> Borders { get; set; }

        public HomePageViewModel()
        {
            Borders = new ObservableCollection<Border>
            {
                new Border
                {
                    Width = 100,
                    Height = 100,
                    Margin = new Thickness(25, 60, 218, 161),
                    Background = new SolidColorBrush(Color.FromRgb(242, 242, 242)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(192, 169, 186)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    IsEnabled = false,
                    Child = new Label
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Content = "Label",
                        FontFamily = new FontFamily("Yu Gothic UI Semibold"),
                        Foreground = (Brush)Application.Current.Resources["MainFontColor"]
                    }
                },
                new Border
                {
                    Width = 100,
                    Height = 100,
                    Margin = new Thickness(172, 60, 71, 161),
                    Background = new SolidColorBrush(Color.FromRgb(242, 242, 242)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(192, 169, 186)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    IsEnabled = false,
                    Child = new Label
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Content = "Label",
                        FontFamily = new FontFamily("Yu Gothic UI Semibold"),
                        Foreground = (Brush)Application.Current.Resources["MainFontColor"]
                    }
                },
                new Border
                {
                    Height = 100,
                    Margin = new Thickness(25, 185, 71, 36),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(192, 169, 186)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    IsEnabled = false,
                    Background = new LinearGradientBrush
                    {
                        StartPoint = new Point(0.5, 0),
                        EndPoint = new Point(0.5, 1),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(Color.FromRgb(199, 147, 247), 0),
                            new GradientStop(Color.FromRgb(75, 179, 216), 1)
                        }
                    },
                    Child = new Label
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Content = "Label",
                        FontFamily = new FontFamily("Yu Gothic UI Semibold"),
                        Foreground = (Brush)Application.Current.Resources["SubOneFontColor"]
                    }
                }
            };
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Border && dropInfo.TargetItem is Border)
            {
                dropInfo.Effects = DragDropEffects.Move;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Border sourceItem && dropInfo.TargetItem is Border targetItem)
            {
                var sourceIndex = Borders.IndexOf(sourceItem);
                var targetIndex = Borders.IndexOf(targetItem);

                if (sourceIndex != -1 && targetIndex != -1)
                {
                    Borders.Move(sourceIndex, targetIndex);
                }
            }
        }
    }
}
