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

                            if (i < Borders2.Count)
                            {
                                var border = Borders2[i];
                                var label = border.Child as Label;
                                if (label != null)
                                {
                                    label.Content = $"Address: {parameters[i].Address}, Value: {parameters[i].DefaultActual}";
                                }
                            }

                            if (i < Borders3.Count)
                            {
                                var border = Borders3[i];
                                var label = border.Child as Label;
                                if (label != null)
                                {
                                    label.Content = $"Address: {parameters[i].Address}, Value: {parameters[i].DefaultActual}";
                                }
                            }
                        }
                    });
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


        // HomePageViewModel.cs 파일에 아래 메소드 추가

        public void RemoveDuplicatesFromBorders1()
        {
            // Border1에서 제거할 아이템을 저장할 리스트
            List<Border> itemsToRemove = new List<Border>();

            // 먼저 Border2와 Border3에 있는 모든 주소를 수집
            HashSet<int> existingAddresses = new HashSet<int>();

            // Border2에서 주소 수집
            foreach (var border in Borders2)
            {
                int? address = GetAddressFromBorder(border);
                if (address.HasValue)
                {
                    existingAddresses.Add(address.Value);
                }
            }

            // Border3에서 주소 수집
            foreach (var border in Borders3)
            {
                int? address = GetAddressFromBorder(border);
                if (address.HasValue)
                {
                    existingAddresses.Add(address.Value);
                }
            }

            // Border1에서 중복 주소를 가진 항목 찾기
            foreach (var border in Borders1)
            {
                int? address = GetAddressFromBorder(border);
                if (address.HasValue && existingAddresses.Contains(address.Value))
                {
                    itemsToRemove.Add(border);
                }
            }

            // 찾은 중복 항목들을 Border1에서 제거
            foreach (var item in itemsToRemove)
            {
                Borders1.Remove(item);
            }

            // 중복 제거 완료 메시지 표시
            if (itemsToRemove.Count > 0)
            {
                MessageBox.Show($"Border1에서 중복된 {itemsToRemove.Count}개의 아이템이 제거되었습니다.",
                    "중복 제거 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Border sourceItem)
            {
                var sourceCollection = GetCollectionContainingItem(sourceItem);
                if (sourceCollection == null) return;

                // 소스 아이템의 주소 가져오기
                int? sourceItemAddress = GetAddressFromBorder(sourceItem);
                if (sourceItemAddress == null) return;

                // UI 업데이트
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

                        if (i < Borders2.Count)
                        {
                            var border = Borders2[i];
                            var label = border.Child as Label;
                            if (label != null)
                            {
                                label.Content = $"Address: {parameters[i].Address}, Value: {parameters[i].DefaultActual}";
                            }
                        }

                        if (i < Borders3.Count)
                        {
                            var border = Borders3[i];
                            var label = border.Child as Label;
                            if (label != null)
                            {
                                label.Content = $"Address: {parameters[i].Address}, Value: {parameters[i].DefaultActual}";
                            }
                        }
                    }
                });

                if (dropInfo.TargetItem is Border targetItem)
                {
                    var targetCollection = GetCollectionContainingItem(targetItem);
                    if (targetCollection != null)
                    {
                        // 주소 중복 검사
                        if (HasDuplicateAddress(targetCollection, sourceItemAddress.Value, sourceItem))
                        {
                            MessageBox.Show($"컬렉션에 이미 주소 {sourceItemAddress}가 존재합니다!", "중복 주소 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        int sourceIndex = sourceCollection.IndexOf(sourceItem);
                        int targetIndex = targetCollection.IndexOf(targetItem);

                        if (sourceIndex != -1 && targetIndex != -1)
                        {
                            sourceCollection.RemoveAt(sourceIndex);

                            if (sourceCollection == targetCollection && sourceIndex < targetIndex)
                            {
                                targetIndex--;
                            }

                            ResizeBorder(sourceItem, targetCollection);
                            targetCollection.Insert(targetIndex, sourceItem);
                        }
                    }
                }
                else if (dropInfo.TargetCollection is IList targetCollection)
                {
                    if (targetCollection is ObservableCollection<Border> borderCollection)
                    {
                        // 주소 중복 검사
                        if (HasDuplicateAddress(borderCollection, sourceItemAddress.Value, sourceItem))
                        {
                            MessageBox.Show($"컬렉션에 이미 주소 {sourceItemAddress}가 존재합니다!", "중복 주소 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        int sourceIndex = sourceCollection.IndexOf(sourceItem);
                        if (sourceIndex != -1)
                        {
                            sourceCollection.RemoveAt(sourceIndex);

                            if (dropInfo.InsertIndex >= 0 && dropInfo.InsertIndex <= borderCollection.Count)
                            {
                                ResizeBorder(sourceItem, borderCollection, dropInfo.InsertIndex);
                                borderCollection.Insert(dropInfo.InsertIndex, sourceItem);
                            }
                            else
                            {
                                ResizeBorder(sourceItem, borderCollection, borderCollection.Count);
                                borderCollection.Add(sourceItem);
                            }
                        }
                    }
                }
            }
        }

        // Border에서 주소값 추출
        private int? GetAddressFromBorder(Border border)
        {
            if (border?.Child is Label label && label.Content != null)
            {
                string content = label.Content.ToString();
                if (content.StartsWith("Address: "))
                {
                    int commaIndex = content.IndexOf(',');
                    if (commaIndex > 0)
                    {
                        string addressStr = content.Substring(9, commaIndex - 9);
                        if (int.TryParse(addressStr, out int address))
                        {
                            return address;
                        }
                    }
                }
            }
            return null;
        }

        // 컬렉션에 동일한 주소가 있는지 확인
        private bool HasDuplicateAddress(ObservableCollection<Border> collection, int address, Border excludeItem)
        {
            foreach (var item in collection)
            {
                // 비교 대상에서 자기 자신 제외
                if (item == excludeItem)
                    continue;

                int? itemAddress = GetAddressFromBorder(item);
                if (itemAddress.HasValue && itemAddress.Value == address)
                {
                    return true;
                }
            }
            return false;
        }

        // Border 크기 조정 및 마진 설정 메서드
        private void ResizeBorder(Border border, ObservableCollection<Border> targetCollection, int? insertIndex = null)
        {
            if (targetCollection == Borders1)
            {
                // Borders1로 이동할 때 크기 조정
                border.Width = 120;
                border.Height = 50;
                border.Margin = new Thickness(0, 0, 0, 0);
            }
            else if (targetCollection == Borders2)
            {
                // Borders2로 이동할 때 크기 조정
                border.Width = 200;
                border.Height = 40;
                border.Margin = new Thickness(0, 0, 0, 0);
            }
            else if (targetCollection == Borders3)
            {
                // Borders3로 이동할 때 크기 조정 및 바둑판 형식 배치
                border.Width = 150;
                border.Height = 150;

                int index = insertIndex ?? targetCollection.Count;
                int row = index / 2;
                int column = index % 2;

                border.Margin = new Thickness(
                    column * 5,
                    row * 5,
                    5,
                    5
                );
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
