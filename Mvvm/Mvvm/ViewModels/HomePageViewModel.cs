using GongSolutions.Wpf.DragDrop;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Prism.Mvvm;
using System;
using System.Linq;
using Mvvm.Model;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;
using Prism.Commands;

namespace Mvvm.ViewModels
{
    public class HomePageViewModel : BindableBase, IDropTarget, IDragSource
    {

   



        #region Fields
        private CancellationTokenSource _cancellationTokenSource;
        private ModbusConnect _modbusConnect;

        private string _SubTitleName = "프로젝트 제목 적을껌";
        private string _SubTitleNote = "이곳은 프로젝트에 대한 설명. 엑셀 파일에서 가져와야 합니다.";
        #endregion


        #region Properties

        public ObservableCollection<Border> Borders1 { get; set; }
        public ObservableCollection<Border> Borders2 { get; set; }
        public ObservableCollection<Border> Borders3 { get; set; }
        public ObservableCollection<Border> Borders4 { get; set; }
        public ObservableCollection<Border> Borders5 { get; set; }



        #region 서브 타이틀 이름  이것도 엑셀 파일에서 가져와야 함.

        public string SubTitleName
        {
            get => _SubTitleName;
            set => SetProperty(ref _SubTitleName, value);
        }

        public string SubTitleNote
        {
            get => _SubTitleNote;
            set => SetProperty(ref _SubTitleNote, value);
        }

        #endregion


        private bool _hasAlert;
        private int _alertCountl;
        private string _iconColor = "Black";

        public bool HasAlert{
            get => _hasAlert;
            set => SetProperty(ref _hasAlert, value);
        }

        public int AlertCount{
            get => _alertCountl;
            set => SetProperty(ref _alertCountl, value);
        }

        public string IconColor
        {
            get => _iconColor;
            set => SetProperty(ref _iconColor, value);
        }

        public ICommand AlertCommand{ get; }


        #endregion


        public HomePageViewModel(ModbusConnect modbusConnect)
        {
            _modbusConnect = modbusConnect;
            _modbusConnect.ConnectionStatusChanged += ModbusConnect_OnConnectionsStatusChanged;

            int startAddress = Properties.Settings.Default.StartAddress;
            int endAddress = Properties.Settings.Default.EndAddress;
            int numberOfPoints = endAddress - startAddress + 1;


            AlertCommand = new DelegateCommand(AlertCommandExecute);

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
                    Content = $"주소 {i + 1}"
                };
                label.Style = (Style)Application.Current.Resources["DefaultBorderLabelStyle"];

