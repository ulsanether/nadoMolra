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
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives; 
using MaterialDesignThemes.Wpf; 

namespace Mvvm.ViewModels
{
    public class HomePageViewModel : BindableBase, IDropTarget, IDragSource
    {
        private CancellationTokenSource _cancellationTokenSource;
        private ModbusConnect _modbusConnect;

        public ObservableCollection<Border> Borders1 { get; set; }
        public ObservableCollection<Border> Borders2 { get; set; }
        public ObservableCollection<Border> Borders3 { get; set; }
        public ObservableCollection<Border> Borders4 { get; set; }
        public ObservableCollection<Border> Borders5 { get; set; }

        public HomePageViewModel(ModbusConnect modbusConnect)
        {
            _modbusConnect = modbusConnect;
            _modbusConnect.ConnectionStatusChanged += ModbusConnect_OnConnectionsStatusChanged;

            int startAddress = Properties.Settings.Default.StartAddress;
            int endAddress = Properties.Settings.Default.EndAddress;
            int numberOfPoints = endAddress - startAddress + 1;

            Borders1 = new ObservableCollection<Border>();
            Borders2 = new ObservableCollection<Border>();
            Borders3 = new ObservableCollection<Border>();
            Borders4 = new ObservableCollection<Border>();
            Borders5 = new ObservableCollection<Border>();

            for (int i = 0; i < numberOfPoints; i++)
            {
                var border = new Border();
                border.Style = (Style)Application.Current.Resources["Borders1Style"];

                var label = new Label
                {
                    Content = $"Add {i + 1}"
                };
                label.Style = (Style)Application.Current.Resources["DefaultBorderLabelStyle"];

                border.Child = label;
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
                    int startAddress = Properties.Settings.Default.StartAddress;
                    int endAddress = Properties.Settings.Default.EndAddress;
                    int numberOfPoints = endAddress - startAddress + 1;

                    if (_modbusConnect != null)
                    {
                        parameters = await _modbusConnect.ReadModbusData(startAddress, numberOfPoints);
                        _modbusConnect.dataBuffer.StoreValues(parameters);
                    }

                    if (parameters != null)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() => UpdateBorderContents());
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"데이터 읽기 중 오류가 발생했습니다: {ex.Message}");
                }

