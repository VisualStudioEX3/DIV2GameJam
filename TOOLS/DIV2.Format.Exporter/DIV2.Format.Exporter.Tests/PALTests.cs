using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DIV2.Format.Exporter.Tests
{
    [TestClass]
    public class PALTests : AbstractTest
    {
        #region Intenral vars
        PAL _palette;
        #endregion

        #region Helper Functions
        public PAL GenerateTestPalette()
        {
            if (this._palette is null)
            {
                this._palette = new PAL();

                for (int i = 0; i < PAL.LENGTH; i++)
                    this._palette[i] = new Color(i / 4, i / 4, i / 4);
            }

            return this._palette;
        }
        #endregion

        #region Test methods
        [TestMethod]
        public void ValidateFile()
        {
            Assert.IsTrue(PAL.ValidateFormat(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV)));
        }

        [TestMethod]
        public void ValidateBuffer()
        {
            byte[] buffer = File.ReadAllBytes(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));
            Assert.IsTrue(PAL.ValidateFormat(buffer));
        }

        [TestMethod]
        public void FailValidateFile()
        {
            Assert.IsFalse(PAL.ValidateFormat(this.GetAssetPath(SharedConstants.FILENAME_IMG_PLAYER_BMP)));
        }

        [TestMethod]
        public void FailValidateBuffer()
        {
            byte[] buffer = File.ReadAllBytes(this.GetAssetPath(SharedConstants.FILENAME_IMG_PLAYER_BMP));
            Assert.IsFalse(PAL.ValidateFormat(buffer));
        }

        [TestMethod]
        public void CreateNewPalette()
        {
            this.GenerateTestPalette();
        }

        [TestMethod]
        public void ReadColorsByIndex()
        {
            Color color;
            var pal = this.GenerateTestPalette();
            for (int i = 0; i < PAL.LENGTH; i++)
                color = pal[i];
        }

        [TestMethod]
        public void ReadColorsByForEach()
        {
            Color color;
            var pal = this.GenerateTestPalette();
            foreach (var value in pal)
                color = value;
        }

        [TestMethod]
        public void SaveFile()
        {
            var pal = this.GenerateTestPalette();
            pal.Save(this.GetOutputPath("TEST.PAL"));
        }

        [TestMethod]
        public void LoadFromFilename()
        {
            new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));
        }

        [TestMethod]
        public void LoadFromBuffer()
        {
            byte[] buffer = File.ReadAllBytes(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));
            new PAL(buffer);
        }

        [DataTestMethod]
        [DataRow(SharedConstants.FILENAME_IMG_PLAYER_PCX)]
        [DataRow(SharedConstants.FILENAME_IMG_PLAYER_BMP)]
        [DataRow(SharedConstants.FILENAME_IMG_PLAYER_PNG)]
        [DataRow(SharedConstants.FILENAME_IMG_PLAYER_MAP)]
        [DataRow(SharedConstants.FILENAME_IMG_PLAYER_FPG)]
        public void CreateFromImage(string file)
        {
            var pal = PAL.FromImage(this.GetAssetPath(file));
            string saveFilename = $"{file}.PAL";
            pal.Save(this.GetOutputPath(saveFilename));
        }

        [TestMethod]
        public void AreEqual()
        {
            var a = new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));
            var b = new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));

            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void AreNotEqual()
        {
            var a = new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));
            var b = new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_SPACE));

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void AreEqualByCompare()
        {
            var a = new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));
            var b = new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));

            Assert.IsTrue(a.Compare(b));
        }

        [TestMethod]
        public void AreNotEqualByCompare()
        {
            var a = new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_DIV));
            var b = new PAL(this.GetAssetPath(SharedConstants.FILENAME_PAL_SPACE));

            Assert.IsFalse(a.Compare(b));
        }
        #endregion
    }
}
