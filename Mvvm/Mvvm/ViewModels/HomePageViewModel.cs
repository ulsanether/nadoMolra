using GongSolutions.Wpf.DragDrop;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Prism.Mvvm;
using Mvvm.ViewModels;
using Accord;
using Point = System.Windows.Point;
using System;
using System.Linq;
using Mvvm.Model;

namespace Mvvm.ViewModels
{
    public class HomePageViewModel : BindableBase, IDropTarget, IDragSource
    {




        private ModbusConnect _modbusConnect;



        public ObservableCollection<Border> Borders1 { get; set; }
        public ObservableCollection<Border> Borders2 { get; set; }
        public ObservableCollection<Border> Borders3 { get; set; }







        public HomePageViewModel(ModbusConnect modbusConnect)
        {



            _modbusConnect = modbusConnect;

            Borders1 = new ObservableCollection<Border>
            {
                new Border
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(0, 0, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(242, 242, 242)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(192, 169, 186)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    IsEnabled = true,
                    Child = new Label
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Content = "Label 1",
                        FontFamily = new FontFamily("Yu Gothic UI Semibold"),
                        Foreground = (Brush)Application.Current.Resources["MainFontColor"]
                    }
                },
                new Border
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(0, 0, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(242, 242, 242)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(192, 169, 186)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    IsEnabled = true,
                    Child = new Label
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Content = "Label 2",
                        FontFamily = new FontFamily("Yu Gothic UI Semibold"),
                        Foreground = (Brush)Application.Current.Resources["MainFontColor"]
                    }
                }
            };




            Borders2 = new ObservableCollection<Border>();
            Borders3 = new ObservableCollection<Border>();
        }



        #region IDropTarget 구현


        /*
         *
         *
         *•	dropInfo.Data: 현재 드래그 중인 데이터 객체
•	dropInfo.TargetItem: 현재 마우스가 위치한 대상 아이템
•	dropInfo.Effects: 허용할 드래그 앤 드롭 작업 종류
•	dropInfo.DropTargetAdorner: 시각적 표시 방법 설정
         *
         *
         * */

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Border)
            {
                dropInfo.Effects = DragDropEffects.Move;

                if (dropInfo.TargetItem is Border)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;

                }
                else
                {

                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;

                }
            }
        }

        public void Drop(IDropInfo dropInfo)  // dd:DragDrop.DropHandler="{Binding}"
        {
            if (dropInfo.Data is Border sourceItem)
            {
                var sourceCollection = GetCollectionContainingItem(sourceItem);

                if (sourceCollection == null) return;

                // Border1이 이동할 때 메시지 박스 표시
                if (sourceCollection == Borders1)
                {



                }
                else {




                }




                if (dropInfo.TargetItem is Border targetItem)
                {
                    var targetCollection = GetCollectionContainingItem(targetItem);
                    if (targetCollection != null)
                    {
                        int sourceIndex = sourceCollection.IndexOf(sourceItem);
                        int targetIndex = targetCollection.IndexOf(targetItem);

                        if (sourceIndex != -1 && targetIndex != -1)
                        {
                            sourceCollection.RemoveAt(sourceIndex);

                            if (sourceCollection == targetCollection && sourceIndex < targetIndex)
                            {
                                targetIndex--;
                            }

                            targetCollection.Insert(targetIndex, sourceItem);
                        }
                    }
                }
                else if (dropInfo.TargetCollection is IList targetCollection)
                {
                    int sourceIndex = sourceCollection.IndexOf(sourceItem);
                    if (sourceIndex != -1)
                    {
                        sourceCollection.RemoveAt(sourceIndex);

                        if (targetCollection is ObservableCollection<Border> borderCollection)
                        {
                            if (dropInfo.InsertIndex >= 0 && dropInfo.InsertIndex <= borderCollection.Count)
                            {
                                borderCollection.Insert(dropInfo.InsertIndex, sourceItem);
                            }
                            else
                            {
                                borderCollection.Add(sourceItem);
                            }
                        }
                    }
                }
            }
        }

        private ObservableCollection<Border> GetCollectionContainingItem(Border item)
        {
            if (Borders1.Contains(item))
            {
                return Borders1;
            }
            else if (Borders2.Contains(item))
            {
                return Borders2;
            }
            else if (Borders3.Contains(item))
            {
                return Borders3;
            }
            return null;
        }
        #endregion

        #region IDragSource 구현
        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return dragInfo.SourceItem is Border;
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Effects = DragDropEffects.Move;
        }

        public void Dropped(IDropInfo dropInfo)
        {
            // 드롭 완료 후 추가 작업 필요 시 여기에 구현
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
            // 완료 후 추가 처리
        }

        public void DragCancelled()
        {
            // 취소 시 처리
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return false;
        }
        #endregion







    }
}
