KanColleOneDropTw
=================

ハッシュタグ #艦これ版深夜の真剣お絵描き60分一本勝負 をTwitterAPIを使って検索し、
取得したツイートから画像ファイルを表示・保存するサンプルアプリケーションです。

このアプリケーションは **開発者向け** ですのでバイナリの配布は行っていません。

TwitterAPIを使用するためのカスタマーキーなどは各自取得してください。
クライアントのためのトークン生成も各自行ってください。

## サポート ##
このパッケージの利用の結果生じた損害について、一切責任を負いません。
自己責任でお願いします。

ソースコードのライセンスについてはLICENCEを参照してください。

## 使い方 ##
TwitterAPIを利用するためのカスタマーキーを発行します
https://dev.twitter.com/

次にクライアント用のトークンを発行します
```c#
string twitterConsumerKey = "***"; // カスタマーキー
string twitterConsumerSecret = "***"; // カスタマーキーのパスワード
 
var service = new TwitterService(twitterConsumerKey, twitterConsumerSecret);
 
// Step 1 - Retrieve an OAuth Request Token
OAuthRequestToken requestToken = service.GetRequestToken();
 
// Step 2 - Redirect to the OAuth Authorization URL
Uri uri = service.GetAuthorizationUri(requestToken);
Process.Start(uri.ToString()); // ブラウザが起動し、認証コードが表示される(Twitterにログインしていない場合は、ログインページが先に表示される)
 
// Step 3 - Exchange the Request Token for an Access Token
string verifier = "123456"; // <-- ブラウザに表示された認証コードを入力
OAuthAccessToken access = service.GetAccessToken(requestToken, verify);

// access.Tokenがアクセストークンです。
// access.TokenSecretがアクセストークンのパスワードです。
```

TwitterAPIを実際に使うには、「カスタマーキー、カスタマーキーのパスワード、アクセストークン、アクセストークンのパスワード」の4つのキー文字列が必要です。