                try
                {
                    await Task.Delay(2000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }


        public void StartDataReading()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () => await ReadDataPeriodically(_cancellationTokenSource.Token));
        }

        public void StopDataReading()
        {
            _cancellationTokenSource?.Cancel();
        }


        private void UpdateBorderContents()
        {
            if (parameters == null || parameters.Count == 0)
                return;

            var parameterMap = parameters.ToDictionary(p => p.Address);

            UpdateBorderCollection(Borders1, parameterMap);
            UpdateBorderCollection(Borders2, parameterMap);
            UpdateBorderCollection(Borders3, parameterMap);
            UpdateBorderCollection(Borders4, parameterMap);
            UpdateBorderCollection(Borders5, parameterMap);
        }

        private void UpdateBorderCollection(ObservableCollection<Border> collection, Dictionary<int, ParameterModel> parameterMap)
        {
            foreach (var border in collection)
            {
                int? borderAddress = GetAddressFromBorder(border);

                if (!borderAddress.HasValue)
                {
                    // 새 Border에 주소가 없는 경우
                    foreach (var param in parameters)
                    {
                        bool isAddressUsed = false;
                        foreach (var existingBorder in collection)
                        {
                            int? existingAddress = GetAddressFromBorder(existingBorder);
                            if (existingAddress.HasValue && existingAddress.Value == param.Address)
                            {
                                isAddressUsed = true;
                                break;
                            }
                        }

                        if (!isAddressUsed)
                        {
                            if (collection == Borders3)
                            {
                                // Borders3인 경우 Grid를 사용하여 이미지와 버튼 설정
                                string content = $"Address: {param.Address}, Value: {param.DefaultActual}, Status: {(param.IsMonitoring ? "true" : "false")}";
                                SetupBorders3Content(border, content, param);
                            }
                            else if (collection == Borders2)
                            {
                                // Borders2인 경우 Grid를 사용하여 슬라이더 설정
                                string content = $"Address: {param.Address}, Value: {param.DefaultActual}, Status: {(param.IsMonitoring ? "true" : "false")}";
                                SetupBorders2Content(border, content, param);
                            }
                            else if (border.Child is Label label)
                            {
                                string status = param.IsMonitoring ? "true" : "false";
                                label.Content = $"Address: {param.Address}, Value: {param.DefaultActual}, Status: {status}";
                            }
                            UpdateParameterStatus(param, collection);
                            break;
                        }
                    }
                    continue;
                }

                if (parameterMap.TryGetValue(borderAddress.Value, out ParameterModel parameter))
                {
                    if (collection == Borders3)
                    {
                        // Borders3인 경우 Grid를 사용하여 이미지와 버튼 설정
                        string content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {(parameter.IsMonitoring ? "true" : "false")}";
                        SetupBorders3Content(border, content, parameter);
                    }
                    else if (collection == Borders2)
                    {
                        // Borders2인 경우 Grid를 사용하여 슬라이더 설정
                        string content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {(parameter.IsMonitoring ? "true" : "false")}";
                        SetupBorders2Content(border, content, parameter);
                    }
                    else if (border.Child is Label label)
                    {
                        string status = parameter.IsMonitoring ? "true" : "false";
                        label.Content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {status}";
                    }
                }
                else
                {
                    if (collection == Borders3)
                    {
                        // 파라미터가 없는 경우에도 Borders3 형식 유지
                        string content = $"Address: {borderAddress.Value}, Value: N/A, Status: false";
                        SetupBorders3Content(border, content, null);
                    }
                    else if (collection == Borders2)
                    {
                        // 파라미터가 없는 경우에도 Borders2 형식 유지
                        string content = $"Address: {borderAddress.Value}, Value: N/A, Status: false";
                        SetupBorders2Content(border, content, null);
                    }
                    else if (border.Child is Label label)
                    {
                        label.Content = $"Address: {borderAddress.Value}, Value: N/A, Status: false";
                    }
                }
            }
        }



        // Borders2용 내용 설정 도우미 메서드
        private void SetupBorders2Content(Border border, string content, ParameterModel parameter)
        {
            // 그리드가 이미 있는지 확인
            if (!(border.Child is Grid))
            {
                // 새로운 그리드 생성
                Grid newGrid = new Grid();
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // 주소 표시 영역
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // 값 표시 영역
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 슬라이더 영역

                // 주소 표시 TextBlock
                TextBlock addressBlock = new TextBlock();
                addressBlock.FontWeight = FontWeights.Bold;
                addressBlock.FontSize = 14;
                addressBlock.HorizontalAlignment = HorizontalAlignment.Center;
                addressBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);
                addressBlock.Margin = new Thickness(0, 5, 0, 0);
                Grid.SetRow(addressBlock, 0);

                // 값 표시 TextBlock
                TextBlock valueBlock = new TextBlock();
                valueBlock.FontSize = 14;
                valueBlock.HorizontalAlignment = HorizontalAlignment.Center;
                valueBlock.Foreground = new SolidColorBrush(Colors.DarkGreen);
                valueBlock.Margin = new Thickness(0, 5, 0, 10);
                Grid.SetRow(valueBlock, 1);

                // MaterialDesign 슬라이더 생성
                Slider slider = new Slider();
                slider.Orientation = Orientation.Vertical;
                slider.Minimum = 0;
                slider.Maximum = 100;
                slider.Height = 120;
                slider.Margin = new Thickness(10);
                slider.VerticalAlignment = VerticalAlignment.Stretch;
                slider.HorizontalAlignment = HorizontalAlignment.Center;
                slider.TickFrequency = 10;
                slider.IsSnapToTickEnabled = true;
                slider.TickPlacement = TickPlacement.BottomRight;

                // Material Design 슬라이더 스타일 설정
                slider.Style = (Style)Application.Current.Resources["MaterialDesignDiscreteSlider"];

                // SliderAssist를 사용하기 위해 직접 클래스를 사용
                MaterialDesignThemes.Wpf.SliderAssist.SetOnlyShowFocusVisualWhileDragging(slider, true);
                // SetValueTooltipFormatter는 제거 (버전에 없음)

                Grid.SetRow(slider, 2);

                // 값 변경 이벤트 핸들러
                slider.ValueChanged += (sender, e) => {
                    if (valueBlock != null)
                    {
                        valueBlock.Text = $"값: {e.NewValue:F2}";

                        // 실제 파라미터 값 업데이트 로직 (필요 시)
                        if (TryExtractAddressAndValue(content, out int sliderAddress, out _))
                        {
                            var param = parameters.FirstOrDefault(p => p.Address == sliderAddress);
                            if (param != null)
                            {
                                // 여기에 실제 모드버스 값 업데이트 로직 추가할 수 있음
                                // _modbusConnect.WriteRegister(sliderAddress, (int)e.NewValue);
                            }
                        }
                    }
                };

                // 그리드에 요소 추가
                newGrid.Children.Add(addressBlock);
                newGrid.Children.Add(valueBlock);
                newGrid.Children.Add(slider);

                // 그리드를 Border의 새 자식으로 설정
                border.Child = newGrid;
            }

            // 주소와 값 추출
            int address = 0;
            double value = 0;
            if (parameter != null)
            {
                address = parameter.Address;
                value = parameter.DefaultActual;
            }
            else if (TryExtractAddressAndValue(content, out int extractedAddress, out double extractedValue))
            {
                address = extractedAddress;
                value = extractedValue;
            }

            // 기존 그리드가 있는 경우 내용 업데이트
            if (border.Child is Grid existingGrid)
            {
                foreach (var child in existingGrid.Children)
                {
                    if (child is TextBlock textBlock)
                    {
                        int row = Grid.GetRow(textBlock);
                        if (row == 0) // 주소 블록
                        {
                            textBlock.Text = $"주소: {address}";
                        }
                        else if (row == 1) // 값 블록
                        {
                            textBlock.Text = $"값: {value:F2}";
                        }
                    }
                    else if (child is Slider slider)
                    {
                        // 슬라이더의 이전 이벤트 핸들러 가져오기 방법이 현재 없으므로
                        // 간단하게 처리하기 위해 임시로 값만 업데이트

                        // 슬라이더 값 임시 설정
                        // (ValueChanged 이벤트는 계속 발생할 수 있음)
                        slider.Value = value;
                        slider.Tag = content;

                        // 최소/최대 값을 적절히 설정
                        double min = Math.Max(0, value * 0.5);
                        double max = value * 1.5;
                        if (max - min < 10) // 너무 작은 범위면 확장
                        {
                            min = Math.Max(0, value - 5);
                            max = value + 5;
                        }

                        slider.Minimum = min;
                        slider.Maximum = max;
                    }
                }
            }
        }




        // Borders3용 내용 설정 도우미 메서드
        private void SetupBorders3Content(Border border, string content, ParameterModel parameter)
        {
            // 그리드가 이미 있는지 확인
            if (!(border.Child is Grid))
            {
                // 그리드 새로 생성
                Grid newGrid = new Grid();
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 주소와 값 표시 영역
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // 이미지 영역
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 버튼 영역

                // 주소와 값 표시할 StackPanel
                StackPanel infoPanel = new StackPanel();
                infoPanel.Orientation = Orientation.Vertical;
                infoPanel.Margin = new Thickness(5);

                // 주소 표시 TextBlock
                TextBlock addressBlock = new TextBlock();
                addressBlock.FontWeight = FontWeights.Bold;
                addressBlock.FontSize = 14;
                addressBlock.HorizontalAlignment = HorizontalAlignment.Center;
                addressBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);

                // 값 표시 TextBlock
                TextBlock valueBlock = new TextBlock();
                valueBlock.FontSize = 14;
                valueBlock.HorizontalAlignment = HorizontalAlignment.Center;
                valueBlock.Foreground = new SolidColorBrush(Colors.DarkGreen);

                // StackPanel에 추가
                infoPanel.Children.Add(addressBlock);
                infoPanel.Children.Add(valueBlock);
                Grid.SetRow(infoPanel, 0);

                // 이미지 생성
                Image img = new Image();
                img.Stretch = Stretch.Uniform;
                img.Margin = new Thickness(5);
                img.Source = new BitmapImage(new Uri("/Dictionaries/free-sticker-retro-5928520.png", UriKind.Relative));
                Grid.SetRow(img, 1);

                // 버튼 생성
                Button btn = new Button();
                btn.Content = "설정";
                btn.Margin = new Thickness(5);
                btn.Style = (Style)Application.Current.Resources["Border3ButtonStyle"];
                Grid.SetRow(btn, 2);

                // 버튼 클릭 이벤트
                btn.Click += Border3ButtonClick;

                // 그리드에 요소 추가
                newGrid.Children.Add(infoPanel);
                newGrid.Children.Add(img);
                newGrid.Children.Add(btn);

                // 그리드를 Border의 새 자식으로 설정
                border.Child = newGrid;
            }

            // 주소와 값 추출
            int address = 0;
            double value = 0;
            if (parameter != null)
            {
                address = parameter.Address;
                value = parameter.DefaultActual;
            }
            else if (TryExtractAddressAndValue(content, out int extractedAddress, out double extractedValue))
            {
                address = extractedAddress;
                value = extractedValue;
            }

            // 기존 그리드가 있는 경우 태그와 내용 업데이트
            if (border.Child is Grid existingGrid)
            {
                foreach (var child in existingGrid.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Tag = content;

                        // 파라미터가 있는 경우 버튼 텍스트 업데이트
                        if (parameter != null)
                        {
                            btn.Content = $"{parameter.Address:D3}";
                        }
                    }
                    else if (child is Image img && parameter != null)
                    {
                        // 파라미터 값에 따라 이미지 동적 변경 가능
                        if (parameter.DefaultActual > 50)
                        {
                            img.Source = new BitmapImage(new Uri("/Dictionaries/free-sticker-retro-5928520.png", UriKind.Relative));
                        }
                    }
                    else if (child is StackPanel infoPanel)
                    {
                        // StackPanel 내의 주소와 값 TextBlock 업데이트
                        if (infoPanel.Children.Count >= 2)
                        {
                            if (infoPanel.Children[0] is TextBlock addressBlock)
                            {
                                addressBlock.Text = $"주소: {address}";
                            }

                            if (infoPanel.Children[1] is TextBlock valueBlock)
                            {
                                valueBlock.Text = $"값: {value:F2}";
                            }
                        }
                    }
                }
            }
        }


        #region IDropTarget 구현
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

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Border sourceItem)
            {
                var sourceCollection = GetCollectionContainingItem(sourceItem);
                if (sourceCollection == null) return;

                // 소스 아이템의 주소 가져오기
                int? sourceItemAddress = GetAddressFromBorder(sourceItem);
                if (sourceItemAddress == null) return;

                // 해당 주소의 파라미터 찾기
                var parameterToUpdate = parameters.FirstOrDefault(p => p.Address == sourceItemAddress);
                if (parameterToUpdate == null) return;

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

                            // 상태 업데이트: Border 컬렉션에 따라 true/false 설정
                            UpdateParameterStatus(parameterToUpdate, targetCollection);

                            ResizeBorder(sourceItem, targetCollection);
                            targetCollection.Insert(targetIndex, sourceItem);
                        }
                    }
                }
                else if (dropInfo.TargetCollection is IList targetCollection)
                {
                    if (targetCollection is ObservableCollection<Border> borderCollection)
                    {
                        if (HasDuplicateAddress(borderCollection, sourceItemAddress.Value, sourceItem))
                        {
                            MessageBox.Show($"컬렉션에 이미 주소 {sourceItemAddress}가 존재합니다!", "중복 주소 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        int sourceIndex = sourceCollection.IndexOf(sourceItem);
                        if (sourceIndex != -1)
                        {
                            sourceCollection.RemoveAt(sourceIndex);

                            UpdateParameterStatus(parameterToUpdate, borderCollection);

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

                UpdateBorderContents();
            }
        }

        // 새로운 메서드: 파라미터의 상태를 컬렉션에 따라 업데이트
        private void UpdateParameterStatus(ParameterModel parameter, ObservableCollection<Border> targetCollection)
        {
            if (parameter != null)
            {
                // 각 컬렉션에 따라 다른 상태 설정
                if (targetCollection == Borders1)
                {
                    parameter.IsMonitoring = false;
                }
                else if (targetCollection == Borders2)
                {
                    parameter.IsMonitoring = true;
                }
                else if (targetCollection == Borders3)
                {
                    parameter.IsMonitoring = true;
                }
                else if (targetCollection == Borders4)
                {
                    parameter.IsMonitoring = true;
                }
                else if (targetCollection == Borders5)
                {
                    parameter.IsMonitoring = true;
                }

                UpdateBorderLabel(parameter);
            }
        }

        // 새로운 메서드: Border의 라벨 업데이트
        private void UpdateBorderLabel(ParameterModel parameter)
        {
            foreach (var border in Borders1
                .Concat(Borders2)
                .Concat(Borders3)
                .Concat(Borders4)
                .Concat(Borders5))
            {
                int? borderAddress = GetAddressFromBorder(border);
                if (borderAddress.HasValue && borderAddress.Value == parameter.Address)
                {
                    if (border.Child is Label label)
                    {
                        string status = parameter.IsMonitoring ? "true" : "false";
                        label.Content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {status}";
                    }
                }
            }
        }

        private int? GetAddressFromBorder(Border border)
        {
            // Label에서 주소 가져오기
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
            // Grid 구조에서 주소 가져오기 (Border2, Border3용)
            else if (border?.Child is Grid grid)
            {
                // Border3의 StackPanel에서 주소 가져오기
                foreach (var child in grid.Children)
                {
                    if (child is StackPanel panel && panel.Children.Count > 0 && panel.Children[0] is TextBlock addressBlock)
                    {
                        // "주소: 123" 형식에서 숫자 부분만 추출
                        string text = addressBlock.Text;
                        if (text.StartsWith("주소: "))
                        {
                            string addressStr = text.Substring(4).Trim();
                            if (int.TryParse(addressStr, out int address))
                            {
                                return address;
                            }
                        }
                    }
                    // Border2의 TextBlock에서 주소 가져오기
                    else if (child is TextBlock textBlock && Grid.GetRow(textBlock) == 0)
                    {
                        string text = textBlock.Text;
                        if (text.StartsWith("주소: "))
                        {
                            string addressStr = text.Substring(4).Trim();
                            if (int.TryParse(addressStr, out int address))
                            {
                                return address;
                            }
                        }
                    }
                    else if (child is Button btn && btn.Tag != null)
                    {
                        // 버튼의 Tag에서 주소 정보 추출
                        string content = btn.Tag.ToString();
                        if (TryExtractAddressAndValue(content, out int address, out _))
                        {
                            return address;
                        }
                    }
                    else if (child is Slider slider && slider.Tag != null)
                    {
                        // 슬라이더의 Tag에서 주소 정보 추출
                        string content = slider.Tag.ToString();
                        if (TryExtractAddressAndValue(content, out int address, out _))
                        {
                            return address;
                        }
                    }
                }
            }

            return null;
        }


        private bool HasDuplicateAddress(ObservableCollection<Border> collection, int address, Border excludeItem)
        {
            foreach (var item in collection)
            {
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
        private void ResizeBorder(Border border, ObservableCollection<Border> targetCollection, int? insertIndex = null)
        {
            if (targetCollection == Borders1)
            {
                border.Width = 120;
                border.Height = 50;
                border.Margin = new Thickness(0, 0, 0, 0);

                // 기존 라벨 유지
                if (border.Child is Label label)
                {
                    // 라벨 스타일 유지
                }
            }
            else if (targetCollection == Borders2)
            {
                border.Width = 100;
                border.Height = 200;

                int index = insertIndex ?? targetCollection.Count;
                int row = index / 2;
                int column = index % 2;

                border.Margin = new Thickness(
                    column * 5,
                    row * 5,
                    0,
                    0
                );

                // 원래 내용 저장
                string originalContent = "";
                int address = 0;
                double value = 0;

                if (border.Child is Label label && label.Content != null)
                {
                    originalContent = label.Content.ToString() ?? "";
                    TryExtractAddressAndValue(originalContent, out address, out value);
                }

                // 기존 라벨 제거
                border.Child = null;

                // 새로운 그리드 생성
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // 주소 표시 영역
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // 값 표시 영역
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 슬라이더 영역

                // 주소 표시 TextBlock
                TextBlock addressBlock = new TextBlock();
                addressBlock.FontWeight = FontWeights.Bold;
                addressBlock.FontSize = 14;
                addressBlock.HorizontalAlignment = HorizontalAlignment.Center;
                addressBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);
                addressBlock.Text = $"주소: {address}";
                addressBlock.Margin = new Thickness(0, 5, 0, 0);
                Grid.SetRow(addressBlock, 0);

                // 값 표시 TextBlock
                TextBlock valueBlock = new TextBlock();
                valueBlock.FontSize = 14;
                valueBlock.HorizontalAlignment = HorizontalAlignment.Center;
                valueBlock.Foreground = new SolidColorBrush(Colors.DarkGreen);
                valueBlock.Text = $"값: {value:F2}";
                valueBlock.Margin = new Thickness(0, 5, 0, 10);
                Grid.SetRow(valueBlock, 1);

                // MaterialDesign 슬라이더 생성
                Slider slider = new Slider();
                slider.Orientation = Orientation.Vertical;
                slider.Minimum = 0;
                slider.Maximum = 100;
                slider.Value = value;
                slider.Height = 120;
                slider.Margin = new Thickness(10);
                slider.VerticalAlignment = VerticalAlignment.Stretch;
                slider.HorizontalAlignment = HorizontalAlignment.Center;
                slider.TickFrequency = 10;
                slider.IsSnapToTickEnabled = true;
                slider.TickPlacement = TickPlacement.BottomRight;

                // Material Design 슬라이더 스타일 설정
                slider.Style = (Style)Application.Current.Resources["MaterialDesignDiscreteSlider"];
                MaterialDesignThemes.Wpf.SliderAssist.SetOnlyShowFocusVisualWhileDragging(slider, true);

                // 값 변경 이벤트 핸들러
                slider.ValueChanged += (sender, e) => {
                    if (valueBlock != null)
                    {
                        valueBlock.Text = $"값: {e.NewValue:F2}";

                        // 실제 파라미터 값도 업데이트
                        if (TryExtractAddressAndValue(originalContent, out int sliderAddress, out _))
                        {
                            var parameter = parameters.FirstOrDefault(p => p.Address == sliderAddress);
                            if (parameter != null)
                            {
                                // 값 변경 로직 - 여기서는 UI만 업데이트
                                // 실제 값을 변경하려면 ModbusConnect를 통해 값 쓰기 필요
                                // _modbusConnect.WriteRegister(sliderAddress, (int)e.NewValue);
                            }
                        }
                    }
                };

                Grid.SetRow(slider, 2);

                // 텍스트 정보를 슬라이더의 Tag에 저장
                slider.Tag = originalContent;

                // 그리드에 요소 추가
                grid.Children.Add(addressBlock);
                grid.Children.Add(valueBlock);
                grid.Children.Add(slider);

                // 그리드를 Border의 새 자식으로 설정
                border.Child = grid;
            }
            else if (targetCollection == Borders3)
            {
                border.Width = 150;
                border.Height = 180; // 약간 높이 증가 (주소와 값을 표시하기 위해)
                border.Style = (Style)Application.Current.Resources["Borders3Style"];

                int index = insertIndex ?? targetCollection.Count;
                int row = index / 2;
                int column = index % 2;

                border.Margin = new Thickness(
                    column * 5,
                    row * 5,
                    0,
                    0
                );

                // 원래 내용 저장
                string originalContent = "";
                int address = 0;
                double value = 0;

                if (border.Child is Label label && label.Content != null)
                {
                    originalContent = label.Content.ToString() ?? "";
                    TryExtractAddressAndValue(originalContent, out address, out value);
                }

                // 기존 라벨 제거
                border.Child = null;

                // 새로운 그리드 생성
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 주소와 값 표시 영역
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // 이미지 영역
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 버튼 영역

                // 주소와 값을 표시할 StackPanel
                StackPanel infoPanel = new StackPanel();
                infoPanel.Orientation = Orientation.Vertical;
                infoPanel.Margin = new Thickness(5);

                // 주소 표시 TextBlock
                TextBlock addressBlock = new TextBlock();
                addressBlock.FontWeight = FontWeights.Bold;
                addressBlock.FontSize = 14;
                addressBlock.HorizontalAlignment = HorizontalAlignment.Center;
                addressBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);
                addressBlock.Text = $"주소: {address}";

                // 값 표시 TextBlock
                TextBlock valueBlock = new TextBlock();
                valueBlock.FontSize = 14;
                valueBlock.HorizontalAlignment = HorizontalAlignment.Center;
                valueBlock.Foreground = new SolidColorBrush(Colors.DarkGreen);
                valueBlock.Text = $"값: {value:F2}";

                // StackPanel에 추가
                infoPanel.Children.Add(addressBlock);
                infoPanel.Children.Add(valueBlock);
                Grid.SetRow(infoPanel, 0);

                // 이미지 생성
                Image img = new Image();
                img.Stretch = Stretch.Uniform;
                img.Margin = new Thickness(5);
                img.Source = new BitmapImage(new Uri("/Dictionaries/free-sticker-retro-5928520.png", UriKind.Relative));
                Grid.SetRow(img, 1);

                // 버튼 생성
                Button btn = new Button();
                btn.Content = "설정";
                btn.Margin = new Thickness(5);
                btn.Style = (Style)Application.Current.Resources["Border3ButtonStyle"];
                Grid.SetRow(btn, 2);

                // 텍스트 정보를 버튼의 Tag에 저장
                btn.Tag = originalContent;

                // 버튼 클릭 이벤트
                btn.Click += (sender, e) => {
                    Button clickedBtn = sender as Button;
                    if (clickedBtn != null)
                    {
                        string extractedAddress = ExtractAddressFromContent(clickedBtn.Tag.ToString());
                        if (!string.IsNullOrEmpty(extractedAddress))
                        {
                            MessageBox.Show($"버튼 클릭됨: 주소 {extractedAddress}");
                        }
                    }
                };

                // 그리드에 요소 추가
                grid.Children.Add(infoPanel);
                grid.Children.Add(img);
                grid.Children.Add(btn);

                // 그리드를 Border의 새 자식으로 설정
                border.Child = grid;
            }
            else if (targetCollection == Borders4)
            {
                border.Width = 50;
                border.Height = 30;
                border.Margin = new Thickness(0, 0, 0, 0);

                // 기존 라벨 유지
                if (border.Child is Label label)
                {
                    // 라벨 스타일 유지
                }
            }
            else if (targetCollection == Borders5)
            {
                border.Width = 100;
                border.Height = 50;
                border.Margin = new Thickness(0, 0, 0, 0);

                // 기존 라벨 유지
                if (border.Child is Label label)
                {
                    // 라벨 스타일 유지
                }
            }
        }

        // 주소를 추출하는 헬퍼 메서드
        private string ExtractAddressFromContent(string content)
        {
            if (content.StartsWith("Address: "))
            {
                int commaIndex = content.IndexOf(',');
                if (commaIndex > 0)
                {
                    return content.Substring(9, commaIndex - 9);
                }
            }
            return "";
        }

        // 주소와 값을 추출하는 헬퍼 메서드
        private bool TryExtractAddressAndValue(string content, out int address, out double value)
        {
            address = 0;
            value = 0;

            if (content.StartsWith("Address: "))
            {
                int commaIndex = content.IndexOf(',');
                if (commaIndex > 0)
                {
                    string addressStr = content.Substring(9, commaIndex - 9);

                    // 값 추출
                    int valueStartIndex = content.IndexOf("Value: ");
                    if (valueStartIndex > 0)
                    {
                        int valueEndIndex = content.IndexOf(',', valueStartIndex);
                        if (valueEndIndex > 0)
                        {
                            string valueStr = content.Substring(valueStartIndex + 7, valueEndIndex - (valueStartIndex + 7));

                            return int.TryParse(addressStr, out address) &&
                                   double.TryParse(valueStr, out value);
                        }
                    }
                }
            }
            return false;
        }


        private void Border3ButtonClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                string content = btn.Tag.ToString();
                if (TryExtractAddressAndValue(content, out int address, out double value))
                {
                    // 파라미터 찾기
                    var parameter = parameters.FirstOrDefault(p => p.Address == address);
                    if (parameter != null)
                    {
                        // 여기서 파라미터를 사용한 작업 수행
                        // 예: 파라미터 설정 다이얼로그 표시
                        MessageBox.Show($"주소: {address}, 현재값: {value}", "파라미터 설정",
                            MessageBoxButton.OK, MessageBoxImage.Information);
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
            else if (Borders4.Contains(item))
            {
                return Borders4;
            }
            else if (Borders5.Contains(item))
            {
                return Borders5;
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

    public static class SliderExtensions
    {
        public static List<EventHandler<RoutedPropertyChangedEventArgs<double>>> GetValueChangedEventHandlers(this Slider slider)
        {
            return new List<EventHandler<RoutedPropertyChangedEventArgs<double>>>();
        }
    }
}
