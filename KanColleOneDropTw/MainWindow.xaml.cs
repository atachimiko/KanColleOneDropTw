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
		// TwitterAPIを利用するための各種キー
		// 
		// twitterConsumerKey    : APIサービスを使用するアプリケーション登録
		// twitterConsumerSecret : Keyに対するパスワード
		// AccessToken           : API利用者側が上記のカスタマーキーを使用する際に発行されるトークン
		// AccessTokenSecret     : AccessTokenのパスワード

		static string twitterConsumerKey = "******************";
		static string twitterConsumerSecret = "************************";
		static string AccessToken = "************************";
		static string AccessTokenSecret = "*******************";
		static TwitterService service;
		static string ApplicationDirectoryPath;

		public MainWindow()
		{
			InitializeComponent();

			InitializeDirectory();

			service = new TwitterService(twitterConsumerKey, twitterConsumerSecret);
			service.AuthenticateWith(AccessToken, AccessTokenSecret);

			DispatcherHelper.UIDispatcher = this.Dispatcher;

			InitializeItems();
		}

		/// <summary>
		///     保存先ディレクトリの作成と指定
		/// </summary>
		void InitializeDirectory()
		{
			// Windowsのマイドキュメント内
			// 「KanColleOneDropTw」というフォルダを(なければ)作成します。
			string personalDirectoryPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			ApplicationDirectoryPath = System.IO.Path.Combine(personalDirectoryPath, @"KanColleOneDropTw");
			System.IO.Directory.CreateDirectory(ApplicationDirectoryPath);
		}

		/// <summary>
		/// 
		/// </summary>
		void InitializeItems()
		{
			DispatcherCollection<TweetListItemData> items = new DispatcherCollection<TweetListItemData>(DispatcherHelper.UIDispatcher);

			SearchOptions searchOption = new SearchOptions();
			searchOption.Q = "-RT #艦これ版深夜の真剣お絵描き60分一本勝負";
			searchOption.Resulttype = TwitterSearchResultType.Recent;
			searchOption.Count = 10;
			searchOption.Lang = "ja";
			//searchOption.MaxId = 0L;

			IAsyncResult result = service.Search(searchOption, (searchResult, response) =>
			{
				if (response.StatusCode == HttpStatusCode.OK)
				{
					foreach (var tweet in searchResult.Statuses)
					{
						var i = new TweetListItemData(tweet);
						items.Add(i);

						// BackgroundWorkerを使えばいいのだけれど。
						DispatcherHelper.UIDispatcher.Invoke(() =>
						{
							if (tweet.Entities.Count() > 0)
							{
								var photo = tweet.Entities.Media.FirstOrDefault();
								if (photo != null)
								{
									SaveMediaFile(photo.MediaUrl, tweet.Id);
									i.PhotoImage = new BitmapImage(new Uri(photo.MediaUrl));
								}
							}

							i.AvaterImage = new BitmapImage(new Uri(tweet.User.ProfileImageUrl));
						});
					}
				}
			});
			//result.AsyncWaitHandle.WaitOne();

			this.TweetListView.ItemsSource = items;
		}

		/// <summary>
		///     HTTPでダウンロードしたものをファイルとして保存(リクエストはWebClient依存)
		/// </summary>
		/// <param name="remoteMediaFileUrl"></param>
		/// <param name="tweetId"></param>
		void SaveMediaFile(string remoteMediaFileUrl, long tweetId)
		{
			var uri = new Uri(remoteMediaFileUrl);
			var localFileName = tweetId + System.IO.Path.GetExtension(uri.LocalPath);

			WebClient webClient = new WebClient();
			webClient.DownloadFile(remoteMediaFileUrl, System.IO.Path.Combine(ApplicationDirectoryPath, localFileName));
		}
	}

	public class TweetListItemData
	{
		public TweetListItemData(TwitterStatus model)
		{
			this.TwitterModel = model;
		}

		readonly TwitterStatus TwitterModel;

		public string BDName { get { return string.Format("{0} @{1}", this.TwitterModel.User.Name, this.TwitterModel.User.ScreenName); } }
		public DateTime CreateDate { get { return this.TwitterModel.CreatedDate; } }
		public DateTime CreateDateJp { get { return this.TwitterModel.CreatedDate.AddHours(9); } }
		public string TweetText { get { return this.TwitterModel.Text; } }

		public BitmapImage PhotoImage { get; set; }
		public BitmapImage AvaterImage { get; set; }
	}
}
