using Livet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TweetSharp;

namespace KanColleOneDropTw
{
	internal class WorkspaceViewModel : ViewModel
	{

		// TwitterAPIを利用するための各種キー
		// 
		// twitterConsumerKey    : APIサービスを使用するアプリケーション登録
		// twitterConsumerSecret : Keyに対するパスワード
		// AccessToken           : API利用者側が上記のカスタマーキーを使用する際に発行されるトークン
		// AccessTokenSecret     : AccessTokenのパスワード
		static string twitterConsumerKey = "*********";
		static string twitterConsumerSecret = "*********";
		static string AccessToken = "*********";
		static string AccessTokenSecret = "*********";

		//=====================================================================
		#region [公開メソッド]
		//=====================================================================
		/// <summary>
		/// 
		/// </summary>
		public void Backlog()
		{
			Run();
		}

		/// <summary>
		/// 
		/// </summary>
		public void OpenBrowser(TweetListItemData item)
		{
			if (item == null) return;
			var tw = item.Status;
			string uriText = string.Format("https://twitter.com/{0}/status/{1}", tw.User.ScreenName, tw.Id);
			System.Diagnostics.Process.Start(uriText);
		}
		#endregion

		//=====================================================================
		#region [コンストラクタ・デストラクタ・Dispose]
		//=====================================================================
		public WorkspaceViewModel()
		{
			InitializeDirectory();

			service = new TwitterService(twitterConsumerKey, twitterConsumerSecret);
			service.AuthenticateWith(AccessToken, AccessTokenSecret);

			Run();
		}
		#endregion

		//=====================================================================
		#region [プロパティ]
		//=====================================================================
		/// <summary>
		/// 
		/// </summary>
		public DispatcherCollection<TweetListItemData> Items { get { return items; } }


		#region SelectedListItem変更通知プロパティ
		private TweetListItemData _SelectedListItem;

		public TweetListItemData SelectedListItem
		{
			get
			{ return _SelectedListItem; }
			set
			{ 
				if (_SelectedListItem == value)
					return;
				_SelectedListItem = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#endregion

		//=====================================================================
		#region [フィールド]
		//=====================================================================
		readonly DispatcherCollection<TweetListItemData> items = new DispatcherCollection<TweetListItemData>(DispatcherHelper.UIDispatcher);
		long lastTweetId = 0L;
		TwitterService service;
		string ApplicationDirectoryPath;
		#endregion

		//=====================================================================
		#region [非公開メンバ]
		//=====================================================================
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
		void Run()
		{
			SearchOptions searchOption = new SearchOptions();
			searchOption.Q = "-RT #艦これ版深夜の真剣お絵描き60分一本勝負";
			searchOption.Resulttype = TwitterSearchResultType.Recent;
			searchOption.Count = 11;
			searchOption.Lang = "ja";

			int skipNum = 0;
			if (lastTweetId != 0L)
			{
				searchOption.MaxId = lastTweetId;
				skipNum = 1;
			}

			this.Items.Clear();
			this.SelectedListItem = null;

			IAsyncResult result = service.Search(searchOption, (searchResult, response) =>
			{
				if (response.StatusCode == HttpStatusCode.OK)
				{
					// RxとLINQを使えばもっとスマートだけれど。
					foreach (var tweet in searchResult.Statuses.Skip(skipNum))
					{
						var i = new TweetListItemData(tweet);
						this.Items.Add(i);

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
						lastTweetId = tweet.Id;
					}
				}
			});
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
		#endregion
	}
}
