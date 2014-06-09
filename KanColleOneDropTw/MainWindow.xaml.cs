using Livet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TweetSharp;

namespace KanColleOneDropTw
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DispatcherHelper.UIDispatcher = this.Dispatcher;

			var vm = new WorkspaceViewModel();
			this.DataContext = vm;
		}
	}

	public class TweetListItemData : NotificationObject
	{
		public TweetListItemData(TwitterStatus model)
		{
			this.TwitterModel = model;
		}

		readonly TwitterStatus TwitterModel;

		public TwitterStatus Status { get { return TwitterModel; } }

		public string BDName { get { return string.Format("{0} @{1}", this.TwitterModel.User.Name, this.TwitterModel.User.ScreenName); } }
		public DateTime CreateDate { get { return this.TwitterModel.CreatedDate; } }
		public DateTime CreateDateJp { get { return this.TwitterModel.CreatedDate.AddHours(9); } }
		public string TweetText { get { return this.TwitterModel.Text; } }

		string _MediaUrl;
		public string MediaUrl
		{
			get { return _MediaUrl; }
			set { _MediaUrl = value; }
		}

		#region PhotoImage変更通知プロパティ
		private Image _PhotoImage;
		/// <summary>
		/// 
		/// </summary>
		public Image PhotoImage
		{
			get
			{ return _PhotoImage; }
			set
			{ 
				if (_PhotoImage == value)
					return;
				_PhotoImage = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region AvaterImage変更通知プロパティ
		private Image _AvaterImage;
		/// <summary>
		/// 
		/// </summary>
		public Image AvaterImage
		{
			get
			{ return _AvaterImage; }
			set
			{ 
				if (_AvaterImage == value)
					return;
				_AvaterImage = value;
				RaisePropertyChanged();
			}
		}
		#endregion

	}
}
