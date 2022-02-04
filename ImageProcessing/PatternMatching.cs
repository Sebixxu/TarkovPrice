using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ImageProcessing
{
    public static class PatternMatching
    {
        public static async Task<IList<PatternMatchingResult>> FindTemplateInImageAsync(Bitmap image, IEnumerable<ValueTuple<Bitmap, string, Size>> templates)
        {
            var patternMatchingResult = await Task.Run(() => FindTemplateInImageForMany(image, templates));
            return patternMatchingResult;
        }

        private static IList<PatternMatchingResult> FindTemplateInImageForMany(Bitmap image, IEnumerable<ValueTuple<Bitmap, string, Size>> templates)
        {
            IList<PatternMatchingResult> patternMatchingResults = new List<PatternMatchingResult>();
            foreach (var template in templates)
            {
                var patternMatchingResult = FindTemplateInImage(image, template);
                patternMatchingResults.Add(patternMatchingResult);
            }

            return patternMatchingResults;
        }

        public static PatternMatchingResult FindTemplateInImage(Bitmap image, ValueTuple<Bitmap, string, Size> template)
        {
            bool isFound = false;

            var imageForCv = image.ToImage<Bgr, byte>();
            var templateForCv = template.Item1.ToImage<Bgr, byte>();

            Mat outImage = new Mat();
            CvInvoke.MatchTemplate(imageForCv, templateForCv, outImage, TemplateMatchingType.Sqdiff);

            double minValue = .0;
            double maxValue = .0;
            Point minLocation = new Point();
            Point maxLocation = new Point();

            CvInvoke.MinMaxLoc(outImage, ref minValue, ref maxValue, ref minLocation, ref maxLocation);

            double matchingValue = minValue / outImage.ToImage<Gray, byte>().GetSum().Intensity;

            if ((1 - matchingValue) >= 0.965f)
            {
                isFound = true;
            }

            imageForCv.Dispose();
            templateForCv.Dispose();

            //image.Dispose();
            //template.Dispose(); //To raczej nie

            return new PatternMatchingResult
            {
                Name = template.Item2,
                Size = template.Item3,
                IsFound = isFound,
                MinValue = minValue,
                MaxValue = maxValue,
                MinLocation = minLocation,
                MaxLocation = maxLocation,
                MatchingValue = matchingValue
            };
        }

        public static Bitmap DrawRectangle(Image image, Size size, Color color, Point minLocation)
        {
            var imageForCv = new Bitmap(image).ToImage<Bgr, byte>();

            var rectangle = new Rectangle(minLocation, size); // maxLocation dla innych typów Matching Type
            CvInvoke.Rectangle(imageForCv, rectangle, new MCvScalar(color.B, color.G, color.R), 2);

            return imageForCv.AsBitmap();
        }
    }

    public class PatternMatchingResult
    {
        public string Name { get; set; }
        public Size Size { get; set; } //chyba wolałbym wymiary w pliku jak sie da xml
        public bool IsFound { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public Point MinLocation { get; set; }
        public Point MaxLocation { get; set; }
        public double MatchingValue { get; set; }
    }
}
