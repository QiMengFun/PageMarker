using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sunny.UI;

namespace PageMarker.Forms
{
    public partial class MainForm : UIForm
    {
        private string _appDir;
        private string _imgSrcDir;
        private string _outputDir;
        private string _configFile;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text = "页码标注工具";

            _appDir = AppDomain.CurrentDomain.BaseDirectory;
            _imgSrcDir = Path.Combine(_appDir, "imgsrc");
            _outputDir = Path.Combine(_appDir, "output");
            _configFile = Path.Combine(_appDir, "config.ini");

            // 初始化字体列表
            uiComboBox1.Items.Clear();
            var fonts = new List<string>
            {
                "宋体", "黑体", "微软雅黑", "楷体", "Arial", "Times New Roman", "Consolas"
            };
            uiComboBox1.Items.AddRange(fonts.ToArray());

            uiProcessBar1.Maximum = 100;
            uiProcessBar1.Value = 0;
            uiProcessBar1.Text = "就绪";

            uiButton1.Click += UiButton1_Click;
            uiButton2.Click += UiButton2_Click;

            // 加载已保存的配置，如果没有则使用默认值
            LoadConfig();
        }

        /// <summary>
        /// 窗体关闭时保存配置
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveConfig();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        private void SaveConfig()
        {
            try
            {
                var lines = new List<string>
                {
                    "FontName=" + uiComboBox1.Text,
                    "FontSize=" + uiTextBox2.Text,
                    "StrokeSize=" + uiTextBox5.Text,
                    "MarginX=" + uiTextBox3.Text,
                    "MarginY=" + uiTextBox4.Text,
                    "PageStartNumber=" + uiTextBox6.Text,
                    "FileStartNumber=" + uiTextBox7.Text,
                    "FontColor=" + ColorToArgb(uiColorPicker1.Value),
                    "StrokeColor=" + ColorToArgb(uiColorPicker2.Value),
                    "PageMode=" + (uiRadioButton1.Checked ? "0" : uiRadioButton2.Checked ? "1" : "2"),
                    "Bold=" + (jiacu.Checked ? "1" : "0"),
                    "ScaleMargin=" + uiTextBox1.Text,
                    "NoScaleForSkipped=" + (uiCheckBox1.Checked ? "1" : "0")
                };
                File.WriteAllLines(_configFile, lines);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        private void LoadConfig()
        {
            // 默认值
            uiComboBox1.SelectedIndex = 2;
            uiTextBox2.Text = "36";
            uiTextBox3.Text = "30";
            uiTextBox4.Text = "30";
            uiTextBox5.Text = "3";
            uiTextBox6.Text = "1";
            uiTextBox7.Text = "1";
            uiColorPicker1.Value = Color.White;
            uiColorPicker2.Value = Color.Black;
            uiRadioButton1.Checked = true;
            uiRadioButton2.Checked = false;
            uiRadioButton3.Checked = false;
            jiacu.Checked = false;
            uiTextBox1.Text = "0";
            uiCheckBox1.Checked = false;

            if (!File.Exists(_configFile))
                return;

            try
            {
                var dict = File.ReadAllLines(_configFile)
                    .Select(line => line.Split(new[] { '=' }, 2))
                    .Where(parts => parts.Length == 2)
                    .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());

                if (dict.ContainsKey("FontName"))
                {
                    int idx = uiComboBox1.Items.IndexOf(dict["FontName"]);
                    if (idx >= 0) uiComboBox1.SelectedIndex = idx;
                    else
                    {
                        uiComboBox1.Text = dict["FontName"];
                    }
                }
                if (dict.ContainsKey("FontSize")) uiTextBox2.Text = dict["FontSize"];
                if (dict.ContainsKey("StrokeSize")) uiTextBox5.Text = dict["StrokeSize"];
                if (dict.ContainsKey("MarginX")) uiTextBox3.Text = dict["MarginX"];
                if (dict.ContainsKey("MarginY")) uiTextBox4.Text = dict["MarginY"];
                if (dict.ContainsKey("PageStartNumber")) uiTextBox6.Text = dict["PageStartNumber"];
                if (dict.ContainsKey("FileStartNumber")) uiTextBox7.Text = dict["FileStartNumber"];
                if (dict.ContainsKey("FontColor")) uiColorPicker1.Value = ArgbToColor(dict["FontColor"]);
                if (dict.ContainsKey("StrokeColor")) uiColorPicker2.Value = ArgbToColor(dict["StrokeColor"]);
                if (dict.ContainsKey("PageMode"))
                {
                    int mode;
                    int.TryParse(dict["PageMode"], out mode);
                    uiRadioButton1.Checked = (mode == 0);
                    uiRadioButton2.Checked = (mode == 1);
                    uiRadioButton3.Checked = (mode == 2);
                }
                if (dict.ContainsKey("Bold"))
                {
                    jiacu.Checked = dict["Bold"] == "1";
                }
                if (dict.ContainsKey("ScaleMargin"))
                {
                    uiTextBox1.Text = dict["ScaleMargin"];
                }
                if (dict.ContainsKey("NoScaleForSkipped"))
                {
                    uiCheckBox1.Checked = dict["NoScaleForSkipped"] == "1";
                }
            }
            catch
            {
            }
        }

        private string ColorToArgb(Color c)
        {
            return c.ToArgb().ToString();
        }

        private Color ArgbToColor(string val)
        {
            int argb;
            if (int.TryParse(val, out argb))
                return Color.FromArgb(argb);
            return Color.White;
        }

        /// <summary>
        /// 获取当前界面上的配置参数
        /// </summary>
        private PageMarkConfig GetCurrentConfig()
        {
            return new PageMarkConfig
            {
                FontName = uiComboBox1.Text,
                FontSize = float.Parse(uiTextBox2.Text),
                StrokeSize = float.Parse(uiTextBox5.Text),
                MarginX = int.Parse(uiTextBox3.Text),
                MarginY = int.Parse(uiTextBox4.Text),
                PageStartNumber = int.Parse(uiTextBox6.Text),
                FileStartNumber = int.Parse(uiTextBox7.Text),
                FontColor = uiColorPicker1.Value,
                StrokeColor = uiColorPicker2.Value,
                PagePositionMode = uiRadioButton1.Checked ? PageMode.LeftRightLeft
                    : uiRadioButton2.Checked ? PageMode.LeftRightRight
                    : PageMode.BottomRight,
                Bold = jiacu.Checked,
                ScaleMargin = string.IsNullOrEmpty(uiTextBox1.Text) ? 0 : double.Parse(uiTextBox1.Text),
                NoScaleForSkipped = uiCheckBox1.Checked
            };
        }

        /// <summary>
        /// 获取 imgsrc 目录下支持的图片文件列表（按文件名排序）
        /// </summary>
        private List<string> GetImageFiles()
        {
            if (!Directory.Exists(_imgSrcDir))
                return new List<string>();

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            return Directory.GetFiles(_imgSrcDir)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, new NaturalStringComparer())
                .ToList();
        }

