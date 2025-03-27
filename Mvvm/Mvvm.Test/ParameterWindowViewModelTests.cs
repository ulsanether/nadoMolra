using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using Mvvm.ViewModels;
using Mvvm.Model;
using DevExpress.Mvvm;

namespace Mvvm.Tests
{
    [TestClass]
    public class ParameterWindowViewModelTests
    {
        private Mock<ModbusConnect> _modbusConnectMock;
        private Mock<MainWindowViewModel> _settingPageViewModelMock;
        private ParameterWindowViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _modbusConnectMock = new Mock<ModbusConnect>();
            _settingPageViewModelMock = new Mock<MainWindowViewModel> (null);
            _viewModel = new ParameterWindowViewModel(_modbusConnectMock.Object, _settingPageViewModelMock.Object);
        }

        [TestMethod]
        public void EndAddress_SetNegativeValue_ShowsError()
        {
            var expectedMessage = "끝 주소는 0보다 작을 수 없습니다.";
            var messageBoxMock = new Mock<IMessageBoxService>();
            messageBoxMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                          .Callback<string, string, MessageBoxButton, MessageBoxImage>((msg, title, btn, img) =>
                          {
                              Assert.AreEqual(expectedMessage, msg);
                          });

            _viewModel.EndAddress = -1;

            messageBoxMock.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [TestMethod]
        public void EndAddress_SetValueGreaterThanMaxAddress_ShowsError()
        {
            var expectedMessage = $"끝 주소는 {ParameterWindowViewModel.MAX_ADDRESS}보다 클 수 없습니다.";
            var messageBoxMock = new Mock<IMessageBoxService>();
            messageBoxMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                          .Callback<string, string, MessageBoxButton, MessageBoxImage>((msg, title, btn, img) =>
                          {
                              Assert.AreEqual(expectedMessage, msg);
                          });

            _viewModel.EndAddress = ParameterWindowViewModel.MAX_ADDRESS + 1;

            messageBoxMock.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [TestMethod]
        public void EndAddress_SetValueLessThanStartAddress_ShowsError()
        {
            _viewModel.StartAddress = 10;
            var expectedMessage = "끝 주소는 시작 주소보다 작을 수 없습니다.";
            var messageBoxMock = new Mock<IMessageBoxService>();
            messageBoxMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                          .Callback<string, string, MessageBoxButton, MessageBoxImage>((msg, title, btn, img) =>
                          {
                              Assert.AreEqual(expectedMessage, msg);
                          });

            _viewModel.EndAddress = 5;

            messageBoxMock.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [TestMethod]
        public void EndAddress_SetValueExceedsModbusLimit_ShowsError()
        {
            _viewModel.StartAddress = 0;
            var expectedMessage = "한 번에 읽을 수 있는 최대 레지스터 수는 125개입니다.";
            var messageBoxMock = new Mock<IMessageBoxService>();
            messageBoxMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                          .Callback<string, string, MessageBoxButton, MessageBoxImage>((msg, title, btn, img) =>
                          {
                              Assert.AreEqual(expectedMessage, msg);
                          });

            _viewModel.EndAddress = 126;

            messageBoxMock.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [TestMethod]
        public void EndAddress_SetValidValue_UpdatesEndAddress()
        {
            _viewModel.StartAddress = 0;

            _viewModel.EndAddress = 100;

            Assert.AreEqual(100, _viewModel.EndAddress);
        }
    }
}
