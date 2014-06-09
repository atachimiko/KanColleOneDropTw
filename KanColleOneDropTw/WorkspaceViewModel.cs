using Livet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TweetSharp;
using ImpromptuInterface;
using TumblrApi;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Timers;

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
		// tumblrCustomerKey     : TumblrAPIを使用するカスタマーキー
		static string twitterConsumerKey = "*********";
		static string twitterConsumerSecret = "*********";
		static string AccessToken = "*********";
		static string AccessTokenSecret = "*********";
		static string tumblrCustomerKey = "*********";

		// Tumblrからも画像を取得する場合は、tumblrCustomerKeyにAPIを利用するためのキーを入力してこのフラグをTrueにします
		static bool UseTumblrApiFlag = false;
		
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
		public async void InitRenderer()
		{
			await Run();
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

		/// <summary>
		/// 
		/// </summary>
		public async void Reload()
		{
			this._RcentMode = true;
			this._Timer.Stop();
			await Run();
			this._Timer.Start();
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

			_RcentMode = true;
			_Timer = new Timer();
			_Timer.Interval = 60000;
			_Timer.Elapsed += _Timer_Elapsed;
			_Timer.Start();
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
		Timer _Timer;
		bool _RcentMode = false;
		#endregion

		//=====================================================================
		#region [非公開メンバ]
		//=====================================================================
		/// <summary>
		/// Tumblr専用ですね
		/// </summary>
		/// <param name="mediaUrl"></param>
		/// <returns></returns>
		string ExcuseTumblrImageUrl(string mediaUrl)
		{
			string tumblrBlogHostName;
			string tumblrBlogPostIdText;

			Regex rgxTmbrShortner = new Regex("^http://tmblr.co/");
			Regex rgxSegumentaion = new Regex("^/post/([\\d]+)");

			if (rgxTmbrShortner.IsMatch(mediaUrl))
			{
				var url = ExpectShortener(mediaUrl);
				tumblrBlogHostName = url.DnsSafeHost;

				var match = rgxSegumentaion.Match(url.LocalPath);
				if (!match.Success) return string.Empty;
				tumblrBlogPostIdText = match.Groups[1].Value;
			}
			else
			{
				return string.Empty;
			}


			WebClient web = new WebClient();
			string urlText = string.Format("http://api.tumblr.com/v2/blog/{0}/posts?api_key={1}&type=photo&id={2}",
				tumblrBlogHostName,
				tumblrCustomerKey,
				tumblrBlogPostIdText);

			try
			{
				var response = web.DownloadString(urlText);
				var tumblerRp = JsonConvert.DeserializeObject(response);
				var itm = tumblerRp.ActLike<IPostsApi>();
				var photo1 = itm.response.posts[0].photos.FirstOrDefault();

				// JArrayはLINQを実行できないので、手動で配列に置き換える｡ﾟ(ﾟ´Д｀ﾟ)ﾟ｡
				// ついでに言うと、ToArrayも実行できません
				var s = new List<IAltSizes>();
				foreach (var p in photo1.alt_sizes)
				{
					s.Add(p);
				}

				var size1 = s.OrderByDescending(p => p.width + p.height).FirstOrDefault();

				return size1.url;
			}
			catch (Exception expr)
			{
				Debug.WriteLine(expr);
				return string.Empty;
			}
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
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnLoadMediaBackgroundWorker(object sender, DoWorkEventArgs e)
		{
			var item = e.Argument as TweetListItemData;
			var worker = sender as BackgroundWorker;

			string mediaUrl = string.Empty;

			var media = item.Status.Entities.Media.FirstOrDefault();
			if (media != null)
			{
				var photo = item.Status.Entities.Media.FirstOrDefault();
				if (photo != null)
				{
					mediaUrl = photo.MediaUrl;
				}
			}
			else
			{
				if (UseTumblrApiFlag)
				{
					// Tumblerチェック
					var url = item.Status.Entities.Urls.FirstOrDefault();
					if (url != null)
					{
						mediaUrl = ExcuseTumblrImageUrl(url.ExpandedValue);
					}
				}
			}

			item.MediaUrl = mediaUrl;

			if (!string.IsNullOrEmpty(mediaUrl))
				SaveMediaFile(mediaUrl, item.Status.Id);

			
			DispatcherHelper.UIDispatcher.Invoke(() =>
			{
				// ここはユーザーインターフェーススレッドでの実行なので、サーバーが重いとアプリの反応が無くなります

				if (!string.IsNullOrEmpty(mediaUrl))
				{
					item.PhotoImage = new BitmapImage(new Uri(mediaUrl));
				}

				item.AvaterImage = new BitmapImage(new Uri(item.Status.User.ProfileImageUrl));
			});
		}


		/// <summary>
		/// 
		/// </summary>
		async Task Run()
		{
			await Task.Factory
				.StartNew(() =>
			{
				SearchOptions searchOption = new SearchOptions();
				searchOption.Q = "-RT #艦これ版深夜の真剣お絵描き60分一本勝負";
				searchOption.Resulttype = TwitterSearchResultType.Recent;
				searchOption.Count = 11;
				searchOption.Lang = "ja";

				int skipNum = 0;

				if (!this._RcentMode)
				{
					if (lastTweetId != 0L)
					{
						searchOption.MaxId = lastTweetId;
						skipNum = 1;
					}
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

							var bgw = new BackgroundWorker();
							bgw.DoWork += OnLoadMediaBackgroundWorker;
							bgw.RunWorkerAsync(i);

							lastTweetId = tweet.Id;
						}
					}
				});

				//result.AsyncWaitHandle.WaitOne();
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

			using (WebClient webClient = new WebClient())
			{
				webClient.DownloadFile(remoteMediaFileUrl, System.IO.Path.Combine(ApplicationDirectoryPath, localFileName));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			_Timer.Stop();

			Run();

			_Timer.Start();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="shortenerUrl"></param>
		/// <returns></returns>
		Uri ExpectShortener(string shortenerUrl)
		{
			using (WebClient wc = new WebClient())
			{
				var r = wc.DownloadString(shortenerUrl);

				var info = typeof(WebClient).GetField("m_WebResponse", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
				var response = info.GetValue(wc) as HttpWebResponse;
				return response.ResponseUri;
			}
		}
		#endregion

		
	}

	
}

// TumblrApiのモデル構造をPROXYするインターフェース
// 必要そうなものしか実装していません。
namespace TumblrApi
{
	public interface IApiBase
	{
		IMeta meta { get; }
	}

	public interface IPostsApi : IApiBase
	{
		IPostsResponse response { get; }
	}

	public interface IPostsResponse
	{
		IBlog blog { get; }
		IList<IPost> posts { get; }
	}

	public interface IMeta
	{
		int status { get; }
		string msg { get; }
	}

	public interface IBlog
	{
		string title { get; }
		string name { get; }
		string url { get; }
	}

	public interface IPost
	{
		string blog_name { get; }
		long id { get; }
		string post_url { get; }
		string type { get; }

		/// <summary>
		/// photo
		/// </summary>
		string caption { get; }

		/// <summary>
		/// photo
		/// </summary>
		IList<IPhoto> photos { get; }
	}

	public interface IPhoto
	{
		string caption { get; }
		ICollection<IAltSizes> alt_sizes { get; }
	}

	public interface IAltSizes
	{
		int width { get; }
		int height { get; }
		string url { get; }
	}
}