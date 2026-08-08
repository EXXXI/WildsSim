using Moq;
using SimModel.Config;
using SimModel.Domain;
using SimModel.ExceptionClass;
using SimModel.Model;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.RegularExpressions;

namespace SimModelTest.Domain
{
    /// <summary>
    /// FileOperationのテスト
    /// </summary>
    public class FileOperationTest
    {

        #region LoadSkillCSV

        /// <summary>
        /// LoadSkillCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadSkillCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadSkillCSV();

            // サンプルとして防具スキルのデータを抽出して確認
            var sampleSkill = Assert.Single(result, x => x.Name == "防具スキル");
            Assert.Equal(6, sampleSkill.Level);
            Assert.Equal("防具スキル(テスト)", sampleSkill.Category);
            Assert.Empty(sampleSkill.SpecificNames);
            Assert.False(sampleSkill.CanWithArtian);

            // この時点では未実装スキルも存在(DataManagementで削除)
            Assert.Single(result, x => x.Name == "未実装スキル");

            // SpecificNamesの確認
            var groupSkill = Assert.Single(result, x => x.Name == "よわシリーズ");
            Assert.Equal(3, groupSkill.Level);
            Assert.Equal("グループスキル", groupSkill.Category);
            Assert.Equal("よわよわ", groupSkill.SpecificNames[3]);
            Assert.True(groupSkill.CanWithArtian);
            var seriesSkill = Assert.Single(result, x => x.Name == "強欲の力");
            Assert.Equal(4, seriesSkill.Level);
            Assert.Equal("シリーズスキル", seriesSkill.Category);
            Assert.Equal("強欲Ⅰ", seriesSkill.SpecificNames[2]);
            Assert.Equal("強欲Ⅱ", seriesSkill.SpecificNames[4]);
            Assert.False(seriesSkill.CanWithArtian);
        }

