using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ImageProcessing;
using ImageProcessing.Results;
using TarkovMarket;
using TarkovPrice.DataModels;
using TarkovPrice.Enums;
using TarkovPrice.Mappers;
using TarkovPrice.Views;
using TarkovPrice.XmlModels;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace TarkovPrice.ViewModels
{
    public class ShellViewModel : Screen
    {
        public bool AnalyzeAsyncEnabled
        {
            get => !IsAnalysisInProgress && !IsGettingTarkovMarketDataInProgress;
            set
            {
                _analyzeAsyncEnabled = value;
                NotifyOfPropertyChange(() => AnalyzeAsyncEnabled);
            }
        }

        public string DataStatus
        {
            get => _dataStatus;
            set
            {
                _dataStatus = value;
                NotifyOfPropertyChange(() => DataStatus);
            }
        }

        public ImageSource OutputImage
        {
            get => _outputImage;
            set
            {
                _outputImage = value;
                NotifyOfPropertyChange(() => OutputImage);
            }
        }

        public bool IsAnalyzeButtonEnable => !IsAnalysisInProgress && !IsGettingTarkovMarketDataInProgress;

        private ITarkovApi TarkovApi;
        private FileAccess FileAccess => new FileAccess();
        private ScreenCapture.ScreenCapture _screenCapture => new ScreenCapture.ScreenCapture();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<TarkovMarketItemData> _tarkovMarketItemDatas = new List<TarkovMarketItemData>();

        private ImageSource _outputImage;
        private string _dataStatus;
        private bool _analyzeAsyncEnabled;

        private ItemsData _itemsData;

        public bool IsAnalysisInProgress
        {
            get => _isAnalysisInProgress;
            private set
            {
                _isAnalysisInProgress = value;
                NotifyOfPropertyChange(() => AnalyzeAsyncEnabled);
                NotifyOfPropertyChange(() => IsAnalysisInProgress);
            }
        }

        public bool IsGettingTarkovMarketDataInProgress
        {
            get => _isGettingTarkovMarketDataInProgress;
            private set
            {
                _isGettingTarkovMarketDataInProgress = value;
                NotifyOfPropertyChange(() => AnalyzeAsyncEnabled);
                NotifyOfPropertyChange(() => IsGettingTarkovMarketDataInProgress);
            }
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            OutputImage = BitmapToImageSource(new Bitmap($"output\\template_image.png"));
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            ManageApplicationSettings();

            TarkovApi = new TarkovMarketApi(Configuration.GetStringValue(Configuration.ApiKey));

            //GetTarkovMarketItemData();
            RefreshingTarkovMarketDataAsync();

            var items = FileAccess.ReadXmlItemsData();
            _itemsData = items.XmlItemsToItemsData();
            LoadTemplateBitmaps(_itemsData);

            return base.OnInitializeAsync(cancellationToken);
        }



        private async void RefreshingTarkovMarketDataAsync()
        {
            log.Debug($"Starting Tarkov Market Data Refreshing task");

            DataStatus = "The data is NOT up-to-date";

            await Task.Run(() =>
            {
                while (true) // ? xD
                {
                    log.Debug($"Refreshing market data");

                    //AnalyzeAsyncEnabled = false;

                    IsGettingTarkovMarketDataInProgress = true;

                    DataStatus = "Refreshing...";
                    //Jeśli teraz trwa analiza to poczekaj 30s i sprobuj znowu?
                    var getAllItemsResult = TarkovApi.GetAllItems().Result;

                    if (getAllItemsResult.Status == Status.Ok)
                    {
                        log.Debug($"Refreshing market data - successful");


                        _tarkovMarketItemDatas = getAllItemsResult.TarkovMarketItemDatas.ToList();
                        //debug info
                        //jakies info na widoku

                        DataStatus = "The data is up-to-date"; //TODO do resx

                        IsGettingTarkovMarketDataInProgress = false;


                        // AnalyzeAsyncEnabled = true;

                        Thread.Sleep(TimeSpan.FromMinutes(15));
                    }
                    else
                    {
                        log.Debug($"Refreshing market data - failed. Message: {getAllItemsResult.Message}");

                        //debug info
                        //info na widoku

                        DataStatus = "The data is NOT up-to-date";

                        Thread.Sleep(TimeSpan.FromMinutes(1));
                    }
                }
            });
        }

        public void ManageApplicationSettings()
        {
            if (FileAccess.IsConfigurationFileCreated())
            {
                var readXmlConfigurationsData = FileAccess.ReadXmlConfigurationsData();
                Configuration.LoadConfiguration(readXmlConfigurationsData);
            }
            else
            {
                Configurations configurations = new Configurations();
                var defaultConfigurationDictionary = Configuration.DefaultConfigurationDictionary;
                foreach (var configuration in defaultConfigurationDictionary)
                {
                    configurations.ConfigurationCollection.Add(new XmlModels.Configuration(configuration.Key, configuration.Value));
                }

                FileAccess.SaveConfigurationFile(configurations);
            }


            //if not exist save config file -> nothing more?

            //load if exist -> load
        }

        public async void AnalyzeAsync()
        {
            if (!_tarkovMarketItemDatas.Any() || IsGettingTarkovMarketDataInProgress) //TODO Lepsza obsługa braku danych z API
                return;

            IsAnalysisInProgress = true;

            string dateTimeNowString = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);//"2101_5.png";

            var screenShot = GetScreenShotImageByCurrentApplicationMode();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            log.Debug($"Starting: {dateTimeNowString}");

            SaveBitmapToFile(new Bitmap(screenShot), "input", dateTimeNowString);
            var imageBitmap = ReduceImageArea(screenShot, Configuration.GetIntValue(Configuration.CutPointX), Configuration.GetIntValue(Configuration.CutPointY),
                Configuration.GetIntValue(Configuration.CutWidth), Configuration.GetIntValue(Configuration.CutHeight));

            Bitmap outputBitmap = imageBitmap;

            var chunkedByItemsData = SplitItemsDataByMaxTaskCount();

            var patternMatchingResults = new List<PatternMatchingResult>();
            var enumerableOfTemplateMatchingTasks = GetEnumerableOfTemplateMatchingTasks(chunkedByItemsData, imageBitmap);
            var tasks = enumerableOfTemplateMatchingTasks.ToList();

            await AwaitTemplateMatchingTasksEnds(tasks, patternMatchingResults);

            var patternMatchingResultsData = ProcessPatternMatchingResultDatas(patternMatchingResults);
            outputBitmap = DrawRectanglesByFoundPatterns(patternMatchingResultsData, outputBitmap);

            SaveBitmapToFile(outputBitmap, "output", dateTimeNowString);
            OutputImage = BitmapToImageSource(outputBitmap);

            Debug.WriteLine("Memory used before collection:       {0:N0}",
                GC.GetTotalMemory(false));

            // Collect all generations of memory.
            GC.Collect();
            Debug.WriteLine("Memory used after full collection:   {0:N0}",
                GC.GetTotalMemory(true));

            watch.Stop();

            log.Debug($"Total time: {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}");

            IsAnalysisInProgress = false;
        }

        private System.Drawing.Image GetScreenShotImageByCurrentApplicationMode()
        {
            bool debugMode = Configuration.GetBoolValue("IsDebugOn");
            System.Drawing.Image screenShot; //TODO Specify window
            if (!debugMode)
            {
                screenShot = _screenCapture.CaptureScreen();
            }
            else
            {
                screenShot = System.Drawing.Image.FromFile(Configuration.GetStringValue("FileName"));
            }

            return screenShot;
        }

        private static void SaveBitmapToFile(Bitmap outputBitmap, string path, string fileName)
        {
            outputBitmap?.Save($"{path}\\{fileName}.png");
        }

        IWindowManager WindowManager = new WindowManager();
        private bool _isAnalysisInProgress = false;
        private bool _isGettingTarkovMarketDataInProgress = false;

        public void OpenConfigurationWindow()
        {
            WindowManager.ShowDialogAsync(new ConfigurationViewModel());
        }

        private static Bitmap DrawRectanglesByFoundPatterns(List<PatternMatchingResultData> patternMatchingResultsData, Bitmap outputBitmap)
        {
            foreach (var patternMatchingResultData in patternMatchingResultsData)
            {
                PatternMatchingResult patternMatchingResult = patternMatchingResultData.PatternMatchingResult;
                var newSize = new Size(patternMatchingResult.Size.Width - 2, patternMatchingResult.Size.Height - 2); //TODO Config params
                outputBitmap = PatternMatching.DrawRectangle(outputBitmap, newSize,
                    patternMatchingResultData.RectangleColor, patternMatchingResult.MinLocation);
            }

            return outputBitmap;
        }

        private List<PatternMatchingResultData> ProcessPatternMatchingResultDatas(List<PatternMatchingResult> patternMatchingResults)
        {
            List<PatternMatchingResultData> patternMatchingResultsData = new List<PatternMatchingResultData>();
            foreach (var patternMatchingResult in patternMatchingResults)
            {
                if (!patternMatchingResult.IsFound)
                {
                    continue;
                }

                var currentItemTarkovMarketData =
                    _tarkovMarketItemDatas.FirstOrDefault(x => x.Name == patternMatchingResult.Name); //To change imho
                var itemPriceLevel = GetItemPriceLevel(currentItemTarkovMarketData);
                var colorByPriceLevel = GetColorByPriceLevel(itemPriceLevel);

                patternMatchingResultsData.Add(
                    new PatternMatchingResultData
                    {
                        PatternMatchingResult = patternMatchingResult,
                        RectangleColor = colorByPriceLevel,
                        ImageSize = patternMatchingResult.Size
                    });

                log.Debug($"Found object: {patternMatchingResult.Name} | Cords: {patternMatchingResult.MinLocation} " +
                          $"| Size: {patternMatchingResult.Size} | Price per slot: {currentItemTarkovMarketData.Price / currentItemTarkovMarketData.Slots} " +
                          $"| Color: {colorByPriceLevel.Name} | Matching Value: {patternMatchingResult.MatchingValue} | Level: {itemPriceLevel}");
            }

            return patternMatchingResultsData;
        }

        private static async Task AwaitTemplateMatchingTasksEnds(List<Task<IList<PatternMatchingResult>>> tasks, List<PatternMatchingResult> patternMatchingResults)
        {
            while (tasks.Any())
            {
                Task<IList<PatternMatchingResult>> finishedTask = await Task.WhenAny(tasks);
                tasks.Remove(finishedTask);

                patternMatchingResults.AddRange(finishedTask.Result);
            }
        }

        private static IEnumerable<Task<IList<PatternMatchingResult>>> GetEnumerableOfTemplateMatchingTasks(List<List<ItemData>> chunkedByItemsData, Bitmap imageBitmap)
        {
            IEnumerable<Task<IList<PatternMatchingResult>>> enumerableOfTemplateMatchingTasks =
                from data in chunkedByItemsData
                select PatternMatching.FindTemplateInImageAsync(new Bitmap(imageBitmap),
                    data.Select(x => (x.Image, x.ItemName, x.Image.Size)));
            return enumerableOfTemplateMatchingTasks;
        }

        private Bitmap ReduceImageArea(System.Drawing.Image screenShot, int leftCornerX, int leftCornerY, int width, int height)
        {
            Bitmap imageBitmap = new Bitmap(screenShot); //new Bitmap($"screenshots\\{input}");
            imageBitmap = CropBitmap(imageBitmap, new Rectangle(new Point(leftCornerX, leftCornerY), new Size(width, height)));
            return imageBitmap;
        }

        private List<List<ItemData>> SplitItemsDataByMaxTaskCount()
        {
            var chunkCount = Math.Ceiling(_itemsData.ItemsCollection.Count / MaxTasksCount);
            var chunkedByItemsData = _itemsData.ItemsCollection.ChunkBy((int)chunkCount);
            return chunkedByItemsData;
        }

        private static double MaxTasksCount => 16.0;

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public Bitmap CropBitmap(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }

        public void LoadTemplateBitmaps(ItemsData itemsData)
        {
            foreach (var item in itemsData.ItemsCollection)
            {
                item.Image = new Bitmap($"_templates\\{item.ImageName}");
            }
        }

        public PriceLevel GetItemPriceLevel(TarkovMarketItemData tarkovMarketItemData) //per slot
        {
            var pricePerSlot = tarkovMarketItemData.Price / tarkovMarketItemData.Slots;
            if (pricePerSlot < 5000)
            {
                return PriceLevel.Level1;
            }

            if (pricePerSlot >= 5000 && pricePerSlot < 7500)
            {
                return PriceLevel.Level2;
            }

            if (pricePerSlot >= 7500 && pricePerSlot < 10000)
            {
                return PriceLevel.Level3;
            }

            if (pricePerSlot >= 10000 && pricePerSlot < 15000)
            {
                return PriceLevel.Level4;
            }

            if (pricePerSlot >= 15000 && pricePerSlot < 20000)
            {
                return PriceLevel.Level5;
            }

            if (pricePerSlot >= 20000 && pricePerSlot < 30000)
            {
                return PriceLevel.Level6;
            }

            return PriceLevel.Level7;
        }

        public Color GetColorByPriceLevel(PriceLevel priceLevel) //TODO Custom colors
        {
            switch (priceLevel)
            {
                case PriceLevel.Level1:
                    return Color.Black;
                case PriceLevel.Level2:
                    return Color.Gray;
                case PriceLevel.Level3:
                    return Color.Green;
                case PriceLevel.Level4:
                    return Color.Blue;
                case PriceLevel.Level5:
                    return Color.DeepPink;
                case PriceLevel.Level6:
                    return Color.Orange;
                case PriceLevel.Level7:
                    return Color.Red;
                default:
                    throw new ArgumentOutOfRangeException(nameof(priceLevel), priceLevel, null);
            }
        }
    }
}

