using Mvvm;
using Mvvm.ViewModels;


namespace Mvvm.Test {
   [TestClass]
   public sealed class Test1 {



      private MainWindowViewModel mainWindowViewModel;

  
      [TestMethod]
      public void TestMethod1() {
          mainWindowViewModel = new MainWindowViewModel();
         int result = mainWindowViewModel.plus(2, 2);

         Assert.AreEqual(4,result,"2 + 2 should be 4");
 

         }
      }
   }
