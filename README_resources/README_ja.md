# EasySlackEmojiCreator

"EasySlackEmojiCreator" は、素早くSlack用の絵文字を作成とアップロードを行うためのアプリです。



# DEMO

![demo](/demo.gif)



# Features

* テキスト入力で簡単に絵文字がつくれます
* 絵文字追加用のトークンを登録しておけば、アプリ内からワークスペースへの絵文字の追加ができます
  * 絵文字を追加する度にワークスペースの絵文字追加用Webページを開く必要がないです
  * ワークスペースからサインアウトした場合は、再度絵文字追加用のトークンを登録する必要があります



# Getting started

必要環境

* Windows 10
* .NET Core 3.0 Runtime (or later) のインストールが必要です。
  * インストールされていない場合は、アプリ起動時にインストールを促すウィンドウが表示されます。

### アプリケーションの入手  

* 最新版の64bit版exeファイルを右のページからダウンロード。 [release](https://github.com/KoseiYoshida/EasySlackEmojiCreator/releases)  

または、

* ソリューションのビルドを行う。
  * 最新版の[VisualStudio](https://developer.microsoft.com/en-us/windows/downloads) 、あるいはその他のビルドツールを用意する。
  * このリポジトリをクローンする
    * git clone https://github.com/KoseiYoshida/EasySlackEmojiCreator.git
  * [SlackEmojiCreator.sln](https://github.com/KoseiYoshida/EasySlackEmojiCreator/blob/master/SlackEmojiCreator.sln) をVisualStudioで開き、ビルドを行う。

### 絵文字アップロード用トークンの入手  

初回利用時に絵文字アップロード用のトークンをアプリに登録します。（サインアウトした場合は、トークンがリフレッシュされるので、次回のサインイン時に再度トークンを取得し直す必要があります。）

* ワークスペースのカスタマイズ（絵文字追加用）ページを開きます。
  * スラック左上に表示されている"ワークスペース名▽"を押してメニューを開きます。
  * メニューから"Administration > Customize (ワークスペース名)".を選択しカスタマイズページを開きます。
* ブラウザの[開発ツール](http://webmasters.stackexchange.com/a/77337)を開きます。
* トークンを開発ツールを使って探します。
  * "Network"タブを開きます。
  * その状態でページをリロードします。
  * 表示される項目の中から"Name"が"emoji.list?" から始まっているものを探し、それをクリックします。
  * クリックすると"emoji.llist?~~"に関する情報が表示されます。
  * 表示された譲歩うから"Headers" の項目を探し、その中にある"Form Data"の欄に書いてあるのが絵文字アップロード用のトークンになります。(トークンは、おそらく "xoxs~"という文字列です。)
* トークンをコピーしておきます。（後でアプリケーションの中に貼り付けます）

### トークンの登録

* アプリを開きます。
* "Account" ボタンを押すと、ダイアログが開かれます。
* ワークスペース名と絵文字アップロード用のトークンを入力します。
* "Save" ボタンを押し保存します。
* ダイアログを閉じます。

### 絵文字をつくりワークスペースに追加します。

* 絵文字をつくる。
  * テキストを左上の入力エリアに入力します。
  * 色とフォントを選択します。
  * "Add" を押すと現在表示されている絵文字が候補に追加されます。（この時点では、まだワークスペースには追加されていません。）
* 絵文字をワークスペースに追加する。
  * "Upload"ボタンを押すと、候補に入っている絵文字がワークスペースに追加されます。
    * アップロードが成功した絵文字は、自動的に候補から削除されます。
    * アップロードが失敗した絵文字は、候補から削除されず、アップロードの失敗理由が表示されます。



# License
"EasySlackEmojiCreator" は[MIT license](https://en.wikipedia.org/wiki/MIT_License)です。