/* DEAD CODE:

         public void Analize()
        {
            var patternMatchingResults = new List<PatternMatchingResult>();
            var watch = System.Diagnostics.Stopwatch.StartNew();

            log.Debug($"{Guid.NewGuid()}");

            //ScreenCapture.ScreenCapture sc = new ScreenCapture.ScreenCapture();
            //// capture entire screen, and save it to a file
            //Image img = sc.CaptureScreen();
            //// display image in a Picture control named imageDisplay
            //// capture this window, and save it

            //img.Save("test.jpg");

            string input = "2101_5.png";
            Bitmap imageBitmap = new Bitmap($"screenshots\\{input}");

            imageBitmap = CropBitmap(imageBitmap, new Rectangle(new Point(650, 70), new Size(1000, 850)));


            List<PatternMatchingResultData> patternMatchingResultsData = new List<PatternMatchingResultData>();
            Bitmap output = imageBitmap;
            foreach (var item in _itemsData.ItemsCollection)
            {
                var sizeToMark = item.Image.Size;
                var patternMatchingResult = PatternMatching.FindTemplateInImage(imageBitmap, (item.Image, item.ImageName, imageBitmap.Size));
                patternMatchingResults.Add(patternMatchingResult);

                if (patternMatchingResult.IsFound)
                {
                    var currentItemTarkovMarketData = GetTarkovMarketItemData().FirstOrDefault(x => x.Name == item.ItemName); //To change imho
                    var itemPriceLevel = GetItemPriceLevel(currentItemTarkovMarketData);
                    var colorByPriceLevel = GetColorByPriceLevel(itemPriceLevel);

                    patternMatchingResultsData.Add(
                        new PatternMatchingResultData
                        {
                            PatternMatchingResult = patternMatchingResult,
                            RectangleColor = colorByPriceLevel,
                            ImageSize = sizeToMark
                        });

                    log.Debug($"Found object: {item.ItemName} | Cords: {patternMatchingResult.MinLocation} " +
                              $"| Size: {item.Image.Size} | Price per slot: {currentItemTarkovMarketData.Price / currentItemTarkovMarketData.Slots} " +
                              $"| Color: {colorByPriceLevel.Name} | Matching Value: {patternMatchingResult.MatchingValue} | Level: {itemPriceLevel}");
                }

                //item.Image.Dispose(); // ? Raczej out
            }

            log.Debug($"Sum: {patternMatchingResultsData.Count} object found.");

            Debug.WriteLine("Memory used before collection:       {0:N0}",
                GC.GetTotalMemory(false));

            // Collect all generations of memory.
            GC.Collect();
            Debug.WriteLine("Memory used after full collection:   {0:N0}",
                GC.GetTotalMemory(true));

            foreach (var patternMatchingResultData in patternMatchingResultsData)
            {
                output = PatternMatching.DrawRectangle(output, patternMatchingResultData.PatternMatchingResult,
                    patternMatchingResultData.ImageSize, patternMatchingResultData.RectangleColor);
            }

            output?.Save($"output\\{input}"); // Raczej out, albo tylko debug
            OutputImage = BitmapToImageSource(output);

            Debug.WriteLine("Memory used before collection:       {0:N0}",
                GC.GetTotalMemory(false));

            // Collect all generations of memory.
            GC.Collect();
            Debug.WriteLine("Memory used after full collection:   {0:N0}",
                GC.GetTotalMemory(true));

            watch.Stop();

            log.Debug($"Total time: {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}");
        }
 */