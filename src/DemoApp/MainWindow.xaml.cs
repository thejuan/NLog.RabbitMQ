using System;
using System.Windows;
using System.Windows.Controls;
using NLog;

namespace DemoApp
{
	/// <summary>
	/// 	Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var b = (Button) sender;

			switch (b.Name)
			{
				case "Trace":
					logger.Trace("This is a sample trace message");
					break;
				case "Debug":
					logger.DebugTag("This is a sample debug message", "important");
					break;
				case "Info":
					logger.Info("This is a sample info message");
					break;
				case "Warn":
					logger.Warn("This is a sample warn message");
					break;
				case "Error":
					logger.Error("This is a sample error message");
					break;
				case "Fatal":
					logger.Fatal("This is a sample fatal message");
					break;
				case "ErrorWException":
					logger.ErrorException("Oh noes (error)",
						PerformIntricateCalulation());
					break;
			}
		}

		Exception PerformIntricateCalulation()
		{
			try
			{
				var sleepSheep = 100;
				var counted = 0;
				var perCentInverse = sleepSheep/counted;
				Console.WriteLine("You are inversely at infinite sleep, here's the largest number possible, {0}", 
					perCentInverse);
			}
			catch (Exception e)
			{
				return e;
			}
			throw new Exception("compile darn it");
		}

		protected override void OnClosed(System.EventArgs e)
		{
			base.OnClosed(e);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
		}
	}
}