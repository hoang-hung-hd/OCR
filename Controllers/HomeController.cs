using IronOcr;
using Microsoft.AspNetCore.Mvc;
using OCR.Models;
using Spire.OCR;
using System.Diagnostics;
using System.Drawing;
using Tesseract;

namespace OCR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            string imagePath = "D:\\videoSub\\china2.png";
            //string text1 = ConvertToText_IronOcr(imagePath, "chi_sim");
            //string text2 = ConvertToText_Tesseract(imagePath, "chi_sim");
            //string scannedText = ScanTextFromImage(imagePath);

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> getVideoContent(OCRModel orcModel)
        {
            string originalPath = "";
            string destinationPath = "";
            //bool result = IronOcr.License.IsValidLicense("IRONSUITE.HUNGHV.BIZSYS.VN.5400-5392C5AA3F-EAJJOIFOGN35PU-3L5DWP5YUJTC-OJ4JTECUO3XQ-IZC657H6LISI-2DI3PJ6FWSV3-B57AYDUWI2XR-XCFZTP-TIVM3P7VJNCNEA-DEPLOYMENT.TRIAL-BDXZGH.TRIAL.EXPIRES.17.AUG.2024");
            try
            {
                if (orcModel.videoFile != null && orcModel.videoFile.Length > 0)
                {
                    var fileName = Path.GetFileName(orcModel.videoFile.FileName);
                    originalPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\videos", fileName);
                    using (var fileStream = new FileStream(originalPath, FileMode.Create))
                    {
                        await orcModel.videoFile.CopyToAsync(fileStream);
                    }

                    destinationPath = "D:\\videoSub\\out%d.png";
                    string strCmdText = "";
                    Process cmd = new Process();

                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.UseShellExecute = false;

                    cmd.Start();
                    using (StreamWriter sw = cmd.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            strCmdText = $"ffmpeg -i {originalPath} -vf \"fps=10\" {destinationPath}";
                            sw.WriteLine(strCmdText);
                        }
                    }
                    DirectoryInfo d = new DirectoryInfo(@"D:\videoSub");
                    FileInfo[] listImage = d.GetFiles("*.png");

                    for (var i = 0; i < listImage.ToList().Count; i++)
                    {
                        string newImage = CropImageBeforeConvert(listImage[i].Directory.ToString(), listImage[i].Name);
                        string text1 = ConvertToText_IronOcr(newImage, "eng");
                        string text2 = ConvertToText_Tesseract(newImage, "eng");
                        System.IO.File.Delete(listImage[i].Directory + "\\" + listImage[i].Name);

                    }
                    //string imagePath = listImage[0].Path

                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string ConvertToText_Tesseract(string imagePath, string lang)
        {
            string result = "";
            string initPath = Path.Combine(Directory.GetCurrentDirectory(), @"tessdata");
            //var ocrengine = new TesseractEngine(@"D:\videoSub\tessdata", "eng", Tesseract.EngineMode.Default);
            var ocrengine = new TesseractEngine(initPath, lang, Tesseract.EngineMode.Default);
            var img = Pix.LoadFromFile(imagePath);
            var res = ocrengine.Process(img);
            result = res.GetText();
            return result;
        }

        public string ConvertToText_IronOcr(string imagePath, string lang)
        {
            IronTesseract IronOcr = new IronTesseract();
            switch (lang)
            {
                case "vn":
                    IronOcr.Language = OcrLanguage.Vietnamese;
                    break;
                case "jpn":
                    IronOcr.Language = OcrLanguage.Japanese;
                    break;
                case "chi_sim":
                    IronOcr.Language = OcrLanguage.ChineseSimplified;
                    break;
                default:
                    IronOcr.Language = OcrLanguage.English;
                    break;
            }
            var Result = IronOcr.Read(imagePath);
            return Result.Text;
        }

        public string CropImageBeforeConvert(string imageDirectory, string imageName)
        {
            string newImageName = "";
            string newImagePath = "";
            string imagePath = imageDirectory + "\\" + imageName;
            try
            {
                if (!string.IsNullOrEmpty(imagePath))
                {
                    Image image = Image.FromFile(imagePath);

                    // Create a bitmap to draw on
                    int width = image.Width;
                    int height = image.Height / 3;  // Height of the bottom third
                    Bitmap bitmap = new Bitmap(width, height);

                    // Create graphics object from the bitmap
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Clear the background to white
                        g.Clear(Color.White);

                        // Define the source rectangle to take the bottom third of the image
                        Rectangle srcRect = new Rectangle(0, image.Height - height, width, height);

                        // Define the destination rectangle to draw the image on the bitmap
                        Rectangle destRect = new Rectangle(0, 0, width, height);

                        // Draw the image at the calculated position
                        g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
                    }

                    // Save the result to a file
                    newImageName = imageName.Replace(".png", "Crop.png");
                    newImagePath = imageDirectory + "\\" + newImageName;
                    bitmap.Save(newImagePath);

                }
                return newImagePath;
            }
            catch (Exception ex)
            {
                return newImagePath;
            }
        }


        public string ScanTextFromImage(string imageFilePath)
        {
            //Instantiate an OcrScanner object
            using (OcrScanner ocrScanner = new OcrScanner())
            {
                //Scan text from the image
                ocrScanner.Scan(imageFilePath);
                //Get the recognized text from the OcrScanner object
                IOCRText text = ocrScanner.Text;
                //Return the text
                return text.ToString();
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