        /// <summary>
        /// 生成预览图
        /// </summary>
        private void UiButton1_Click(object sender, EventArgs e)
        {
            var files = GetImageFiles();
            if (files.Count == 0)
            {
                UIMessageBox.ShowError("未找到图片文件，请在程序目录下的 imgsrc 文件夹中放入 jpg/png 图片。");
                return;
            }

            var config = GetCurrentConfig();

            try
            {
                using (var srcImage = LoadImage(files[0]))
                {
                    int pageNumber = config.PageStartNumber;
                    using (var result = AddPageNumber(srcImage, pageNumber, config))
                    {
                        var previewImage = new Bitmap(result);
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox1.Image = previewImage;
                    }
                }
                uiProcessBar1.Value = 0;
                uiProcessBar1.Text = "预览已生成";
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError("生成预览失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 开始批量处理
        /// </summary>
        private void UiButton2_Click(object sender, EventArgs e)
        {
            var files = GetImageFiles();
            if (files.Count == 0)
            {
                UIMessageBox.ShowError("未找到图片文件，请在程序目录下的 imgsrc 文件夹中放入 jpg/png 图片。");
                return;
            }

            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);

            var config = GetCurrentConfig();
            uiProcessBar1.Value = 0;
            uiProcessBar1.Maximum = files.Count;
            uiProcessBar1.Text = "处理中...";

            uiButton1.Enabled = false;
            uiButton2.Enabled = false;

            int successCount = 0;
            int failCount = 0;

            // 先将 FileStartNumber 之前跳过的页面处理输出
            for (int i = 0; i < config.FileStartNumber && i < files.Count; i++)
            {
                try
                {
                    string ext = Path.GetExtension(files[i]).ToLowerInvariant();
                    string outputFileName = i + ext;
                    string outputPath = Path.Combine(_outputDir, outputFileName);

                    if (config.NoScaleForSkipped)
                    {
                        // 勾选：不加页码的图片不缩放，原样复制
                        File.Copy(files[i], outputPath, true);
                    }
                    else
                    {
                        // 未勾选：缩放但不加页码
                        using (var srcImage = LoadImage(files[i]))
                        {
                            using (var result = ScaleImage(srcImage, config))
                            {
                                SaveImage(result, outputPath, ext);
                            }
                        }
                    }
                    successCount++;
                }
                catch (Exception)
                {
                    failCount++;
                }

                uiProcessBar1.Value = i + 1;
                uiProcessBar1.Text = string.Format("处理跳过页 {0}/{1}", i + 1, files.Count);
                Application.DoEvents();
            }

            // 从 FileStartNumber 开始加页码输出
            for (int i = config.FileStartNumber; i < files.Count; i++)
            {
                try
                {
                    using (var srcImage = LoadImage(files[i]))
                    {
                        int fileIndex = i;
                        int pageNumber = config.PageStartNumber + (i - config.FileStartNumber);

                        using (var result = AddPageNumber(srcImage, pageNumber, config))
                        {
                            string ext = Path.GetExtension(files[i]).ToLowerInvariant();
                            string outputFileName = fileIndex + ext;
                            string outputPath = Path.Combine(_outputDir, outputFileName);

                            SaveImage(result, outputPath, ext);
                            successCount++;
                        }
                    }
                }
                catch (Exception)
                {
                    failCount++;
                }

                uiProcessBar1.Value = i + 1;
                uiProcessBar1.Text = string.Format("处理中 {0}/{1}", i + 1, files.Count);
                Application.DoEvents();
            }

            uiButton1.Enabled = true;
            uiButton2.Enabled = true;
            uiProcessBar1.Text = string.Format("完成！成功 {0} 张，失败 {1} 张", successCount, failCount);

            UIMessageBox.ShowSuccess(string.Format("批量处理完成！\n成功：{0} 张\n失败：{1} 张\n输出目录：{2}", successCount, failCount, _outputDir));
        }

        /// <summary>
        /// 加载图片文件，使用 Stream 方式避免文件锁
        /// </summary>
        private Image LoadImage(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        /// <summary>
        /// 仅缩放图片并白色填充，不加页码
        /// </summary>
        private Image ScaleImage(Image srcImage, PageMarkConfig config)
        {
            int origWidth = srcImage.Width;
            int origHeight = srcImage.Height;

            int canvasWidth = origWidth;
            int canvasHeight = origHeight;

            float drawX, drawY, drawWidth, drawHeight;

            if (config.ScaleMargin > 0)
            {
                double scale = (100.0 - config.ScaleMargin) / 100.0;
                drawWidth = (float)(origWidth * scale);
                drawHeight = (float)(origHeight * scale);
                drawX = (canvasWidth - drawWidth) / 2f;
                drawY = (canvasHeight - drawHeight) / 2f;
            }
            else
            {
                drawX = 0;
                drawY = 0;
                drawWidth = origWidth;
                drawHeight = origHeight;
            }

            var bmp = new Bitmap(canvasWidth, canvasHeight, PixelFormat.Format32bppArgb);
            bmp.SetResolution(srcImage.HorizontalResolution, srcImage.VerticalResolution);

            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var whiteBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(whiteBrush, 0, 0, canvasWidth, canvasHeight);
                }

                g.DrawImage(srcImage, drawX, drawY, drawWidth, drawHeight);
            }

            return bmp;
        }

        /// <summary>
        /// 在图片上添加页码文字，保持原始分辨率和画质
        /// </summary>
        private Image AddPageNumber(Image srcImage, int pageNumber, PageMarkConfig config)
        {
            int origWidth = srcImage.Width;
            int origHeight = srcImage.Height;

            // 计算缩放后的画布大小和图片绘制区域
            int canvasWidth, canvasHeight;
            float drawX, drawY, drawWidth, drawHeight;

            if (config.ScaleMargin > 0)
            {
                // 保持原始画布尺寸，将图片缩小 (100 - ScaleMargin)% 后居中
                canvasWidth = origWidth;
                canvasHeight = origHeight;
                double scale = (100.0 - config.ScaleMargin) / 100.0;
                drawWidth = (float)(origWidth * scale);
                drawHeight = (float)(origHeight * scale);
                drawX = (canvasWidth - drawWidth) / 2f;
                drawY = (canvasHeight - drawHeight) / 2f;
            }
            else
            {
                canvasWidth = origWidth;
                canvasHeight = origHeight;
                drawX = 0;
                drawY = 0;
                drawWidth = origWidth;
                drawHeight = origHeight;
            }

            var bmp = new Bitmap(canvasWidth, canvasHeight, PixelFormat.Format32bppArgb);
            bmp.SetResolution(srcImage.HorizontalResolution, srcImage.VerticalResolution);

            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // 白色背景
                using (var whiteBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(whiteBrush, 0, 0, canvasWidth, canvasHeight);
                }

                // 绘制缩放后的原始图片
                g.DrawImage(srcImage, drawX, drawY, drawWidth, drawHeight);

                // 切换到文字绘制模式
                g.CompositingMode = CompositingMode.SourceOver;

                // 计算页码文字位置
                float fontSize = config.FontSize;
                var fontStyle = config.Bold ? FontStyle.Bold : FontStyle.Regular;
                var font = new Font(config.FontName, fontSize, fontStyle, GraphicsUnit.Pixel);
                string pageText = pageNumber.ToString();

                var textSize = g.MeasureString(pageText, font);
                float textWidth = textSize.Width;
                float textHeight = textSize.Height;

                // 计算绘制位置
                float x, y;
                if (config.PagePositionMode == PageMode.LeftRightLeft)
                {
                    // 左右分页(左开始)：奇数页在左下角，偶数页在右下角
                    if (pageNumber % 2 == 1)
                        x = config.MarginX;
                    else
                        x = canvasWidth - textWidth - config.MarginX;
                    y = canvasHeight - textHeight - config.MarginY;
                }
                else if (config.PagePositionMode == PageMode.LeftRightRight)
                {
                    // 左右分页(右开始)：奇数页在右下角，偶数页在左下角
                    if (pageNumber % 2 == 1)
                        x = canvasWidth - textWidth - config.MarginX;
                    else
                        x = config.MarginX;
                    y = canvasHeight - textHeight - config.MarginY;
                }
                else
                {
                    // 居右下角
                    x = canvasWidth - textWidth - config.MarginX;
                    y = canvasHeight - textHeight - config.MarginY;
                }

                // 绘制描边：用多次偏移绘制替代 GraphicsPath 描边，避免闭合内圈异常
                if (config.StrokeSize > 0)
                {
                    using (var strokeBrush = new SolidBrush(config.StrokeColor))
                    {
                        float step = 1f;
                        float maxOffset = config.StrokeSize;
                        for (float dx = -maxOffset; dx <= maxOffset; dx += step)
                        {
                            for (float dy = -maxOffset; dy <= maxOffset; dy += step)
                            {
                                if (dx * dx + dy * dy <= maxOffset * maxOffset)
                                {
                                    g.DrawString(pageText, font, strokeBrush, x + dx, y + dy);
                                }
                            }
                        }
                    }
                }

                // 绘制文字填充
                using (var brush = new SolidBrush(config.FontColor))
                {
                    g.DrawString(pageText, font, brush, x, y);
                }

                font.Dispose();
            }

            return bmp;
        }