        /// <summary>
        /// LoadSkillCSVのテスト(異常系: ファイルが存在しない)
        /// </summary>
        [Fact]
        public void LoadSkillCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_SKILL.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadSkillCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadSkillCSVのテスト(異常系: ファイルの内容が不正)
        /// </summary>
        [Fact]
        public void LoadSkillCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_SKILL.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "スキル系統,発動スキル,必要ポイント,カテゴリ,効果,アーティア対応\r\n" +
                    "武器スキル,,5,武器スキル(テスト),,\r\n" +
                    "スロ1武器スキル,,7,武器スキル(スロット)") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadSkillCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadWeaponCSV

        /// <summary>
        /// LoadWeaponCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadWeaponCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadWeaponCSV();

            // サンプルとして太刀のデータを抽出して確認
            var sampleWeapon = Assert.Single(result, x => x.Name == "スキルつき太刀");
            Assert.Equal(90, sampleWeapon.Attack);
            Assert.Equal(WeaponType.太刀, sampleWeapon.WeaponType);
            Assert.Equal(1, sampleWeapon.Rare);
            Assert.Equal(3, sampleWeapon.Slot1);
            Assert.Equal(2, sampleWeapon.Slot2);
            Assert.Equal(1, sampleWeapon.Slot3);
            Assert.Equal(1, sampleWeapon.SlotType1);
            Assert.Equal(1, sampleWeapon.SlotType2);
            Assert.Equal(1, sampleWeapon.SlotType3);
            Assert.Equal(10, sampleWeapon.Mindef);
            Assert.Equal(10, sampleWeapon.Maxdef);
            Assert.Equal(10, sampleWeapon.TranscendingDef);
            Assert.Equal(0, sampleWeapon.Fire);
            Assert.Equal(0, sampleWeapon.Thunder);
            Assert.Equal(0, sampleWeapon.Water);
            Assert.Equal(0, sampleWeapon.Ice);
            Assert.Equal(0, sampleWeapon.Dragon);
            Assert.False(sampleWeapon.IsOneSet);
            Assert.Equal(int.MaxValue, sampleWeapon.RowNo);
            Assert.False(sampleWeapon.IsVirtual);
            Assert.Null(sampleWeapon.Upper);
            Assert.Single(sampleWeapon.Skills);
            Assert.Equal("武器スキル", sampleWeapon.Skills[0].Name);
            Assert.Equal(1, sampleWeapon.Skills[0].Level);

            // 未実装の読み飛ばし確認
            Assert.DoesNotContain(result, x => x.Name == "未実装大剣");

            // 汎用スロット確認
            Assert.Equal(20, result.Where(x => x.WeaponType == WeaponType.指定なし).Count());
        }

        /// <summary>
        /// LoadWeaponCSVのテスト(正常系・スロット4、未実装許可)
        /// </summary>
        [Fact]
        public void LoadWeaponCSVTest_another()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            config.MaxSlotSize = 4; // スロット数を4に変更してテスト
            config.AllowUnavailableEquipments = true; // 未実装装備を許可してテスト

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadWeaponCSV();

            // 未実装確認
            var unavailableWeapon = Assert.Single(result, x => x.Name == "未実装大剣");
            Assert.Equal(1, unavailableWeapon.RowNo);

            // 汎用スロット確認
            Assert.Equal(35, result.Where(x => x.WeaponType == WeaponType.指定なし).Count());
        }

        /// <summary>
        /// LoadWeaponCSVのテスト(異常系・ファイルの内容が不正)
        /// </summary>
        [Fact]
        public void LoadWeaponCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_WEAPON.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("武器種,名前,レア度,スロット1,スロット2,スロット3,入手時期,表示攻撃力,防御ボーナス,スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,スキル系統4,スキル値4,スキル系統5,スキル値5,スキル系統6,スキル値6,スキル系統7,スキル値7,生産素材1,個数,生産素材2,個数,生産素材3,個数,生産素材4,個数,仮番号\r\n" +
                    "大剣,スキルつき大剣,1,0,0,0,0,90,0,武器スキル,1,,,,,,,,,,,,,,,,,,,,,\r\n" +
                    "大剣ではない,スキルなし大剣,1,0,0,0,0,100,0,,,,,,,,,,,,,,,,,,,,,,,\r\n" +
                    "太刀,スキルつき太刀,1,3,2,1,0,90,10,武器スキル,1,,,,,,,,,,,,,,,,,,,,,\r\n" +
                    "大剣,未実装大剣,2,0,0,0,99,90,0,武器スキル,1,,,,,,,,,,,,,,,,,,,,,1\r\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadWeaponCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        /// <summary>
        /// LoadWeaponCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadWeaponCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_WEAPON.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadWeaponCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        #endregion

        #region LoadHeadCSV

        /// <summary>
        /// LoadHeadCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadHeadCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadHeadCSV(defUpgrades);

            // サンプルとしてつよのデータを抽出して確認
            string kindName = "頭";
            var sampleEquip = Assert.Single(result, x => x.Name == "つよ" + kindName);
            Assert.Equal(EquipKind.head, sampleEquip.Kind);
            Assert.Equal(8, sampleEquip.Rare);
            Assert.Equal(0, sampleEquip.Slot1);
            Assert.Equal(0, sampleEquip.Slot2);
            Assert.Equal(0, sampleEquip.Slot3);
            Assert.Equal(0, sampleEquip.SlotType1);
            Assert.Equal(0, sampleEquip.SlotType2);
            Assert.Equal(0, sampleEquip.SlotType3);
            Assert.Equal(20, sampleEquip.Mindef);
            Assert.Equal(28, sampleEquip.Maxdef);
            Assert.Equal(36, sampleEquip.TranscendingDef);
            Assert.Equal(5, sampleEquip.Fire);
            Assert.Equal(-10, sampleEquip.Water);
            Assert.Equal(-10, sampleEquip.Thunder);
            Assert.Equal(-10, sampleEquip.Ice);
            Assert.Equal(-10, sampleEquip.Dragon);
            Assert.False(sampleEquip.IsOneSet);
            Assert.Equal(20, sampleEquip.RowNo);
            Assert.False(sampleEquip.IsVirtual);
            Assert.Null(sampleEquip.Upper);
            Assert.Equal(5, sampleEquip.Skills.Count);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよシリーズ" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよの力" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "防具スキル" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル全" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル" + kindName && s.Level == 1);

            // セット防具も確認、ついでにスロットも確認
            var setEquip = Assert.Single(result, x => x.Name == "セット" + kindName);
            Assert.True(setEquip.IsOneSet);
            Assert.Equal(2, setEquip.Slot1);
            Assert.Equal(1, setEquip.Slot2);
            Assert.Equal(0, setEquip.Slot3);

            // 未実装の読み飛ばし確認
            Assert.DoesNotContain(result, x => x.Name == "未実装" + kindName);
        }

        /// <summary>
        /// LoadHeadCSVのテスト(正常系・未実装防具を許可)
        /// </summary>
        [Fact]
        public void LoadHeadCSVTest_allowUnaveilable()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            config.AllowUnavailableEquipments = true; // 未実装防具を許可してテスト

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadHeadCSV(defUpgrades);

            // 未実装の確認・強化データ無し時の確認
            string kindName = "頭";
            var equip = Assert.Single(result, x => x.Name == "未実装" + kindName);
            Assert.Equal(10, equip.Mindef);
            Assert.Equal(10, equip.Maxdef);
            Assert.Equal(10, equip.TranscendingDef);
        }

        /// <summary>
        /// LoadHeadCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadHeadCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_HEAD.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadHeadCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadHeadCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadHeadCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_HEAD.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "名前,レア度,スロット1,スロット2,スロット3,入手時期,初期防御力,最終防御力,火耐性,水耐性,雷耐性,氷耐性,龍耐性,スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,スキル系統4,スキル値4,スキル系統5,スキル値5,スキル系統6,スキル値6,スキル系統7,スキル値7,生産素材1,個数,生産素材2,個数,生産素材3,個数,生産素材4,個数,ワンセット,仮番号\n" +
                    "よわ頭,1,0,0,0,0,10,,0,0,0,0,0,よわシリーズ,1,よわの力,1,防具スキル,1,,,,,,,,,,,,,,,,,,10\n" +
                    "つよ頭,8,0,0,0,20,,5,-10,-10,-10,-10,つよシリーズ,1,つよの力,1,防具スキル,1,つよ防具スキル全,1,つよ防具スキル頭,1,,,,,,,,,,,,,,20\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadHeadCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadBodyCSV

        /// <summary>
        /// LoadBodyCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadBodyCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadBodyCSV(defUpgrades);

            // サンプルとしてつよのデータを抽出して確認
            string kindName = "胴";
            var sampleEquip = Assert.Single(result, x => x.Name == "つよ" + kindName);
            Assert.Equal(EquipKind.body, sampleEquip.Kind);
            Assert.Equal(8, sampleEquip.Rare);
            Assert.Equal(0, sampleEquip.Slot1);
            Assert.Equal(0, sampleEquip.Slot2);
            Assert.Equal(0, sampleEquip.Slot3);
            Assert.Equal(0, sampleEquip.SlotType1);
            Assert.Equal(0, sampleEquip.SlotType2);
            Assert.Equal(0, sampleEquip.SlotType3);
            Assert.Equal(20, sampleEquip.Mindef);
            Assert.Equal(28, sampleEquip.Maxdef);
            Assert.Equal(36, sampleEquip.TranscendingDef);
            Assert.Equal(-10, sampleEquip.Fire);
            Assert.Equal(5, sampleEquip.Water);
            Assert.Equal(-10, sampleEquip.Thunder);
            Assert.Equal(-10, sampleEquip.Ice);
            Assert.Equal(-10, sampleEquip.Dragon);
            Assert.False(sampleEquip.IsOneSet);
            Assert.Equal(20, sampleEquip.RowNo);
            Assert.False(sampleEquip.IsVirtual);
            Assert.Null(sampleEquip.Upper);
            Assert.Equal(5, sampleEquip.Skills.Count);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよシリーズ" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよの力" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "防具スキル" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル全" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル" + kindName && s.Level == 1);

            // セット防具も確認、ついでにスロットも確認
            var setEquip = Assert.Single(result, x => x.Name == "セット" + kindName);
            Assert.True(setEquip.IsOneSet);
            Assert.Equal(2, setEquip.Slot1);
            Assert.Equal(1, setEquip.Slot2);
            Assert.Equal(0, setEquip.Slot3);

            // 未実装の読み飛ばし確認
            Assert.DoesNotContain(result, x => x.Name == "未実装" + kindName);
        }

        /// <summary>
        /// LoadBodyCSVのテスト(正常系・未実装防具を許可)
        /// </summary>
        [Fact]
        public void LoadBodyCSVTest_allowUnaveilable()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            config.AllowUnavailableEquipments = true; // 未実装防具を許可してテスト

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadBodyCSV(defUpgrades);

            // 未実装の確認
            string kindName = "胴";
            var equip = Assert.Single(result, x => x.Name == "未実装" + kindName);
            Assert.Equal(10, equip.Mindef);
            Assert.Equal(10, equip.Maxdef);
            Assert.Equal(10, equip.TranscendingDef);
        }

        /// <summary>
        /// LoadBodyCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadBodyCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_BODY.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadBodyCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadBodyCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadBodyCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_BODY.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "名前,レア度,スロット1,スロット2,スロット3,入手時期,初期防御力,最終防御力,火耐性,水耐性,雷耐性,氷耐性,龍耐性,スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,スキル系統4,スキル値4,スキル系統5,スキル値5,スキル系統6,スキル値6,スキル系統7,スキル値7,生産素材1,個数,生産素材2,個数,生産素材3,個数,生産素材4,個数,ワンセット,仮番号\n" +
                    "よわ頭,1,0,0,0,0,10,,0,0,0,0,0,よわシリーズ,1,よわの力,1,防具スキル,1,,,,,,,,,,,,,,,,,,10\n" +
                    "つよ頭,8,0,0,0,20,,5,-10,-10,-10,-10,つよシリーズ,1,つよの力,1,防具スキル,1,つよ防具スキル全,1,つよ防具スキル頭,1,,,,,,,,,,,,,,20\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadBodyCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadArmCSV

        /// <summary>
        /// LoadArmCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadArmCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadArmCSV(defUpgrades);

            // サンプルとしてつよのデータを抽出して確認
            string kindName = "腕";
            var sampleEquip = Assert.Single(result, x => x.Name == "つよ" + kindName);
            Assert.Equal(EquipKind.arm, sampleEquip.Kind);
            Assert.Equal(8, sampleEquip.Rare);
            Assert.Equal(0, sampleEquip.Slot1);
            Assert.Equal(0, sampleEquip.Slot2);
            Assert.Equal(0, sampleEquip.Slot3);
            Assert.Equal(0, sampleEquip.SlotType1);
            Assert.Equal(0, sampleEquip.SlotType2);
            Assert.Equal(0, sampleEquip.SlotType3);
            Assert.Equal(20, sampleEquip.Mindef);
            Assert.Equal(28, sampleEquip.Maxdef);
            Assert.Equal(36, sampleEquip.TranscendingDef);
            Assert.Equal(-10, sampleEquip.Fire);
            Assert.Equal(-10, sampleEquip.Water);
            Assert.Equal(5, sampleEquip.Thunder);
            Assert.Equal(-10, sampleEquip.Ice);
            Assert.Equal(-10, sampleEquip.Dragon);
            Assert.False(sampleEquip.IsOneSet);
            Assert.Equal(20, sampleEquip.RowNo);
            Assert.False(sampleEquip.IsVirtual);
            Assert.Null(sampleEquip.Upper);
            Assert.Equal(5, sampleEquip.Skills.Count);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよシリーズ" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよの力" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "防具スキル" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル全" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル" + kindName && s.Level == 1);

            // セット防具も確認、ついでにスロットも確認
            var setEquip = Assert.Single(result, x => x.Name == "セット" + kindName);
            Assert.True(setEquip.IsOneSet);
            Assert.Equal(2, setEquip.Slot1);
            Assert.Equal(1, setEquip.Slot2);
            Assert.Equal(0, setEquip.Slot3);

            // 未実装の読み飛ばし確認
            Assert.DoesNotContain(result, x => x.Name == "未実装" + kindName);
        }

        /// <summary>
        /// LoadArmCSVのテスト(正常系・未実装防具を許可)
        /// </summary>
        [Fact]
        public void LoadArmCSVTest_allowUnaveilable()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            config.AllowUnavailableEquipments = true; // 未実装防具を許可してテスト

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadArmCSV(defUpgrades);

            // 未実装の確認
            string kindName = "腕";
            var equip = Assert.Single(result, x => x.Name == "未実装" + kindName);
            Assert.Equal(10, equip.Mindef);
            Assert.Equal(10, equip.Maxdef);
            Assert.Equal(10, equip.TranscendingDef);
        }

        /// <summary>
        /// LoadArmCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadArmCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_ARM.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadArmCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadArmCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadArmCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_ARM.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "名前,レア度,スロット1,スロット2,スロット3,入手時期,初期防御力,最終防御力,火耐性,水耐性,雷耐性,氷耐性,龍耐性,スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,スキル系統4,スキル値4,スキル系統5,スキル値5,スキル系統6,スキル値6,スキル系統7,スキル値7,生産素材1,個数,生産素材2,個数,生産素材3,個数,生産素材4,個数,ワンセット,仮番号\n" +
                    "よわ頭,1,0,0,0,0,10,,0,0,0,0,0,よわシリーズ,1,よわの力,1,防具スキル,1,,,,,,,,,,,,,,,,,,10\n" +
                    "つよ頭,8,0,0,0,20,,5,-10,-10,-10,-10,つよシリーズ,1,つよの力,1,防具スキル,1,つよ防具スキル全,1,つよ防具スキル頭,1,,,,,,,,,,,,,,20\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadArmCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadWaistCSV

        /// <summary>
        /// LoadWaistCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadWaistCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadWaistCSV(defUpgrades);

            // サンプルとしてつよのデータを抽出して確認
            string kindName = "腰";
            var sampleEquip = Assert.Single(result, x => x.Name == "つよ" + kindName);
            Assert.Equal(EquipKind.waist, sampleEquip.Kind);
            Assert.Equal(8, sampleEquip.Rare);
            Assert.Equal(0, sampleEquip.Slot1);
            Assert.Equal(0, sampleEquip.Slot2);
            Assert.Equal(0, sampleEquip.Slot3);
            Assert.Equal(0, sampleEquip.SlotType1);
            Assert.Equal(0, sampleEquip.SlotType2);
            Assert.Equal(0, sampleEquip.SlotType3);
            Assert.Equal(20, sampleEquip.Mindef);
            Assert.Equal(28, sampleEquip.Maxdef);
            Assert.Equal(36, sampleEquip.TranscendingDef);
            Assert.Equal(-10, sampleEquip.Fire);
            Assert.Equal(-10, sampleEquip.Water);
            Assert.Equal(-10, sampleEquip.Thunder);
            Assert.Equal(5, sampleEquip.Ice);
            Assert.Equal(-10, sampleEquip.Dragon);
            Assert.False(sampleEquip.IsOneSet);
            Assert.Equal(20, sampleEquip.RowNo);
            Assert.False(sampleEquip.IsVirtual);
            Assert.Null(sampleEquip.Upper);
            Assert.Equal(5, sampleEquip.Skills.Count);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよシリーズ" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよの力" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "防具スキル" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル全" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル" + kindName && s.Level == 1);

            // セット防具も確認、ついでにスロットも確認
            var setEquip = Assert.Single(result, x => x.Name == "セット" + kindName);
            Assert.True(setEquip.IsOneSet);
            Assert.Equal(2, setEquip.Slot1);
            Assert.Equal(1, setEquip.Slot2);
            Assert.Equal(0, setEquip.Slot3);

            // 未実装の読み飛ばし確認
            Assert.DoesNotContain(result, x => x.Name == "未実装" + kindName);
        }

        /// <summary>
        /// LoadWaistCSVのテスト(正常系・未実装防具を許可)
        /// </summary>
        [Fact]
        public void LoadWaistCSVTest_allowUnaveilable()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            config.AllowUnavailableEquipments = true; // 未実装防具を許可してテスト

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadWaistCSV(defUpgrades);

            // 未実装の確認
            string kindName = "腰";
            var equip = Assert.Single(result, x => x.Name == "未実装" + kindName);
            Assert.Equal(10, equip.Mindef);
            Assert.Equal(10, equip.Maxdef);
            Assert.Equal(10, equip.TranscendingDef);
        }

        /// <summary>
        /// LoadWaistCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadWaistCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_WST.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadWaistCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadWaistCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadWaistCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_WST.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "名前,レア度,スロット1,スロット2,スロット3,入手時期,初期防御力,最終防御力,火耐性,水耐性,雷耐性,氷耐性,龍耐性,スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,スキル系統4,スキル値4,スキル系統5,スキル値5,スキル系統6,スキル値6,スキル系統7,スキル値7,生産素材1,個数,生産素材2,個数,生産素材3,個数,生産素材4,個数,ワンセット,仮番号\n" +
                    "よわ頭,1,0,0,0,0,10,,0,0,0,0,0,よわシリーズ,1,よわの力,1,防具スキル,1,,,,,,,,,,,,,,,,,,10\n" +
                    "つよ頭,8,0,0,0,20,,5,-10,-10,-10,-10,つよシリーズ,1,つよの力,1,防具スキル,1,つよ防具スキル全,1,つよ防具スキル頭,1,,,,,,,,,,,,,,20\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadWaistCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadLegCSV

        /// <summary>
        /// LoadLegCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadLegCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadLegCSV(defUpgrades);

            // サンプルとしてつよのデータを抽出して確認
            string kindName = "足";
            var sampleEquip = Assert.Single(result, x => x.Name == "つよ" + kindName);
            Assert.Equal(EquipKind.leg, sampleEquip.Kind);
            Assert.Equal(8, sampleEquip.Rare);
            Assert.Equal(0, sampleEquip.Slot1);
            Assert.Equal(0, sampleEquip.Slot2);
            Assert.Equal(0, sampleEquip.Slot3);
            Assert.Equal(0, sampleEquip.SlotType1);
            Assert.Equal(0, sampleEquip.SlotType2);
            Assert.Equal(0, sampleEquip.SlotType3);
            Assert.Equal(20, sampleEquip.Mindef);
            Assert.Equal(28, sampleEquip.Maxdef);
            Assert.Equal(36, sampleEquip.TranscendingDef);
            Assert.Equal(-10, sampleEquip.Fire);
            Assert.Equal(-10, sampleEquip.Water);
            Assert.Equal(-10, sampleEquip.Thunder);
            Assert.Equal(-10, sampleEquip.Ice);
            Assert.Equal(5, sampleEquip.Dragon);
            Assert.False(sampleEquip.IsOneSet);
            Assert.Equal(20, sampleEquip.RowNo);
            Assert.False(sampleEquip.IsVirtual);
            Assert.Null(sampleEquip.Upper);
            Assert.Equal(5, sampleEquip.Skills.Count);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよシリーズ" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよの力" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "防具スキル" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル全" && s.Level == 1);
            Assert.Single(sampleEquip.Skills, s => s.Name == "つよ防具スキル" + kindName && s.Level == 1);

            // セット防具も確認、ついでにスロットも確認
            var setEquip = Assert.Single(result, x => x.Name == "セット" + kindName);
            Assert.True(setEquip.IsOneSet);
            Assert.Equal(2, setEquip.Slot1);
            Assert.Equal(1, setEquip.Slot2);
            Assert.Equal(0, setEquip.Slot3);

            // 未実装の読み飛ばし確認
            Assert.DoesNotContain(result, x => x.Name == "未実装" + kindName);
        }

        /// <summary>
        /// LoadLegCSVのテスト(正常系・未実装防具を許可)
        /// </summary>
        [Fact]
        public void LoadLegCSVTest_allowUnaveilable()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            config.AllowUnavailableEquipments = true; // 未実装防具を許可してテスト

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadLegCSV(defUpgrades);

            // 未実装の確認
            string kindName = "足";
            var equip = Assert.Single(result, x => x.Name == "未実装" + kindName);
            Assert.Equal(10, equip.Mindef);
            Assert.Equal(10, equip.Maxdef);
            Assert.Equal(10, equip.TranscendingDef);
        }

        /// <summary>
        /// LoadLegCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadLegCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_LEG.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadLegCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadLegCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadLegCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_EQUIP_LEG.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "名前,レア度,スロット1,スロット2,スロット3,入手時期,初期防御力,最終防御力,火耐性,水耐性,雷耐性,氷耐性,龍耐性,スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,スキル系統4,スキル値4,スキル系統5,スキル値5,スキル系統6,スキル値6,スキル系統7,スキル値7,生産素材1,個数,生産素材2,個数,生産素材3,個数,生産素材4,個数,ワンセット,仮番号\n" +
                    "よわ頭,1,0,0,0,0,10,,0,0,0,0,0,よわシリーズ,1,よわの力,1,防具スキル,1,,,,,,,,,,,,,,,,,,10\n" +
                    "つよ頭,8,0,0,0,20,,5,-10,-10,-10,-10,つよシリーズ,1,つよの力,1,防具スキル,1,つよ防具スキル全,1,つよ防具スキル頭,1,,,,,,,,,,,,,,20\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadLegCSV(defUpgrades));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadCharmCSV

        /// <summary>
        /// LoadCharmCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadCharmCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadCharmCSV();

            // データを確認
            var charm = Assert.Single(result);
            Assert.Equal(EquipKind.charm, charm.Kind);
            Assert.Equal(1, charm.Rare);
            Assert.Equal(0, charm.Slot1);
            Assert.Equal(0, charm.Slot2);
            Assert.Equal(0, charm.Slot3);
            Assert.Equal(0, charm.SlotType1);
            Assert.Equal(0, charm.SlotType2);
            Assert.Equal(0, charm.SlotType3);
            Assert.Equal(0, charm.Mindef);
            Assert.Equal(0, charm.Maxdef);
            Assert.Equal(0, charm.TranscendingDef);
            Assert.Equal(0, charm.Fire);
            Assert.Equal(0, charm.Water);
            Assert.Equal(0, charm.Thunder);
            Assert.Equal(0, charm.Ice);
            Assert.Equal(0, charm.Dragon);
            Assert.False(charm.IsOneSet);
            Assert.Equal(int.MaxValue, charm.RowNo);
            Assert.False(charm.IsVirtual);
            Assert.Null(charm.Upper);
            var skill = Assert.Single(charm.Skills);
            Assert.Equal("防具スキル", skill.Name);
            Assert.Equal(1, skill.Level);

            // 未実装の読み飛ばし確認
            Assert.DoesNotContain(result, x => x.Name == "未実装護石Ⅰ");
        }

        /// <summary>
        /// LoadCharmCSVのテスト(正常系・未実装防具を許可)
        /// </summary>
        [Fact]
        public void LoadCharmCSVTest_allowUnaveilable()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            config.AllowUnavailableEquipments = true; // 未実装防具を許可してテスト

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadCharmCSV();

            // 未実装の確認
            Assert.Single(result, x => x.Name == "未実装護石Ⅰ");
        }

        /// <summary>
        /// LoadCharmCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadCharmCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_CHARM.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadCharmCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadCharmCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadCharmCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // defUpGradesを入手(LoadDefupgrades自体のテストは別で行う)
            FileOperation tempFileOperation = new(config, fs);
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_CHARM.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "名前,レア度,入手時期,スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,スキル系統4,スキル値4,スキル系統5,スキル値5,スキル系統6,スキル値6,スキル系統7,スキル値7,生産素材1,個数,生産素材2,個数,生産素材3,個数,生産素材4,個数,仮番号\n" +
                    "テスト護石Ⅰ,1,0,防具スキル,1,,,,,,,,,,,,,,,,,,,,,\n" +
                    "未実装護石Ⅰ,1,防具スキル,1,,,,,,,,,,,,,,,,,,,,,\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadCharmCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadDecoCSV

        /// <summary>
        /// LoadDecoCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadDecoCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadDecoCSV();

            // サンプルとして武三珠【３】のデータを抽出して確認
            var sampleEquip = Assert.Single(result, x => x.Name == "武三珠【３】");
            Assert.Equal(EquipKind.deco, sampleEquip.Kind);
            Assert.Equal(0, sampleEquip.Rare);
            Assert.Equal(3, sampleEquip.Slot1);
            Assert.Equal(0, sampleEquip.Slot2);
            Assert.Equal(0, sampleEquip.Slot3);
            Assert.Equal(1, sampleEquip.SlotType1);
            Assert.Equal(0, sampleEquip.SlotType2);
            Assert.Equal(0, sampleEquip.SlotType3);
            Assert.Equal(0, sampleEquip.Mindef);
            Assert.Equal(0, sampleEquip.Maxdef);
            Assert.Equal(0, sampleEquip.TranscendingDef);
            Assert.Equal(0, sampleEquip.Fire);
            Assert.Equal(0, sampleEquip.Water);
            Assert.Equal(0, sampleEquip.Thunder);
            Assert.Equal(0, sampleEquip.Ice);
            Assert.Equal(0, sampleEquip.Dragon);
            Assert.False(sampleEquip.IsOneSet);
            Assert.Equal(int.MaxValue, sampleEquip.RowNo);
            Assert.False(sampleEquip.IsVirtual);
            Assert.Null(sampleEquip.Upper);
            var skill = Assert.Single(sampleEquip.Skills);
            Assert.Equal("スロ3武器スキル", skill.Name);
            Assert.Equal(1, skill.Level);
            Assert.Equal(skill.Category, sampleEquip.DecoCateory);
            Assert.Equal(7, sampleEquip.DecoCount);

            // 所持数の確認
            var shortDeco = Assert.Single(result, x => x.Name == "不足珠【１】");
            Assert.Equal(1, shortDeco.DecoCount);

            // 未実装の読み飛ばし確認
            Assert.DoesNotContain(result, x => x.Name == "複合珠【４】");
        }

        /// <summary>
        /// LoadDecoCSVのテスト(正常系・未実装防具を許可)
        /// </summary>
        [Fact]
        public void LoadDecoCSVTest_allowUnaveilable()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            config.AllowUnavailableEquipments = true; // 未実装防具を許可してテスト

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadDecoCSV();

            // 未実装の確認、4スロ、複合の確認も実施
            var equip = Assert.Single(result, x => x.Name == "複合珠【４】");
            Assert.Equal("未実装スキル複合", equip.DecoCateory);
            Assert.Equal(0, equip.DecoCount);
        }

        /// <summary>
        /// LoadDecoCSVのテスト(正常系・decocount.jsonなし)
        /// </summary>
        [Fact]
        public void LoadDecoCSVTest_noDecoCount()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 装飾品データのみ取得
            string decoData = File.ReadAllText("MHWilds_DECO.csv");

            // 空のMockFileSystemに装飾品データのみを用意
            string mockFilePath = "MHWilds_DECO.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData(decoData) }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var result = fileOperation.LoadDecoCSV();

            // 所持数の確認
            var shortDeco = Assert.Single(result, x => x.Name == "不足珠【１】");
            Assert.Equal(7, shortDeco.DecoCount);
        }

        /// <summary>
        /// LoadDecoCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadDecoCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            string mockFilePath = "MHWilds_DECO.csv";
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadDecoCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadDecoCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadDecoCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_DECO.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "名前,レア度,スロットサイズ,スロットタイプ,入手時期,スキル系統1,スキル値1,スキル系統2,スキル値2,仮番号\n" +
                    "武一珠【１】,0,1,1,0,スロ1武器スキル,1,,,\n" +
                    "武二珠【２】,1,,,\n" +
                    "武三珠【３】,0,3,1,0,スロ3武器スキル,1,,,") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadDecoCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }


        /// <summary>
        /// LoadDecoCSVのテスト(異常系・所持数ファイルが不正)
        /// </summary>
        [Fact]
        public void LoadDecoCSVTest_invalidCountFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 装飾品データのみ取得
            string decoData = File.ReadAllText("MHWilds_DECO.csv");

            // 不正データ(所持数ファイル)入りのMockFileSystem
            string mockFilePath = "MHWilds_DECO.csv";
            string mockFilePath2 = "save/decocount.json";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData(decoData) },
                    { mockFilePath2, new MockFileData("{{\n  \"武四珠【４】\": 7,\n  \"防四珠【４】\": \"a\",\n  \"両四珠【４】\": 7,\n  \"不足珠【１】\": 1\n}") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadDecoCSV());
            Assert.Equal($"ファイル {mockFilePath2} の読み込みに失敗しました。", ex.Message);
        }

        #endregion

        #region SaveDecoCountJson

        /// <summary>
        /// SaveDecoCountJsonのテスト(正常系)
        /// </summary>
        [Fact]
        public void SaveDecoCountJsonTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<Deco> decos = [
                new Deco() { Name = "試験珠【３】", DecoCount = 3 },
                new Deco() { Name = "試験珠【１】", DecoCount = 1 }
            ];

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveDecoCountJson(decos);

            // 出力を確認
            string saveFile = "save/decocount.json";
            Assert.Equal("{\"試験珠【３】\":3,\"試験珠【１】\":1}", rf.WriteLog[saveFile].Last());
        }

        /// <summary>
        /// SaveDecoCountJsonのテスト(異常系・IOException)
        /// </summary>
        [Fact]
        public void SaveDecoCountJsonTest_IOException()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // IOExceptionを吐くモック
            string mockFilePath = "save/decocount.json";
            var exfile = new Mock<IFile>();
            exfile.Setup(f => f.WriteAllText(mockFilePath, It.IsAny<string>()))
                .Throws(new IOException("Disk full"));
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(exfile.Object);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<Deco> decos = [
                new Deco() { Name = "試験珠【３】", DecoCount = 3 },
                new Deco() { Name = "試験珠【１】", DecoCount = 1 }
            ];

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.SaveDecoCountJson(decos));
            Assert.Equal($"ファイル {mockFilePath} への書き込みに失敗しました。", ex.Message);
        }

        /// <summary>
        /// SaveDecoCountJsonのテスト(異常系・引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveDecoCountJsonTest_null()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveDecoCountJson(null));
        }

        #endregion

        #region LoadCludeCSV

        /// <summary>
        /// LoadCludeCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadCludeCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // テストデータをMockFileSystem上に用意
            string mockFilePath = "save/clude.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "対象,種別\n" +
                    "除外テスト,0\n" +
                    "固定テスト,1\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var result = fileOperation.LoadCludeCSV();

            // データを確認
            Assert.Equal(2, result.Count);
            Assert.Equal("除外テスト", result[0].Name);
            Assert.Equal(CludeKind.exclude, result[0].Kind);
            Assert.Equal("固定テスト", result[1].Name);
            Assert.Equal(CludeKind.include, result[1].Kind);
        }

        /// <summary>
        /// LoadCludeCSVのテスト(正常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadCludeCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var result = fileOperation.LoadCludeCSV();
            Assert.Empty(result);
        }

        /// <summary>
        /// LoadCludeCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadCludeCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "save/clude.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "対象,種別\n" +
                    "除外テスト,0\n" +
                    "固定テスト\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadCludeCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region SaveCludeCSV

        /// <summary>
        /// SaveCludeCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void SaveCludeCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<Clude> cludes = [
                new Clude() { Name = "除外テスト", Kind = CludeKind.exclude },
                new Clude() { Name = "固定テスト", Kind = CludeKind.include }
            ];

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveCludeCSV(cludes);

            // 出力を確認
            string saveFile = "save/clude.csv";
            Assert.Equal("対象,種別\r\n除外テスト,0\r\n固定テスト,1\r\n", rf.WriteLog[saveFile].Last());
        }

        /// <summary>
        /// SaveCludeCSVのテスト(異常系・IOException)
        /// </summary>
        [Fact]
        public void SaveCludeCSVTest_IOException()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // IOExceptionを吐くモック
            string mockFilePath = "save/clude.csv";
            var exfile = new Mock<IFile>();
            exfile.Setup(f => f.WriteAllText(mockFilePath, It.IsAny<string>()))
                .Throws(new IOException("Disk full"));
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(exfile.Object);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<Clude> cludes = [
                new Clude() { Name = "除外テスト", Kind = CludeKind.exclude },
                new Clude() { Name = "固定テスト", Kind = CludeKind.include }
            ];

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.SaveCludeCSV(cludes));
            Assert.Equal($"ファイル {mockFilePath} への書き込みに失敗しました。", ex.Message);
        }

        /// <summary>
        /// SaveCludeCSVのテスト(異常系・引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveCludeCSVTest_null()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveCludeCSV(null));
        }

        #endregion

        #region LoadMySetCSV

        /// <summary>
        /// LoadMySetCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadMySetCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // 装備データが必要なのでロード
            var defUpgrades = fileOperation.LoadDefUpgradeCSV();
            var weapons = fileOperation.LoadWeaponCSV();
            var heads = fileOperation.LoadHeadCSV(defUpgrades);
            var bodies = fileOperation.LoadBodyCSV(defUpgrades);
            var arms = fileOperation.LoadArmCSV(defUpgrades);
            var waists = fileOperation.LoadWaistCSV(defUpgrades);
            var legs = fileOperation.LoadLegCSV(defUpgrades);
            var charms = fileOperation.LoadCharmCSV();
            var decos = fileOperation.LoadDecoCSV();
            var artians = fileOperation.LoadArtianCSV();
            var additionalCharms = fileOperation.LoadAdditionalCharmCSV();
            var allEquipments = weapons.Union(heads).Union(bodies).Union(arms).Union(waists).Union(legs).Union(charms).Union(decos).Union(artians).Union(additionalCharms).ToList();

            // テスト
            var result = fileOperation.LoadMySetCSV(allEquipments);

            // データを抜き出して確認
            // 能力値等の確認はEquipSetのテストで行うので、ここでは確認しない
            var sampleSet = Assert.Single(result, x => x.Name == "ワンセット");
            Assert.Equal("スロットのみ_0-0-0", sampleSet.Weapon.Name);
            Assert.Equal("セット頭", sampleSet.Head.Name);
            Assert.Equal("セット胴", sampleSet.Body.Name);
            Assert.Equal("セット腕", sampleSet.Arm.Name);
            Assert.Equal("セット腰", sampleSet.Waist.Name);
            Assert.Equal("セット足", sampleSet.Leg.Name);
            Assert.Equal("テスト護石Ⅰ", sampleSet.Charm.Name);
            Assert.Equal(5, sampleSet.Decos.Count);
            Assert.Collection(sampleSet.Decos,
                d => Assert.Equal("防二珠【２】", d.Name),
                d => Assert.Equal("防二珠【２】", d.Name),
                d => Assert.Equal("防二珠【２】", d.Name),
                d => Assert.Equal("防二珠【２】", d.Name),
                d => Assert.Equal("防二珠【２】", d.Name)
            );
            Assert.False(sampleSet.IsTranscending);

            // 限界突破の確認
            var trancendingSet = Assert.Single(result, x => x.Name == "ワンセット限界突破");
            Assert.True(trancendingSet.IsTranscending);

            // 追加装備の確認
            var additionalSet = Assert.Single(result, x => x.Name == "追加装備検証");
            Assert.Equal("登録済アーティア", additionalSet.Weapon.DispName);
            Assert.Equal(3, additionalSet.Charm.Slot1);
            Assert.Equal(2, additionalSet.Charm.Slot2);
            Assert.Equal(1, additionalSet.Charm.Slot3);
            Assert.Equal(0, additionalSet.Charm.SlotType1);
            Assert.Equal(0, additionalSet.Charm.SlotType2);
            Assert.Equal(0, additionalSet.Charm.SlotType3);
        }

        /// <summary>
        /// LoadMySetCSVのテスト(正常系・存在しない装備)
        /// </summary>
        [Fact]
        public void LoadMySetCSVTest_notExistEquip()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // 装備データが必要なのでロード
            var defUpgrades = fileOperation.LoadDefUpgradeCSV();
            var weapons = fileOperation.LoadWeaponCSV();
            var heads = fileOperation.LoadHeadCSV(defUpgrades);
            var bodies = fileOperation.LoadBodyCSV(defUpgrades);
            var arms = fileOperation.LoadArmCSV(defUpgrades);
            var waists = fileOperation.LoadWaistCSV(defUpgrades);
            var legs = fileOperation.LoadLegCSV(defUpgrades);
            var charms = fileOperation.LoadCharmCSV();
            var decos = fileOperation.LoadDecoCSV();
            // 追加装備をテストのため除外
            var artians = new List<Weapon>();
            var additionalCharms = new List<Equipment>();
            var allEquipments = weapons.Union(heads).Union(bodies).Union(arms).Union(waists).Union(legs).Union(charms).Union(decos).Union(artians).Union(additionalCharms).ToList();

            // テスト
            var result = fileOperation.LoadMySetCSV(allEquipments);

            // データを抜き出して確認

            // Nameだけ反映された無能力装備に置換されていることを確認
            var additionalSet = Assert.Single(result, x => x.Name == "追加装備検証");
            Assert.Equal("17d1d296-0c36-4b40-a954-87ef527bd2cd", additionalSet.Weapon.Name);
            Assert.Empty(additionalSet.Weapon.Skills);
            Assert.Equal("e0b2395f-5dff-419a-997b-d80218ef647a", additionalSet.Charm.Name);
            Assert.Equal(0, additionalSet.Charm.Slot1);
            Assert.Equal(0, additionalSet.Charm.Slot2);
            Assert.Equal(0, additionalSet.Charm.Slot3);
            Assert.Equal(0, additionalSet.Charm.SlotType1);
            Assert.Equal(0, additionalSet.Charm.SlotType2);
            Assert.Equal(0, additionalSet.Charm.SlotType3);
        }

        /// <summary>
        /// LoadMySetCSVのテスト(正常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadMySetCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            FileOperation tempFileOperation = new(config, fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // 装備データが必要なのでロード
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();
            var weapons = tempFileOperation.LoadWeaponCSV();
            var heads = tempFileOperation.LoadHeadCSV(defUpgrades);
            var bodies = tempFileOperation.LoadBodyCSV(defUpgrades);
            var arms = tempFileOperation.LoadArmCSV(defUpgrades);
            var waists = tempFileOperation.LoadWaistCSV(defUpgrades);
            var legs = tempFileOperation.LoadLegCSV(defUpgrades);
            var charms = tempFileOperation.LoadCharmCSV();
            var decos = tempFileOperation.LoadDecoCSV();
            var artians = tempFileOperation.LoadArtianCSV();
            var additionalCharms = tempFileOperation.LoadAdditionalCharmCSV();
            var allEquipments = weapons.Union(heads).Union(bodies).Union(arms).Union(waists).Union(legs).Union(charms).Union(decos).Union(artians).Union(additionalCharms).ToList();

            // テスト
            var result = fileOperation.LoadMySetCSV(allEquipments);
            Assert.Empty(result);
        }

        /// <summary>
        /// LoadMySetCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadMySetCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            FileOperation tempFileOperation = new(config, fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "save/myset.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("武器,頭,胴,腕,腰,足,護石,装飾品,名前,限界突破有無\r\n" +
                    "スロットのみ_0-0-0,つよ頭,つよ胴,つよ腕,つよ腰,つよ足,テスト護石Ⅰ,,つよセット,1\r\n" +
                    "スロットのみ_0-0-0,セット頭,\r\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // 装備データが必要なのでロード
            var defUpgrades = tempFileOperation.LoadDefUpgradeCSV();
            var weapons = tempFileOperation.LoadWeaponCSV();
            var heads = tempFileOperation.LoadHeadCSV(defUpgrades);
            var bodies = tempFileOperation.LoadBodyCSV(defUpgrades);
            var arms = tempFileOperation.LoadArmCSV(defUpgrades);
            var waists = tempFileOperation.LoadWaistCSV(defUpgrades);
            var legs = tempFileOperation.LoadLegCSV(defUpgrades);
            var charms = tempFileOperation.LoadCharmCSV();
            var decos = tempFileOperation.LoadDecoCSV();
            var artians = tempFileOperation.LoadArtianCSV();
            var additionalCharms = tempFileOperation.LoadAdditionalCharmCSV();
            var allEquipments = weapons.Union(heads).Union(bodies).Union(arms).Union(waists).Union(legs).Union(charms).Union(decos).Union(artians).Union(additionalCharms).ToList();

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadMySetCSV(allEquipments));
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }


        /// <summary>
        /// LoadMySetCSVのテスト(異常系・引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void LoadMySetCSVTest_null()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.LoadMySetCSV(null));
        }

        #endregion

        #region SaveMySetCSV

        /// <summary>
        /// SaveMySetCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void SaveMySetCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);
            FileOperation tempFileOreration = new(config, fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データを用意するため、各種データをロード
            var defUpgrades = fileOperation.LoadDefUpgradeCSV();
            var weapons = fileOperation.LoadWeaponCSV();
            var heads = fileOperation.LoadHeadCSV(defUpgrades);
            var bodies = fileOperation.LoadBodyCSV(defUpgrades);
            var arms = fileOperation.LoadArmCSV(defUpgrades);
            var waists = fileOperation.LoadWaistCSV(defUpgrades);
            var legs = fileOperation.LoadLegCSV(defUpgrades);
            var charms = fileOperation.LoadCharmCSV();
            var decos = fileOperation.LoadDecoCSV();
            var artians = fileOperation.LoadArtianCSV();
            var additionalCharms = fileOperation.LoadAdditionalCharmCSV();
            var allEquipments = weapons.Union(heads).Union(bodies).Union(arms).Union(waists).Union(legs).Union(charms).Union(decos).Union(artians).Union(additionalCharms).ToList();
            var mysets = fileOperation.LoadMySetCSV(allEquipments);
            var expect = File.ReadAllText("save/myset.csv");

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveMySetCSV(mysets);

            // 出力を確認
            string saveFile = "save/myset.csv";
            Assert.Equal(expect.Replace("\r\n", "\n"), rf.WriteLog[saveFile].Last().Replace("\r\n","\n"));
        }

        /// <summary>
        /// SaveMySetCSVのテスト(異常系・IOException)
        /// </summary>
        [Fact]
        public void SaveMySetCSVTest_IOException()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // IOExceptionを吐くモック
            string mockFilePath = "save/myset.csv";
            var exfile = new Mock<IFile>();
            exfile.Setup(f => f.WriteAllText(mockFilePath, It.IsAny<string>()))
                .Throws(new IOException("Disk full"));
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(exfile.Object);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<EquipSet> mysets = [];

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.SaveMySetCSV(mysets));
            Assert.Equal($"ファイル {mockFilePath} への書き込みに失敗しました。", ex.Message);
        }

        /// <summary>
        /// SaveMySetCSVのテスト(異常系・引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveMySetCSVTest_null()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveMySetCSV(null));
        }

        #endregion

        #region LoadRecentSkillCSV

        /// <summary>
        /// LoadRecentSkillCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadRecentSkillCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadRecentSkillCSV();

            // データを確認
            Assert.Equal(2, result.Count);
            Assert.Equal("武器スキル", result[0]);
            Assert.Equal("防具スキル", result[1]);
        }

        /// <summary>
        /// LoadRecentSkillCSVのテスト(正常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadRecentSkillCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var result = fileOperation.LoadRecentSkillCSV();
            Assert.Empty(result);
        }

        /// <summary>
        /// LoadRecentSkillCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadRecentSkillCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "save/recentSkill.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("スキル名あああ\n武器スキル\n防具スキル") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadRecentSkillCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 2行目", ex.Message);
        }

        #endregion

        #region SaveRecentSkillCSV

        /// <summary>
        /// SaveRecentSkillCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void SaveRecentSkillCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<string> skillNames = [
                "武器スキル","防具スキル"
                ];

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveRecentSkillCSV(skillNames);

            // 出力を確認
            string saveFile = "save/recentSkill.csv";
            Assert.Equal("スキル名\r\n武器スキル\r\n防具スキル\r\n", rf.WriteLog[saveFile].Last());
        }

        /// <summary>
        /// SaveRecentSkillCSVのテスト(異常系・IOException)
        /// </summary>
        [Fact]
        public void SaveRecentSkillCSVTest_IOException()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // IOExceptionを吐くモック
            string mockFilePath = "save/recentSkill.csv";
            var exfile = new Mock<IFile>();
            exfile.Setup(f => f.WriteAllText(mockFilePath, It.IsAny<string>()))
                .Throws(new IOException("Disk full"));
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(exfile.Object);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<string> skillNames = [];

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.SaveRecentSkillCSV(skillNames));
            Assert.Equal($"ファイル {mockFilePath} への書き込みに失敗しました。", ex.Message);
        }

        /// <summary>
        /// SaveRecentSkillCSVのテスト(異常系・引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveRecentSkillCSVTest_null()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveRecentSkillCSV(null));
        }

        #endregion

        #region LoadMyConditionCSV

        /// <summary>
        /// LoadMyConditionCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadMyConditionCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadMyConditionCSV();

            // データを確認
            var defCond = Assert.Single(result, cond => cond.DispName == "デフォルト");
            Assert.True(defCond.IsSpecificWeapon);
            Assert.Equal("スロットのみ_0-0-0", defCond.WeaponName);
            Assert.Null(defCond.MinAttack);
            Assert.Null(defCond.Def);
            Assert.Null(defCond.Fire);
            Assert.Null(defCond.Water);
            Assert.Null(defCond.Thunder);
            Assert.Null(defCond.Ice);
            Assert.Null(defCond.Dragon);
            Assert.Equal(2, defCond.Skills.Count);
            Assert.Equal("防具スキル", defCond.Skills[0].Name);
            Assert.Equal(2, defCond.Skills[0].Level);
            Assert.Equal("つよ防具スキル全", defCond.Skills[1].Name);
            Assert.Equal(1, defCond.Skills[1].Level);
            Assert.True(defCond.IsTranscending);

            // 0指定、武器種のみ指定、限界突破無しの確認
            var zeroCond = Assert.Single(result, cond => cond.DispName == "0入力");
            Assert.False(zeroCond.IsSpecificWeapon);
            Assert.Equal(WeaponType.大剣, zeroCond.WeaponType);
            Assert.Equal(20, zeroCond.MinAttack);
            Assert.Equal(0, zeroCond.Def);
            Assert.Equal(0, zeroCond.Fire);
            Assert.Equal(0, zeroCond.Water);
            Assert.Equal(0, zeroCond.Thunder);
            Assert.Equal(0, zeroCond.Ice);
            Assert.Equal(0, zeroCond.Dragon);
            Assert.Equal(2, zeroCond.Skills.Count);
            Assert.False(zeroCond.IsTranscending);

            // 武器指定の確認
            var weaponCond = Assert.Single(result, cond => cond.DispName == "武器指定");
            Assert.True(weaponCond.IsSpecificWeapon);
            Assert.Equal("ホープブレイドⅠ", weaponCond.WeaponName);
        }

        /// <summary>
        /// LoadMyConditionCSVのテスト(正常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadMyConditionCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var result = fileOperation.LoadMyConditionCSV();
            Assert.Empty(result);
        }

        /// <summary>
        /// LoadMyConditionCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadMyConditionCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "save/condition.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("ID,名前,武器指定有無,武器名,武器種,攻撃力,防御力,火耐性,水耐性,雷耐性,氷耐性,龍耐性,スキル,限界突破有無\n" +
                    "ca6cc050-e8c0-4b18-a5af-eec7a177b853,デフォルト,True,スロットのみ_0-0-0,指定なし,null,null,null,null,null,null,null,\"防具スキル,2,つよ防具スキル全,1\",1\n" +
                    "2022a3aa-cf82-409b-a549-434069da0a19,0入力,\",\n" +
                    "9c28cbf1-8fb4-4b7a-8d91-97b1a8d23838,武器指定,True,ホープブレイドⅠ,大剣,null,0,0,0,0,0,0,\"防具スキル,2,つよ防具スキル全,1\",1\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadMyConditionCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region SaveMyConditionCSV

        /// <summary>
        /// SaveMyConditionCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void SaveMyConditionCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データをロード
            var conditions = fileOperation.LoadMyConditionCSV();
            var expect = File.ReadAllText("save/condition.csv");

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveMyConditionCSV(conditions);

            // 出力を確認
            string saveFile = "save/condition.csv";
            Assert.Equal(expect.Replace("\r\n", "\n"), rf.WriteLog[saveFile].Last().Replace("\r\n", "\n"));
        }

        /// <summary>
        /// SaveMyConditionCSVのテスト(異常系・IOException)
        /// </summary>
        [Fact]
        public void SaveMyConditionCSVTest_IOException()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // IOExceptionを吐くモック
            string mockFilePath = "save/condition.csv";
            var exfile = new Mock<IFile>();
            exfile.Setup(f => f.WriteAllText(mockFilePath, It.IsAny<string>()))
                .Throws(new IOException("Disk full"));
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(exfile.Object);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<SearchCondition> conditions = [];

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.SaveMyConditionCSV(conditions));
            Assert.Equal($"ファイル {mockFilePath} への書き込みに失敗しました。", ex.Message);
        }

        /// <summary>
        /// SaveMyConditionCSVのテスト(異常系・引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveMyConditionCSVTest_null()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveMyConditionCSV(null));
        }

        #endregion

        #region LoadAdditionalCharmCSV

        /// <summary>
        /// LoadAdditionalCharmCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadAdditionalCharmCSV();

            // データを確認
            var armorCharm = Assert.Single(result, charm => charm.DispName == "3(防)-2(防)-1(防)");
            Assert.Equal(EquipKind.charm, armorCharm.Kind);
            Assert.Equal(0, armorCharm.Rare);
            Assert.Equal(3, armorCharm.Slot1);
            Assert.Equal(2, armorCharm.Slot2);
            Assert.Equal(1, armorCharm.Slot3);
            Assert.Equal(0, armorCharm.SlotType1);
            Assert.Equal(0, armorCharm.SlotType2);
            Assert.Equal(0, armorCharm.SlotType3);
            Assert.Equal(0, armorCharm.Mindef);
            Assert.Equal(0, armorCharm.Maxdef);
            Assert.Equal(0, armorCharm.TranscendingDef);
            Assert.Equal(0, armorCharm.Fire);
            Assert.Equal(0, armorCharm.Water);
            Assert.Equal(0, armorCharm.Thunder);
            Assert.Equal(0, armorCharm.Ice);
            Assert.Equal(0, armorCharm.Dragon);
            Assert.False(armorCharm.IsOneSet);
            Assert.Equal(int.MaxValue, armorCharm.RowNo);
            Assert.False(armorCharm.IsVirtual);
            Assert.Null(armorCharm.Upper);
            Assert.Empty(armorCharm.Skills);

            // 武器スロットデータを確認
            var weaponCharm = Assert.Single(result, charm => charm.DispName == "3(武)-2(武)-1(武)");
            Assert.Equal(3, weaponCharm.Slot1);
            Assert.Equal(2, weaponCharm.Slot2);
            Assert.Equal(1, weaponCharm.Slot3);
            Assert.Equal(1, weaponCharm.SlotType1);
            Assert.Equal(1, weaponCharm.SlotType2);
            Assert.Equal(1, weaponCharm.SlotType3);

            // 両対応スロットデータを確認
            var doubleCharm = Assert.Single(result, charm => charm.DispName == "3(両)-2(両)-1(両)");
            Assert.Equal(3, doubleCharm.Slot1);
            Assert.Equal(2, doubleCharm.Slot2);
            Assert.Equal(1, doubleCharm.Slot3);
            Assert.Equal(2, doubleCharm.SlotType1);
            Assert.Equal(2, doubleCharm.SlotType2);
            Assert.Equal(2, doubleCharm.SlotType3);

            // スキルデータを確認
            var skillCharm = Assert.Single(result, charm => charm.DispName == "スロ1不足スキルLv2, 0-0-0");
            var skill = Assert.Single(skillCharm.Skills);
            Assert.Equal("スロ1不足スキル", skill.Name);
            Assert.Equal(2, skill.Level);
        }
        /// <summary>
        /// LoadAdditionalCharmCSVのテスト(正常系・泣シミュフォーマット)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmCSVTest_another()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 泣シミュフォーマットデータ入りのMockFileSystem
            string mockFilePath = "save/additionalCharm.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,(泣用防具スロ1),(泣用防具スロ2),(泣用防具スロ3),(泣用武器スロ1),(泣用武器スロ2),(泣用武器スロ3),スロット1,スロット2,スロット3,スロット1タイプ,スロット2タイプ,スロット3タイプ,内部管理ID,マイセット登録有無\r\n" +
                    ",,,,,,3,2,1,0,0,0\r\n" +
                    ",,,,,,0,0,0,3,2,1\r\n" +
                    "スロ1不足スキル,2,,,,,0,0,0,0,0,0") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var result = fileOperation.LoadAdditionalCharmCSV();

            // データを確認
            var armorCharm = Assert.Single(result, charm => charm.DispName == "3(防)-2(防)-1(防)");
            Assert.Equal(EquipKind.charm, armorCharm.Kind);
            Assert.Equal(0, armorCharm.Rare);
            Assert.Equal(3, armorCharm.Slot1);
            Assert.Equal(2, armorCharm.Slot2);
            Assert.Equal(1, armorCharm.Slot3);
            Assert.Equal(0, armorCharm.SlotType1);
            Assert.Equal(0, armorCharm.SlotType2);
            Assert.Equal(0, armorCharm.SlotType3);
            Assert.Equal(0, armorCharm.Mindef);
            Assert.Equal(0, armorCharm.Maxdef);
            Assert.Equal(0, armorCharm.TranscendingDef);
            Assert.Equal(0, armorCharm.Fire);
            Assert.Equal(0, armorCharm.Water);
            Assert.Equal(0, armorCharm.Thunder);
            Assert.Equal(0, armorCharm.Ice);
            Assert.Equal(0, armorCharm.Dragon);
            Assert.False(armorCharm.IsOneSet);
            Assert.Equal(int.MaxValue, armorCharm.RowNo);
            Assert.False(armorCharm.IsVirtual);
            Assert.Null(armorCharm.Upper);
            Assert.Empty(armorCharm.Skills);

            // 武器スロットデータを確認
            var weaponCharm = Assert.Single(result, charm => charm.DispName == "3(武)-2(武)-1(武)");
            Assert.Equal(3, weaponCharm.Slot1);
            Assert.Equal(2, weaponCharm.Slot2);
            Assert.Equal(1, weaponCharm.Slot3);
            Assert.Equal(1, weaponCharm.SlotType1);
            Assert.Equal(1, weaponCharm.SlotType2);
            Assert.Equal(1, weaponCharm.SlotType3);

            // スキルデータを確認
            var skillCharm = Assert.Single(result, charm => charm.DispName == "スロ1不足スキルLv2, 0-0-0");
            var skill = Assert.Single(skillCharm.Skills);
            Assert.Equal("スロ1不足スキル", skill.Name);
            Assert.Equal(2, skill.Level);
        }

        /// <summary>
        /// LoadAdditionalCharmCSVのテスト(正常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var result = fileOperation.LoadAdditionalCharmCSV();
            Assert.Empty(result);
        }

        /// <summary>
        /// LoadAdditionalCharmCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "save/additionalCharm.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "スキル系統1,スキル値1,スキル系統2,スキル値2,スキル系統3,スキル値3,(泣用防具スロ1),(泣用防具スロ2),(泣用防具スロ3),(泣用武器スロ1),(泣用武器スロ2),(泣用武器スロ3),スロット1,スロット2,スロット3,スロット1タイプ,スロット2タイプ,スロット3タイプ,内部管理ID,マイセット登録有無\r\n" +
                    ",,,,,,3,2,1,0,0,0,3,2,1,0,0,0,e0b2395f-5dff-419a-997b-d80218ef647a,\r\n" +
                    ",,,\r\n" +
                    ",,,,,,0,0,0,0,0,0,3,2,1,2,2,2,568efef3-ee34-4fd6-938a-d80218ef647a,\r\n" +
                    "スロ1不足スキル,2,,,,,0,0,0,0,0,0,0,0,0,0,0,0,568ewwww-ee34-4fd6-938a-d80218ef647a,") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadAdditionalCharmCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region SaveAdditionalCharmCSV

        /// <summary>
        /// SaveAdditionalCharmCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void SaveAdditionalCharmCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データをロード
            var charms = fileOperation.LoadAdditionalCharmCSV();
            var expect = File.ReadAllText("save/additionalCharm.csv");

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveAdditionalCharmCSV(charms, []);

            // 出力を確認
            string saveFile = "save/additionalCharm.csv";
            Assert.Equal(expect.Replace("\r\n", "\n"), rf.WriteLog[saveFile].Last().Replace("\r\n", "\n"));
        }

        /// <summary>
        /// SaveAdditionalCharmCSVのテスト(正常系・マイセット反映)
        /// </summary>
        [Fact]
        public void SaveAdditionalCharmCSVTest_myset()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データとマイセットデータをロード
            var defUpgrades = fileOperation.LoadDefUpgradeCSV();
            var weapons = fileOperation.LoadWeaponCSV();
            var heads = fileOperation.LoadHeadCSV(defUpgrades);
            var bodies = fileOperation.LoadBodyCSV(defUpgrades);
            var arms = fileOperation.LoadArmCSV(defUpgrades);
            var waists = fileOperation.LoadWaistCSV(defUpgrades);
            var legs = fileOperation.LoadLegCSV(defUpgrades);
            var charms = fileOperation.LoadCharmCSV();
            var decos = fileOperation.LoadDecoCSV();
            var artians = fileOperation.LoadArtianCSV();
            var additionalCharms = fileOperation.LoadAdditionalCharmCSV();
            var allEquipments = weapons.Union(heads).Union(bodies).Union(arms).Union(waists).Union(legs).Union(charms).Union(decos).Union(artians).Union(additionalCharms).ToList();
            var mysets = fileOperation.LoadMySetCSV(allEquipments);

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveAdditionalCharmCSV(additionalCharms, mysets);

            // 出力を確認
            string saveFile = "save/additionalCharm.csv";
            int count = Regex.Count(rf.WriteLog[saveFile].Last(), @"マイセット登録中");
            Assert.Equal(1, count);
        }

        /// <summary>
        /// SaveAdditionalCharmCSVのテスト(異常系・IOException)
        /// </summary>
        [Fact]
        public void SaveAdditionalCharmCSVTest_IOException()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // IOExceptionを吐くモック
            string mockFilePath = "save/additionalCharm.csv";
            var exfile = new Mock<IFile>();
            exfile.Setup(f => f.WriteAllText(mockFilePath, It.IsAny<string>()))
                .Throws(new IOException("Disk full"));
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(exfile.Object);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<Equipment> charms = [];

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.SaveAdditionalCharmCSV(charms, []));
            Assert.Equal($"ファイル {mockFilePath} への書き込みに失敗しました。", ex.Message);
        }

        /// <summary>
        /// SaveAdditionalCharmCSVのテスト(異常系・引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveAdditionalCharmCSVTest_null()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveAdditionalCharmCSV(null, []));
        }

        /// <summary>
        /// SaveAdditionalCharmCSVのテスト(異常系・マイセットの引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveAdditionalCharmCSVTest_mysetNull()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データをロード
            var charms = fileOperation.LoadAdditionalCharmCSV();

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveAdditionalCharmCSV(charms, null));
        }

        #endregion

        #region LoadAdditionalCharmComboCSV

        /// <summary>
        /// LoadAdditionalCharmComboCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmComboCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadAdditionalCharmComboCSV();

            // データを確認
            var rare5conbo = Assert.Single(result, conbo => conbo.Rare == 5);
            Assert.Equal(1, rare5conbo.Group1);
            Assert.Equal(0, rare5conbo.Group2);
            Assert.Equal(0, rare5conbo.Group3);
            Assert.Equal(1, rare5conbo.Slot1);
            Assert.Equal(0, rare5conbo.Slot2);
            Assert.Equal(0, rare5conbo.Slot3);
            Assert.Equal(0, rare5conbo.SlotType1);
            Assert.Equal(0, rare5conbo.SlotType2);
            Assert.Equal(0, rare5conbo.SlotType3);
            var rare8conbo = Assert.Single(result, conbo => conbo.Rare == 8);
            Assert.Equal(2, rare8conbo.Group1);
            Assert.Equal(0, rare8conbo.Group2);
            Assert.Equal(0, rare8conbo.Group3);
            Assert.Equal(1, rare8conbo.Slot1);
            Assert.Equal(1, rare8conbo.Slot2);
            Assert.Equal(0, rare8conbo.Slot3);
            Assert.Equal(1, rare8conbo.SlotType1);
            Assert.Equal(0, rare8conbo.SlotType2);
            Assert.Equal(0, rare8conbo.SlotType3);
        }

        /// <summary>
        /// LoadAdditionalCharmComboCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmComboCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);
            string mockFilePath = "MHWilds_COMBO_SHININGCHARM.csv";

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadAdditionalCharmComboCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);

        }

        /// <summary>
        /// LoadAdditionalCharmComboCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmComboCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_COMBO_SHININGCHARM.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "レア度,グループ1,グループ2,グループ3,スロット1,スロット2,スロット3,スロット1タイプ,スロット2タイプ,スロット3タイプ\r\n" +
                    "5,1,0,0,1,0,0,0,0,0\r\n" +
                    "8,2,0,,0\r\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadAdditionalCharmComboCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadAdditionalCharmGroupCSV

        /// <summary>
        /// LoadAdditionalCharmGroupCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmGroupCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadAdditionalCharmGroupCSV();

            // データを確認
            var group1skill = Assert.Single(result[1]);
            Assert.Equal("防具スキル", group1skill.Name);
            Assert.Equal(1, group1skill.Level);
            var group2skill = Assert.Single(result[2]);
            Assert.Equal("つよ防具スキル", group2skill.Name);
            Assert.Equal(1, group2skill.Level);
        }

        /// <summary>
        /// LoadAdditionalCharmGroupCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmGroupCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);
            string mockFilePath = "MHWilds_GROUP_SHININGCHARM.csv";

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadAdditionalCharmGroupCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadAdditionalCharmGroupCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadAdditionalCharmGroupCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_GROUP_SHININGCHARM.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "グループ,スキル名,レベル\r\n" +
                    "1,防具スキル,1\r\n" +
                    "2,1\r\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadAdditionalCharmGroupCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadDefUpgradeCSV

        /// <summary>
        /// LoadDefUpgradeCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadDefUpgradeCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadDefUpgradeCSV();

            // データを確認;
            Assert.Equal(10, result[5].UpgradeDef);
            Assert.Equal(8, result[8].TranscendingDef);
        }

        /// <summary>
        /// LoadDefUpgradeCSVのテスト(異常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadDefUpgradeCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);
            string mockFilePath = "MHWilds_DEF_UPGRADE.csv";

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadDefUpgradeCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: ファイル本体", ex.Message);
        }

        /// <summary>
        /// LoadDefUpgradeCSVのテスト(異常系・不正なファイル)
        /// </summary>
        [Fact]
        public void LoadDefUpgradeCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "MHWilds_DEF_UPGRADE.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("レア度,最大強化,限界突破強化\r\n" +
                    "1,11,11\r\n" +
                    "5,\r\n" +
                    "6,9,9\r\n" +
                    "8,8,8\r\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadDefUpgradeCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 3行目", ex.Message);
        }

        #endregion

        #region LoadArtianCSV

        /// <summary>
        /// LoadArtianCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void LoadArtianCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 通常のファイルはFileSystem上に用意
            FileOperation fileOperation = new(config, fs);

            // テスト
            var result = fileOperation.LoadArtianCSV();

            // サンプルデータを抽出して確認
            var sampleWeapon = Assert.Single(result, x => x.DispName == "登録済アーティア");
            Assert.Equal(190, sampleWeapon.Attack);
            Assert.Equal(WeaponType.大剣, sampleWeapon.WeaponType);
            Assert.Equal(8, sampleWeapon.Rare);
            Assert.Equal(3, sampleWeapon.Slot1);
            Assert.Equal(3, sampleWeapon.Slot2);
            Assert.Equal(3, sampleWeapon.Slot3);
            Assert.Equal(1, sampleWeapon.SlotType1);
            Assert.Equal(1, sampleWeapon.SlotType2);
            Assert.Equal(1, sampleWeapon.SlotType3);
            Assert.Equal(0, sampleWeapon.Mindef);
            Assert.Equal(0, sampleWeapon.Maxdef);
            Assert.Equal(0, sampleWeapon.TranscendingDef);
            Assert.Equal(0, sampleWeapon.Fire);
            Assert.Equal(0, sampleWeapon.Thunder);
            Assert.Equal(0, sampleWeapon.Water);
            Assert.Equal(0, sampleWeapon.Ice);
            Assert.Equal(0, sampleWeapon.Dragon);
            Assert.False(sampleWeapon.IsOneSet);
            Assert.Equal(int.MaxValue, sampleWeapon.RowNo);
            Assert.False(sampleWeapon.IsVirtual);
            Assert.Null(sampleWeapon.Upper);
            Assert.Equal(2, sampleWeapon.Skills.Count);
            Assert.Equal("よわシリーズ", sampleWeapon.Skills[0].Name);
            Assert.Equal(1, sampleWeapon.Skills[0].Level);
            Assert.Equal("つよの力", sampleWeapon.Skills[1].Name);
            Assert.Equal(1, sampleWeapon.Skills[1].Level);
        }

        /// <summary>
        /// LoadArtianCSVのテスト(異常系・ファイルの内容が不正)
        /// </summary>
        [Fact]
        public void LoadArtianCSVTest_invalidFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 不正データ入りのMockFileSystem
            string mockFilePath = "save/artian.csv";
            IFileSystem mockfs = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { mockFilePath, new MockFileData("" +
                    "武器種,名前,スキル系統1,スキル値1,スキル系統2,スキル値2,内部管理ID,マイセット登録有無\r\n" +
                    "大剣,登録済アーティア,\r\n") }
                });
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.LoadArtianCSV());
            Assert.Equal($"ファイル {mockFilePath} の読み込みに失敗しました。エラー箇所: 2行目", ex.Message);
        }

        /// <summary>
        /// LoadArtianCSVのテスト(正常系・ファイルなし)
        /// </summary>
        [Fact]
        public void LoadArtianCSVTest_noFile()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            var result = fileOperation.LoadArtianCSV();
            Assert.Empty(result);
        }

        #endregion

        #region SaveArtianCSV

        /// <summary>
        /// SaveArtianCSVのテスト(正常系)
        /// </summary>
        [Fact]
        public void SaveArtianCSVTest_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データをロード
            var artians = fileOperation.LoadArtianCSV();
            var expect = File.ReadAllText("save/artian.csv");

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveArtianCSV(artians, []);

            // 出力を確認
            string saveFile = "save/artian.csv";
            Assert.Equal(expect.Replace("\r\n", "\n"), rf.WriteLog[saveFile].Last().Replace("\r\n", "\n"));
        }

        /// <summary>
        /// SaveArtianCSVのテスト(正常系・マイセット反映)
        /// </summary>
        [Fact]
        public void SaveArtianCSVTest_myset()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データとマイセットデータをロード
            var defUpgrades = fileOperation.LoadDefUpgradeCSV();
            var weapons = fileOperation.LoadWeaponCSV();
            var heads = fileOperation.LoadHeadCSV(defUpgrades);
            var bodies = fileOperation.LoadBodyCSV(defUpgrades);
            var arms = fileOperation.LoadArmCSV(defUpgrades);
            var waists = fileOperation.LoadWaistCSV(defUpgrades);
            var legs = fileOperation.LoadLegCSV(defUpgrades);
            var charms = fileOperation.LoadCharmCSV();
            var decos = fileOperation.LoadDecoCSV();
            var artians = fileOperation.LoadArtianCSV();
            var additionalCharms = fileOperation.LoadAdditionalCharmCSV();
            var allEquipments = weapons.Union(heads).Union(bodies).Union(arms).Union(waists).Union(legs).Union(charms).Union(decos).Union(artians).Union(additionalCharms).ToList();
            var mysets = fileOperation.LoadMySetCSV(allEquipments);

            // テスト
            rf.ClearAllWriteLog();
            fileOperation.SaveArtianCSV(artians, mysets);

            // 出力を確認
            string saveFile = "save/artian.csv";
            int count = Regex.Count(rf.WriteLog[saveFile].Last(), @"マイセット登録中");
            Assert.Equal(1, count);
        }

        /// <summary>
        /// SaveArtianCSVのテスト(異常系・IOException)
        /// </summary>
        [Fact]
        public void SaveArtianCSVTest_IOException()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // IOExceptionを吐くモック
            string mockFilePath = "save/artian.csv";
            var exfile = new Mock<IFile>();
            exfile.Setup(f => f.WriteAllText(mockFilePath, It.IsAny<string>()))
                .Throws(new IOException("Disk full"));
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(exfile.Object);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データ
            List<Weapon> artians = [];

            // テスト
            var ex = Assert.Throws<SimulatorException>(() => fileOperation.SaveArtianCSV(artians, []));
            Assert.Equal($"ファイル {mockFilePath} への書き込みに失敗しました。", ex.Message);
        }

        /// <summary>
        /// SaveArtianCSVのテスト(異常系・引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveArtianCSVTest_null()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveArtianCSV(null, []));
        }

        /// <summary>
        /// SaveArtianCSVのテスト(異常系・マイセットの引数がnull)
        /// 想定していない呼び出し方なので、例外で止まることだけを確認する
        /// </summary>
        [Fact]
        public void SaveArtianCSVTest_mysetNull()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // ファイル出力のかわりにReadOnlyFileでログ出力
            var rf = new ReadOnlyFile(fs);
            var mockfs = new Mock<IFileSystem>();
            mockfs.SetupGet(x => x.File).Returns(rf);
            FileOperation fileOperation = new(config, mockfs.Object);

            // 出力用データをロード
            var artians = fileOperation.LoadArtianCSV();

            // テスト
            Assert.ThrowsAny<Exception>(() => fileOperation.SaveArtianCSV(artians, null));
        }

        #endregion

        #region MakeSaveFolder

        /// <summary>
        /// MakeSaveFolderのテスト(正常系)
        /// </summary>
        [Fact]
        public void MakeSaveFolder_normal()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // 空のMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            fileOperation.MakeSaveFolder();
            Assert.True(mockfs.Directory.Exists("save"));
        }

        /// <summary>
        /// MakeSaveFolderのテスト(正常系・すでに存在する)
        /// </summary>
        [Fact]
        public void MakeSaveFolder_alreadyExist()
        {
            // 通常のファイルはFileSystem上に用意
            IFileSystem fs = new FileSystem();
            LogicConfig config = new(fs);

            // saveのみのMockFileSystem
            IFileSystem mockfs = new MockFileSystem();
            mockfs.Directory.CreateDirectory("save");
            FileOperation fileOperation = new(config, mockfs);

            // テスト
            fileOperation.MakeSaveFolder();
            Assert.True(mockfs.Directory.Exists("save"));
        }

        #endregion

    }
}
