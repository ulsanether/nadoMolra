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
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using DryIoc;
using System.Collections.Generic;

namespace Mvvm.ViewModels
{
    public class HomePageViewModel : BindableBase, IDropTarget, IDragSource
    {


        private CancellationTokenSource _cancellationTokenSource;

        private ModbusConnect _modbusConnect;



        public ObservableCollection<Border> Borders1 { get; set; }
        public ObservableCollection<Border> Borders2 { get; set; }
        public ObservableCollection<Border> Borders3 { get; set; }



        



        public HomePageViewModel(ModbusConnect modbusConnect)
        {



            _modbusConnect = modbusConnect;
            _modbusConnect.ConnectionStatusChanged += ModbusConnect_OnConnectionsStatusChanged;



            int startAddress = Properties.Settings.Default.StartAddress; // Settings에서 시작 주소 가져오기
            int endAddress = Properties.Settings.Default.EndAddress; // Settings에서 끝 주소 가져오기
            int numberOfPoints = endAddress - startAddress + 1; // 읽어올 포인트 수 계산

            Borders1 = new ObservableCollection<Border>();
            Borders2 = new ObservableCollection<Border>();
            Borders3 = new ObservableCollection<Border>();

            for (int i = 0; i < numberOfPoints; i++)
            {
                var border = new Border
                {
                    Width = 120,
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(242, 242, 242)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(192, 169, 186)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    IsEnabled = true,
                    Child = new Label
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Content = $"Add {i + 1}",
                        FontFamily = new FontFamily("Yu Gothic UI Semibold"),
                        Foreground = (Brush)Application.Current.Resources["MainFontColor"]
                    }
                };
                Borders1.Add(border);
            }

        }




        private void ModbusConnect_OnConnectionsStatusChanged(bool isConnected)
        {
            if (isConnected)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Run(async () => await ReadDataPeriodically(_cancellationTokenSource.Token));
            }
            else
            {
                _cancellationTokenSource?.Cancel();
            }
        }

        List<ParameterModel> parameters = new List<ParameterModel>();

        private async Task ReadDataPeriodically(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int startAddress = Properties.Settings.Default.StartAddress; // Settings에서 시작 주소 가져오기
                    int endAddress = Properties.Settings.Default.EndAddress; // Settings에서 끝 주소 가져오기
                    int numberOfPoints = endAddress - startAddress + 1; // 읽어올 포인트 수 계산
                     parameters = await _modbusConnect.ReadModbusData(startAddress, numberOfPoints);
                    _modbusConnect.dataBuffer.StoreValues(parameters);
          
                    foreach (var parameter in parameters)
                    {
                        Debug.WriteLine($"Address: {parameter.Address}, Value: {parameter.DefaultActual}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"데이터 읽기 중 오류가 발생했습니다: {ex.Message}");
                }

                await Task.Delay(2000, cancellationToken);
            }
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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        if (i < Borders1.Count)
                        {
                            var border = Borders1[i];
                            var label = border.Child as Label;
                            if (label != null)
                            {
                                label.Content = $"Address: {parameters[i].Address}, Value: {parameters[i].DefaultActual}";
                            }
                        }
                    }
                });

                if (sourceCollection == Borders1)
                {
                //    MessageBox.Show("Border1이 이동되었습니다.");
                }
                else if (sourceCollection == Borders2)
                {
                  //  MessageBox.Show("Border2가 이동되었습니다.");
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

                            if (targetCollection == Borders1)
                            {
                                // Borders1로 이동할 때 크기 조정
                                sourceItem.Width = 100;
                                sourceItem.Height = 30;
                            }

                            else if (targetCollection == Borders2)
                            {
                                sourceItem.Width = 50;
                                sourceItem.Height = 30;
                            }

                            else if (targetCollection == Borders3)
                            {
                           
                                sourceItem.Width = 100; // 적절한 가로 크기
                                sourceItem.Height = 100; // 적절한 세로 크기

                                // 짝수/홀수 인덱스에 따라 다른 여백 적용 (바둑판 형태)
                                int row = targetIndex / 2; // 행 계산
                                int column = targetIndex % 2; // 열 계산

                           
                                sourceItem.Margin = new Thickness(
                                    column * 5, // 왼쪽 여백 (열에 따라 조정)
                                    row * 5,    // 상단 여백 (행에 따라 조정)
                                    5,          // 오른쪽 여백
                                    5           // 하단 여백
                                );
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
                                if (borderCollection == Borders3)
                                {
                                    // 바둑판 형식으로 배치하기 위한 사이즈 및 여백 조정
                                    sourceItem.Width = 100; // 적절한 가로 크기
                                    sourceItem.Height = 100; // 적절한 세로 크기

                                    // 짝수/홀수 인덱스에 따라 다른 여백 적용 (바둑판 형태)
                                    int row = dropInfo.InsertIndex / 2; // 행 계산
                                    int column = dropInfo.InsertIndex % 2; // 열 계산

                                    // 바둑판 형태로 여백 조정
                                    sourceItem.Margin = new Thickness(
                                        column * 5, // 왼쪽 여백 (열에 따라 조정)
                                        row * 5,    // 상단 여백 (행에 따라 조정)
                                        5,          // 오른쪽 여백
                                        5           // 하단 여백
                                    );
                                }
                                else if (borderCollection == Borders1)
                                {
                                    // Borders1로 이동할 때 크기 조정
                                    sourceItem.Width = 120;
                                    sourceItem.Height = 50;
                                }

                                borderCollection.Insert(dropInfo.InsertIndex, sourceItem);
                            }
                            else
                            {
                                if (borderCollection == Borders3)
                                {
                                    // 바둑판 형식으로 배치하기 위한 사이즈 및 여백 조정
                                    sourceItem.Width = 270; // 적절한 가로 크기
                                    sourceItem.Height = 300; // 적절한 세로 크기

                                    // 마지막 아이템의 경우 인덱스 계산
                                    int index = borderCollection.Count;
                                    int row = index / 2; // 행 계산
                                    int column = index % 2; // 열 계산

                                    // 바둑판 형태로 여백 조정
                                    sourceItem.Margin = new Thickness(
                                        column * 5, // 왼쪽 여백 (열에 따라 조정)
                                        row * 5,    // 상단 여백 (행에 따라 조정)
                                        5,          // 오른쪽 여백
                                        5           // 하단 여백
                                    );
                                }
                                else if (borderCollection == Borders1)
                                {
                                    // Borders1로 이동할 때 크기 조정
                                    sourceItem.Width = 120;
                                    sourceItem.Height = 50;
                                }

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