        /// <summary>
        /// 保存图片，根据格式选择最佳编码参数以保持高清
        /// </summary>
        private void SaveImage(Image image, string outputPath, string ext)
        {
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

            if (ext == ".png")
            {
                image.Save(outputPath, ImageFormat.Png);
            }
            else
            {
                var jpegCodec = ImageCodecInfo.GetImageDecoders()
                    .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                if (jpegCodec != null)
                    image.Save(outputPath, jpegCodec, encoderParams);
                else
                    image.Save(outputPath, ImageFormat.Jpeg);
            }
        }

        /// <summary>
        /// 自然排序比较器，确保文件按数字顺序排列（如 1.jpg, 2.jpg, 10.jpg）
        /// </summary>
        private class NaturalStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                string nameX = Path.GetFileNameWithoutExtension(x);
                string nameY = Path.GetFileNameWithoutExtension(y);

                int numX, numY;
                bool isNumX = int.TryParse(nameX, out numX);
                bool isNumY = int.TryParse(nameY, out numY);

                if (isNumX && isNumY)
                    return numX.CompareTo(numY);

                if (isNumX)
                    return -1;
                if (isNumY)
                    return 1;

                return string.Compare(x, y, StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }

    public enum PageMode
    {
        LeftRightLeft = 0,
        LeftRightRight = 1,
        BottomRight = 2
    }

    /// <summary>
    /// 页码标注配置
    /// </summary>
    public class PageMarkConfig
    {
        public string FontName { get; set; }
        public float FontSize { get; set; }
        public float StrokeSize { get; set; }
        public int MarginX { get; set; }
        public int MarginY { get; set; }
        public int PageStartNumber { get; set; }
        public int FileStartNumber { get; set; }
        public Color FontColor { get; set; }
        public Color StrokeColor { get; set; }
        public PageMode PagePositionMode { get; set; }
        public bool Bold { get; set; }
        public double ScaleMargin { get; set; }
        public bool NoScaleForSkipped { get; set; }
    }
}