                border.Child = label;
                Borders1.Add(border);
            }
        }

        private void AlertCommandExecute(){
            HasAlert = !HasAlert;
            AlertCount = HasAlert ? AlertCount + 1 : 0;
            IconColor = HasAlert ? "Red" : "Black";
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
                        try
                        {
                            await Application.Current.Dispatcher.InvokeAsync(() => UpdateBorderContents());
                        }
                        catch (Exception ex)
                        {

                        MessageBox.Show($"데이터 업데이트 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                            throw;
                        }
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
                                string content = $"Address: {param.Address}, Value: {param.DefaultActual}, Status: {(param.IsMonitoring ? "true" : "false")}";
                                SetupBorders3Content(border, content, param);
                            }
                            else if (collection == Borders2)
                            {
                                string content = $"Address: {param.Address}, Value: {param.DefaultActual}, Status: {(param.IsMonitoring ? "true" : "false")}";
                                SetupBorders2Content(border, content, param);
                            }
                            else if (border.Child is Label label)
                            {
                                // Borders1의 경우 "주소, Val:값" 형태로 표시
                                if (collection == Borders1)
                                {
                                    label.Content = $"{param.Address}, Val:{param.DefaultActual:F0}";
                                }
                                else
                                {
                                    string status = param.IsMonitoring ? "true" : "false";
                                    label.Content = $"Address: {param.Address}, Value: {param.DefaultActual}, Status: {status}";
                                }
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
                        string content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {(parameter.IsMonitoring ? "true" : "false")}";
                        SetupBorders3Content(border, content, parameter);
                    }
                    else if (collection == Borders2)
                    {
                        string content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {(parameter.IsMonitoring ? "true" : "false")}";
                        SetupBorders2Content(border, content, parameter);
                    }
                    else if (border.Child is Label label)
                    {
                        // Borders1의 경우 "주소, Val:값" 형태로 표시
                        if (collection == Borders1)
                        {
                            label.Content = $"{parameter.Address}, Val:{parameter.DefaultActual:F0}";
                        }
                        else
                        {
                            string status = parameter.IsMonitoring ? "true" : "false";
                            label.Content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {status}";
                        }
                    }
                }
                else
                {
                    if (collection == Borders3)
                    {
                        string content = $"Address: {borderAddress.Value}, Value: N/A, Status: false";
                        SetupBorders3Content(border, content, null);
                    }
                    else if (collection == Borders2)
                    {
                        string content = $"Address: {borderAddress.Value}, Value: N/A, Status: false";
                        SetupBorders2Content(border, content, null);
                    }
                    else if (border.Child is Label label)
                    {
                        // Borders1의 경우 "주소, Val:값" 형태로 표시
                        if (collection == Borders1)
                        {
                            label.Content = $"{borderAddress.Value}, Val:N/A";
                        }
                        else
                        {
                            label.Content = $"Address: {borderAddress.Value}, Value: N/A, Status: false";
                        }
                    }
                }
            }
        }






        private void SetupBorders2Content(Border border, string content, ParameterModel parameter)
        {
            if (!(border.Child is Grid))
            {
            
                Grid newGrid = new Grid();
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // 주소 표시 영역
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // 값 표시 영역
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 슬라이더 영역

    
                TextBlock addressBlock = new TextBlock();
                addressBlock.FontWeight = FontWeights.Bold;
                addressBlock.FontSize = 14;
                addressBlock.HorizontalAlignment = HorizontalAlignment.Center;
                addressBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);
                addressBlock.Margin = new Thickness(0, 5, 0, 0);
                Grid.SetRow(addressBlock, 0);

                TextBlock valueBlock = new TextBlock();
                valueBlock.FontSize = 14;
                valueBlock.HorizontalAlignment = HorizontalAlignment.Center;
                valueBlock.Foreground = new SolidColorBrush(Colors.DarkGreen);
                valueBlock.Margin = new Thickness(0, 5, 0, 10);
                Grid.SetRow(valueBlock, 1);

    
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


                slider.Style = (Style)Application.Current.Resources["MaterialDesignDiscreteSlider"];

              SliderAssist.SetOnlyShowFocusVisualWhileDragging(slider, true);


                Grid.SetRow(slider, 2);

                slider.ValueChanged += async (sender, e) => {
                    if (valueBlock != null)
                    {
                        valueBlock.Text = $"값: {e.NewValue:F2}";

                        if (TryExtractAddressAndValue(content, out int sliderAddress, out _))
                        {
                            var param = parameters.FirstOrDefault(p => p.Address == sliderAddress);
                            if (param != null)
                            {
                              
                                await _modbusConnect.WriteRegister(sliderAddress, (int)e.NewValue);
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



        private void SetupBorders3Content(Border border, string content, ParameterModel parameter)
        {
            if (!(border.Child is Grid))
            {
                Grid newGrid = new Grid();
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 주소/값 영역
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // 이미지 영역
                newGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // 입력/버튼 영역

                StackPanel infoPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5) };
                TextBlock addressBlock = new TextBlock
                {
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.DarkBlue)
                };
                TextBlock valueBlock = new TextBlock
                {
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.DarkGreen)
                };
                infoPanel.Children.Add(addressBlock);
                infoPanel.Children.Add(valueBlock);
                Grid.SetRow(infoPanel, 0);

                Image img = new Image
                {
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(5),
                    Source = new BitmapImage(new Uri("/Dictionaries/fsticker_retro.png", UriKind.Relative))
                };
                Grid.SetRow(img, 1);

        
                StackPanel inputPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(5) };
                TextBox valueInput = new TextBox
                {
                    Width = 60,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                valueInput.PreviewTextInput += (s, e) =>
                {
                    
                    e.Handled = !e.Text.All(char.IsDigit);
                };
                Button btn = new Button
                {
                    Content = "설정",
                    Margin = new Thickness(5, 0, 0, 0),
                    Style = (Style)Application.Current.Resources["Border3ButtonStyle"]
                };

              
                btn.Click += async (s, e) =>
                {
                    int address = 0;
                    if (parameter != null)
                        address = parameter.Address;
                    else if (TryExtractAddressAndValue(content, out int extractedAddress, out _))
                        address = extractedAddress;

                    if (address == 0)
                    {
                        MessageBox.Show("주소를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!int.TryParse(valueInput.Text, out int newValue))
                    {
                        MessageBox.Show("숫자 값을 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    try
                    {
                        await _modbusConnect.WriteRegister(address, newValue);
                        MessageBox.Show($"주소 {address}에 값 {newValue}를 전송했습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"모드버스 전송 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                inputPanel.Children.Add(valueInput);
                inputPanel.Children.Add(btn);
                Grid.SetRow(inputPanel, 2);

                newGrid.Children.Add(infoPanel);
                newGrid.Children.Add(img);
                newGrid.Children.Add(inputPanel);

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

            if (border.Child is Grid existingGrid)
            {
                foreach (var child in existingGrid.Children)
                {
                    if (child is StackPanel infoPanel && infoPanel.Children.Count >= 2)
                    {
                        if (infoPanel.Children[0] is TextBlock addressBlock)
                            addressBlock.Text = $"주소: {address}";
                        if (infoPanel.Children[1] is TextBlock valueBlock)
                            valueBlock.Text = $"값: {value:F2}";
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



    public void RightMouseButtonDown(IDropInfo dropInfo)
        {


         



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

                            // ResizeBorder에서 현재 값을 유지하도록 수정
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

                // UpdateBorderContents() 호출을 제거하거나 조건부로 호출
                // UpdateBorderContents(); // 이 줄을 제거하거나 주석 처리
            }
        }


        private void UpdateParameterStatus(ParameterModel parameter, ObservableCollection<Border> targetCollection)
        {
            if (parameter != null)
            {
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
                        // Borders1인지 확인
                        var collection = GetCollectionContainingItem(border);
                        if (collection == Borders1)
                        {
                            label.Content = $"{parameter.Address}, Val:{parameter.DefaultActual:F0}";
                        }
                        else
                        {
                            string status = parameter.IsMonitoring ? "true" : "false";
                            label.Content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {status}";
                        }
                    }
                }
            }
        }


        private int? GetAddressFromBorder(Border border)
        {
            if (border?.Child is Label label && label.Content != null)
            {
                string content = label.Content.ToString();

                if (content.Contains(", Val:"))
                {
                    string[] parts = content.Split(new string[] { ", Val:" }, StringSplitOptions.None);
                    if (parts.Length >= 1 && int.TryParse(parts[0], out int address))
                    {
                        return address;
                    }
                }
                else if (content.Contains(" : "))
                {
                    string[] parts = content.Split(" : ");
                    if (parts.Length >= 1 && int.TryParse(parts[0], out int address))
                    {
                        return address;
                    }
                }
                else if (content.StartsWith("Address: "))
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
            else if (border?.Child is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is StackPanel panel && panel.Children.Count > 0 && panel.Children[0] is TextBlock addressBlock)
                    {
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
                        string content = btn.Tag.ToString();
                        if (TryExtractAddressAndValue(content, out int address, out _))
                        {
                            return address;
                        }
                    }
                    else if (child is Slider slider && slider.Tag != null)
                    {
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
            int? _curAddr = GetAddressFromBorder(border);
            ParameterModel _curPara = null;

            if (_curAddr.HasValue)
            {
                _curPara = parameters.FirstOrDefault(p => p.Address == _curAddr.Value);
            }

            if (targetCollection == Borders1)
            {
                border.Width = 110;
                border.Height = 30;
                border.Margin = new Thickness(0, 0, 0, 0);
                border.Style = (Style)Application.Current.Resources["Borders1Style"];

                var newLabel = new Label();
                newLabel.Style = (Style)Application.Current.Resources["DefaultBorderLabelStyle"];
                if (_curPara != null)
                {
                  
                    newLabel.Content = $"{_curPara.Address}, Val:{_curPara.DefaultActual:F0}";
                }
                border.Child = newLabel;
            }
            else if (targetCollection == Borders2)
            {
                border.Width = 80;
                border.Height = 200;
                int index = insertIndex ?? targetCollection.Count;
                int row = index / 2;
                int column = index % 2;
                border.Margin = new Thickness(column * 5, row * 5, 0, 0);
                border.Style = (Style)Application.Current.Resources["Borders2Style"];

                CreateBorders2Grid(border, _curPara);
            }
            else if (targetCollection == Borders3)
            {
                border.Width = 150;
                border.Height = 180;
                border.Style = (Style)Application.Current.Resources["Borders3Style"];
                int index = insertIndex ?? targetCollection.Count;
                int row = index / 2;
                int column = index % 2;
                border.Margin = new Thickness(column * 5, row * 5, 0, 0);

                CreateBorders3Grid(border, _curPara);
            }
            else if (targetCollection == Borders4)
            {
                border.Width = 150;
                border.Height = 30;
                border.Margin = new Thickness(0, 0, 0, 0);
                border.Style = (Style)Application.Current.Resources["Borders4Style"];

                var newLabel = new Label();
                newLabel.Style = (Style)Application.Current.Resources["DefaultBorderLabelStyle"];
                if (_curPara != null)
                {
                    string status = _curPara.IsMonitoring ? "true" : "false";
                    newLabel.Content = $"Address: {_curPara.Address}, Value: {_curPara.DefaultActual}, Status: {status}";
                }
                border.Child = newLabel;
            }
            else if (targetCollection == Borders5)
            {
                border.Width = 100;
                border.Height = 50;
                border.Margin = new Thickness(0, 0, 0, 0);
                border.Style = (Style)Application.Current.Resources["Borders5Style"];

                var newLabel = new Label();
                newLabel.Style = (Style)Application.Current.Resources["DefaultBorderLabelStyle"];
                if (_curPara != null)
                {
                    string status = _curPara.IsMonitoring ? "true" : "false";
                    newLabel.Content = $"Address: {_curPara.Address}, Value: {_curPara.DefaultActual}, Status: {status}";
                }
                border.Child = newLabel;
            }
        }



        private void CreateBorders2Grid(Border border, ParameterModel parameter)
        {
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // 주소 표시 TextBlock
            TextBlock addressBlock = new TextBlock();
            addressBlock.FontWeight = FontWeights.Bold;
            addressBlock.FontSize = 14;
            addressBlock.HorizontalAlignment = HorizontalAlignment.Center;
            addressBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);
            addressBlock.Text = parameter != null ? $"주소: {parameter.Address}" : "주소: N/A";
            addressBlock.Margin = new Thickness(0, 5, 0, 0);
            Grid.SetRow(addressBlock, 0);

            // 값 표시 TextBlock
            TextBlock valueBlock = new TextBlock();
            valueBlock.FontSize = 14;
            valueBlock.HorizontalAlignment = HorizontalAlignment.Center;
            valueBlock.Foreground = new SolidColorBrush(Colors.DarkGreen);
            valueBlock.Text = parameter != null ? $"값: {parameter.DefaultActual:F2}" : "값: N/A";
            valueBlock.Margin = new Thickness(0, 5, 0, 10);
            Grid.SetRow(valueBlock, 1);

            // MaterialDesign 슬라이더 생성
            Slider slider = new Slider();
            slider.Orientation = Orientation.Vertical;
            slider.Height = 120;
            slider.Margin = new Thickness(10);
            slider.VerticalAlignment = VerticalAlignment.Stretch;
            slider.HorizontalAlignment = HorizontalAlignment.Center;
            slider.TickFrequency = 10;
            slider.IsSnapToTickEnabled = true;
            slider.TickPlacement = TickPlacement.BottomRight;
            slider.Style = (Style)Application.Current.Resources["MaterialDesignDiscreteSlider"];

            MaterialDesignThemes.Wpf.SliderAssist.SetOnlyShowFocusVisualWhileDragging(slider, true);

            if (parameter != null)
            {
                slider.Value = parameter.DefaultActual;
                double min = Math.Max(0, parameter.DefaultActual * 0.5);
                double max = parameter.DefaultActual * 1.5;
                if (max - min < 10)
                {
                    min = Math.Max(0, parameter.DefaultActual - 5);
                    max = parameter.DefaultActual + 5;
                }
                slider.Minimum = min;
                slider.Maximum = max;

                string content = $"Address: {parameter.Address}, Value: {parameter.DefaultActual}, Status: {(parameter.IsMonitoring ? "true" : "false")}";
                slider.Tag = content;
            }
            else
            {
                slider.Minimum = 0;
                slider.Maximum = 100;
                slider.Value = 0;
            }

            // 슬라이더 값 변경 시 화면 업데이트 및 모드버스 Write
            slider.ValueChanged += async (sender, e) => {
                if (valueBlock != null)
                {
                    valueBlock.Text = $"값: {e.NewValue:F2}";
                }

                // 모드버스에 값 전송
                if (parameter != null)
                {
                    try
                    {
                        await _modbusConnect.WriteRegister(parameter.Address, (int)e.NewValue);
                        // 성공적으로 전송된 경우 파라미터 모델의 값도 업데이트
                        parameter.DefaultActual = e.NewValue;
                    }
                    catch (Exception ex)
                    {
                        // 오류 발생 시 메시지 표시 (선택사항)
                        Debug.WriteLine($"모드버스 전송 오류 (주소: {parameter.Address}): {ex.Message}");
                        // 필요시 MessageBox로 사용자에게 알릴 수도 있음
                        // MessageBox.Show($"모드버스 전송 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };

            Grid.SetRow(slider, 2);

            grid.Children.Add(addressBlock);
            grid.Children.Add(valueBlock);
            grid.Children.Add(slider);

            border.Child = grid;
        }

        /// <summary>
        /// Borders3용 새 Grid 생성
        /// </summary>
        private void CreateBorders3Grid(Border border, ParameterModel parameter)
        {
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

          
            StackPanel infoPanel = new StackPanel();
            infoPanel.Orientation = Orientation.Vertical;
            infoPanel.Margin = new Thickness(5);

            TextBlock addressBlock = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.DarkBlue),
                Text = parameter != null ? $"주소: {parameter.Address}" : "주소: N/A"
            };

            TextBlock valueBlock = new TextBlock
            {
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.DarkGreen),
                Text = parameter != null ? $"값: {parameter.DefaultActual:F2}" : "값: N/A"
            };

            infoPanel.Children.Add(addressBlock);
            infoPanel.Children.Add(valueBlock);
            Grid.SetRow(infoPanel, 0);

            // 이미지 생성
            Image img = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                Source = new BitmapImage(new Uri("/Dictionaries/fsticker_retro.png", UriKind.Relative))
            };
            Grid.SetRow(img, 1);

            // 입력/버튼 영역
            StackPanel inputPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };

            TextBox valueInput = new TextBox
            {
                Width = 60,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            valueInput.PreviewTextInput += (s, e) =>
            {
                // 숫자만 입력 허용
                e.Handled = !e.Text.All(char.IsDigit);
            };

            Button btn = new Button
            {
                Content = parameter != null ? $"{parameter.Address:D3}" : "설정",
                Width = 60,
                Margin = new Thickness(5, 0, 0, 0),
                Style = (Style)Application.Current.Resources["Border3ButtonStyle"]
            };

            btn.Click += async (s, e) =>
            {
                int address = parameter != null ? parameter.Address : 0;
                if (address == 0)
                {
                    MessageBox.Show("주소를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(valueInput.Text, out int newValue))
                {
                    MessageBox.Show("숫자 값을 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    await _modbusConnect.WriteRegister(address, newValue);
                    MessageBox.Show($"주소 {address}에 값 {newValue}를 전송했습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"모드버스 전송 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            inputPanel.Children.Add(valueInput);
            inputPanel.Children.Add(btn);
            Grid.SetRow(inputPanel, 2);

            grid.Children.Add(infoPanel);
            grid.Children.Add(img);
            grid.Children.Add(inputPanel);

            border.Child = grid;
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




        //지금 수정하는 부분. 클릭스 값 수정이 되어 야 함. <-------------------------------------------------- 07.23 수정 부분
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
