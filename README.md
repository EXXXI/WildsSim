# WildsSim(気ままに作ったモンハンシミュレータ for MHWilds)

## 概要

Windows上で動くモンハンワイルズスキルシミュレータです

## 更新・対応状況

本体の最終更新：2025/4/18 シリーズスキル・グループスキルのレベル固定検索の仕様を改善(固定したLv以上、次段階のLv未満で検索)

CSVの最終更新：2025/4/30 歴戦王レダウ関連の装備を解放

今までの更新履歴は[ReleaseNote.md](./ReleaseNote.md)を参照

## 特徴

- 各種データをCSV形式(一部json形式)で保持しています
  - 発売直後やイベクエ追加時など、装備等の追加を、シミュの更新を待たずに自身で行えます
- 「最近検索に利用したスキル」からスキルを選択できます
- 「よく使う検索条件」を保存して、必要な時に呼び出すことができます
- ソースを公開しています
  - ご自由に自分好みにカスタマイズしてください

## 使い方

### 起動方法

- WildsSim.zipをダウンロード
- WildsSim.zipを解凍
- 中にあるWildsSim.exeをダブルクリック

### 機能説明

偉大な先人が作られたシミュに似せているので大体見ればわかるはず

詳しく知りたい人は[Reference.md](./Reference.md)を参照してください

## 注意点

- 64bitマシンのWindowsでしか動きません
- .Netのインストールが必要です(無ければ起動時に案内されるはず)
  - x64のデスクトップランタイムで動くはず
  - 持ってない場合は、Visual C++ 再頒布可能パッケージも必要
- ファイルにデータを書き出す(マイセットとかの保存用)ので、ウィルス対策ソフトが文句言ってくる場合があります
- スキルによる防御や耐性上昇は計算していません
- 装飾品の組み合わせは1通りしか検索しないため、他の組み合わせが可能な場合もあります
  - 例：研磨・達人珠【３】1つと攻撃珠【１】1つ→研磨・攻撃珠【３】1つと達人珠【１】1つ
- 検索アルゴリズムの仕様上、1度に1000件や2000件検索するのはとても重くなります
  - 大量に検索するより、追加スキル検索や除外・固定機能をうまく使って絞り込む方がいいです

## ライセンス

The MIT License

### ↑このライセンスって何？

こういう使い方までならOKだよ、ってのを定める取り決め

今回のは大体こんな感じ

- 基本は好きに使ってOK
- でもこのシミュのせいで何か起きても開発者は責任取らんよ
- 改変や再配布するときはよく調べてルールに従ってね

## お問い合わせ

不具合発見時や欲しい機能がある際は、このリポジトリのIssueか、以下の質問箱へどうぞ(質問箱は別のところでも使いまわしているので、このシミュのことだと分かるようにお願いします)

[質問箱](https://peing.net/ja/58b7c250e12e37)

## 使わせていただいたOSS(+必要であればライセンス)

### Google OR-Tools

プロジェクト：<https://github.com/google/or-tools>

ライセンス：<https://github.com/google/or-tools/blob/stable/LICENSE>

### CSV

プロジェクト：<https://github.com/stevehansen/csv/>

ライセンス：<https://raw.githubusercontent.com/stevehansen/csv/master/LICENSE>

### Prism.Wpf

プロジェクト：<https://github.com/PrismLibrary/Prism>

ライセンス：<https://www.nuget.org/packages/Prism.Wpf/8.1.97/license>

### ReactiveProperty

プロジェクト：<https://github.com/runceel/ReactiveProperty>

ライセンス：<https://github.com/runceel/ReactiveProperty/blob/main/LICENSE.txt>

### NLog

プロジェクト：<https://nlog-project.org/>

### DotNetKit.Wpf.AutoCompleteComboBox

プロジェクト：<https://github.com/vain0x/DotNetKit.Wpf.AutoCompleteComboBox/>

ライセンス：<https://www.nuget.org/packages/DotNetKit.Wpf.AutoCompleteComboBox/1.6.0/license>

## スペシャルサンクス

### 5chモンハン板シミュスレの方々

特にVer.13の>>480様の以下論文を大いに参考にしました

<https://github.com/13-480/lp-doc>

### 先人のシミュ作成者様

特に頑シミュ様、泣シミュ様のUIに大きく影響を受けています
