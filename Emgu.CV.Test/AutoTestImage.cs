//----------------------------------------------------------------------------
//  Copyright (C) 2004-2012 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.Util;
using NUnit.Framework;

namespace Emgu.CV.Test
{
   [TestFixture]
   public class AutoTestImage
   {
      [Test]
      public void TestRunningAvg()
      {
         Image<Gray, Single> img1 = new Image<Gray, float>(100, 40, new Gray(100));
         Image<Gray, Single> img2 = new Image<Gray, float>(100, 40, new Gray(50));
         IImage img = img2;
         img1.RunningAvg(img2, 0.5);
      }

      [Test]
      public void TestSetValue()
      {
         Image<Bgr, Single> img1 = new Image<Bgr, float>(50, 20, new Bgr(8.0, 1.0, 2.0));
         for (int i = 0; i < img1.Width; i++)
            for (int j = 0; j < img1.Height; j++)
            {
               Bgr c = img1[j, i];
               EmguAssert.IsTrue(c.Equals(new Bgr(8.0, 1.0, 2.0)));
            }
      }

      [Test]
      public void TestMinMax()
      {
         Image<Gray, Byte> img1 = new Image<Gray, Byte>(50, 60);
         System.Random r = new Random();

         using (Image<Gray, Byte> img2 = img1.Convert<Byte>(delegate(Byte f) { return (Byte)r.Next(255); }))
         {
            img2._Max(120.0);
            for (int i = 0; i < img2.Width; i++)
               for (int j = 0; j < img2.Height; j++)
                  EmguAssert.IsTrue(img2[j, i].Intensity >= 120.0);
         }

         using (Image<Gray, Byte> img2 = img1.Convert<Byte>(delegate(Byte f) { return (Byte)r.Next(255); }))
         {
            img2._Min(120.0);
            for (int i = 0; i < img2.Width; i++)
               for (int j = 0; j < img2.Height; j++)
                  EmguAssert.IsTrue(120.0 >= img2[j, i].Intensity);
         }

         using (Image<Gray, Byte> img2 = img1.Convert<Byte>(delegate(Byte f) { return (Byte)r.Next(255); }))
         using (Image<Gray, Byte> img3 = img1.Convert<Byte>(delegate(Byte f) { return (Byte)r.Next(255); }))
         using (Image<Gray, Byte> img4 = img2.Max(img3))
         {
            for (int i = 0; i < img2.Width; i++)
               for (int j = 0; j < img2.Height; j++)
               {
                  Point location = new Point(i, j);
                  EmguAssert.IsTrue(img4[location].Intensity >= img2[location].Intensity);
                  EmguAssert.IsTrue(img4[j, i].Intensity >= img3[j, i].Intensity);
               }
         }

         /*
         using (Image<Gray, Byte> img2 = img1.Convert<Byte>(delegate(Byte f) { return (Byte)r.Next(255); }))
         using (Image<Gray, Byte> img3 = img1.Convert<Byte>(delegate(Byte f) { return (Byte)r.Next(255); }))
         using (Image<Gray, Byte> img4 = img2.Min(img3))
         {
             for (int i = 0; i < img2.Width; i++)
                 for (int j = 0; j < img2.Height; j++)
                 {
                     Assert.GreaterOrEqual(img2.GetPixel(new Point2D<int>(i, j)).Intensity, img4.GetPixel(new Point2D<int>(i, j)).Intensity);
                     Assert.GreaterOrEqual(img3.GetPixel(new Point2D<int>(i, j)).Intensity, img4.GetPixel(new Point2D<int>(i, j)).Intensity);
                 }
         }*/
      }

      [Test]
      public void TestAvgSdv()
      {
         Image<Gray, Single> img1 = new Image<Gray, float>(50, 20);
         img1.SetRandNormal(new MCvScalar(100), new MCvScalar(30));
         Gray mean;
         MCvScalar std;
         img1.AvgSdv(out mean, out std);
      }

      [Test]
      public void TestBgrFloat()
      {
         Image<Bgr, float> img = new Image<Bgr, float>("lena.jpg");
         Size s = img.Size;
      }

      [Test]
      public void TestGenericOperation()
      {
         Image<Gray, Single> img1 = new Image<Gray, float>(50, 20);
         img1.ROI = new Rectangle(10, 1, 50 - 10, 19 - 1);
         img1.SetValue(5.0);

         Image<Gray, Single> img2 = new Image<Gray, float>(50, 20);
         img2.ROI = new Rectangle(0, 2, 40, 20 - 2);
         img2.SetValue(new Gray(2.0));

         EmguAssert.IsTrue(img1.Width == img2.Width);
         EmguAssert.IsTrue(img1.Height == img2.Height);

         Stopwatch watch = Stopwatch.StartNew();
         Image<Gray, Single> img3 = img1.Add(img2);
         long cvAddTime = watch.ElapsedMilliseconds;

         watch.Reset();
         watch.Start();
         Image<Gray, Single> img4 = img1.Convert<Single, Single>(img2, delegate(Single v1, Single v2) {
            return v1 + v2; });
         long genericAddTime = watch.ElapsedMilliseconds;

         Image<Gray, Single> img5 = img3.AbsDiff(img4);

         watch.Reset();
         watch.Start();
         double sum1 = img5.GetSum().Intensity;
         long cvSumTime = watch.ElapsedMilliseconds;

         watch.Reset();
         watch.Start();
         Single sum2 = 0.0f;
         img5.Action(delegate(Single v) {
            sum2 += v; });
         long genericSumTime = watch.ElapsedMilliseconds;

         EmguAssert.WriteLine(String.Format("CV Add     : {0} milliseconds", cvAddTime));
         EmguAssert.WriteLine(String.Format("Generic Add: {0} milliseconds", genericAddTime));
         EmguAssert.WriteLine(String.Format("CV Sum     : {0} milliseconds", cvSumTime));
         EmguAssert.WriteLine(String.Format("Generic Sum: {0} milliseconds", genericSumTime));
         EmguAssert.WriteLine(String.Format("Abs Diff = {0}", sum1));
         EmguAssert.WriteLine(String.Format("Abs Diff = {0}", sum2));
         EmguAssert.IsTrue(sum1 == sum2);

         img3.Dispose();
         img4.Dispose();
         img5.Dispose();

         DateTime t1 = DateTime.Now;
         img3 = img1.Mul(2.0);
         DateTime t2 = DateTime.Now;
         img4 = img1.Convert<Single>(delegate(Single v1) {
            return v1 * 2.0f; });
         DateTime t3 = DateTime.Now;

         /*
         ts1 = t2.Subtract(t1);
         ts2 = t3.Subtract(t2);
         Trace.WriteLine(String.Format("CV Mul     : {0} milliseconds", ts1.TotalMilliseconds));
         Trace.WriteLine(String.Format("Generic Mul: {0} milliseconds", ts2.TotalMilliseconds));
         */

         EmguAssert.IsTrue(img3.Equals(img4));
         img3.Dispose();
         img4.Dispose();

         t1 = DateTime.Now;
         img3 = img1.Add(img1);
         img4 = img3.Add(img1);
         t2 = DateTime.Now;
         img5 = img1.Convert<Single, Single, Single>(img1, img1, delegate(Single v1, Single v2, Single v3) {
            return v1 + v2 + v3; });
         t3 = DateTime.Now;

         /*
         ts1 = t2.Subtract(t1);
         ts2 = t3.Subtract(t2);
         Trace.WriteLine(String.Format("CV Sum (3 images)     : {0} milliseconds", ts1.TotalMilliseconds));
         Trace.WriteLine(String.Format("Generic Sum (3 images): {0} milliseconds", ts2.TotalMilliseconds));
         */
         EmguAssert.IsTrue(img5.Equals(img4));
         img3.Dispose();
         img4.Dispose();
         img5.Dispose();

         img1.Dispose();
         img2.Dispose();

         Image<Gray, Byte> gimg1 = new Image<Gray, Byte>(400, 300, new Gray(30));
         Image<Gray, Byte> gimg2 = gimg1.Convert<Byte>(delegate(Byte b) {
            return (Byte) (255 - b); });
         gimg1.Dispose();
         gimg2.Dispose();
      }

      [Test]
      public void TestConvertDepth()
      {
         Image<Gray, Byte> img1 = new Image<Gray, byte>(100, 100, new Gray(10.0));
         img1.SetRandUniform(new MCvScalar(0, 0, 0), new MCvScalar(255, 255, 255));
         Image<Gray, Single> img2 = img1.ConvertScale<Single>(2.0, 0.0);
         Image<Gray, Byte> img3 = img2.ConvertScale<Byte>(0.5, 0.0);
         EmguAssert.IsTrue(img3.Equals(img1));

         Image<Gray, Double> img4 = img1.Convert<Gray, Double>();
         Image<Gray, Byte> img5 = img4.Convert<Gray, Byte>();
         EmguAssert.IsTrue(img5.Equals(img1));
      }

      [Test]
      public void TestMemory()
      {
         for (int i = 0; i <= 100; i++)
         {
            Image<Bgr, Single> img = new Image<Bgr, Single>(1000, 1000);
         }
      }

      [Test]
      public void TestConversion()
      {
         Image<Bgr, Single> img1 = new Image<Bgr, Single>(100, 100);
         img1.SetRandUniform(new MCvScalar(), new MCvScalar(255.0, 255.0, 255.0));

         Image<Xyz, Single> img2 = img1.Convert<Xyz, Single>();

         Image<Gray, Byte> img3 = img1.Convert<Gray, Byte>();

      }

      [Test]
      public void TestGenericSetColor()
      {
         Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(20, 40, new Bgr());

         int flag = 0;

         Image<Bgr, Byte> img2 = img1.Convert<Byte>(
             delegate(Byte b)
         {
            return ((flag++ % 3) == 0) ? (Byte) 255 : (Byte) 0;
         });

         img1.SetValue(new Bgr(255, 0, 0));

         Image<Bgr, Byte> img = new Image<Bgr, byte>(800, 800);
         img.SetValue(255);
         Image<Bgr, Byte> mask = new Image<Bgr, byte>(img.Width, img.Height);
         mask.SetRandUniform(new MCvScalar(0, 0, 0), new MCvScalar(255, 255, 255)); //file the mask with random color

         Stopwatch watch = Stopwatch.StartNew();
         Image<Bgr, Byte> imgMasked = img.Convert<Byte, Byte>(mask,
            delegate(Byte byteFromImg, Byte byteFromMask)
         {
            return byteFromMask > (Byte) 120 ? byteFromImg : (Byte) 0;
         });
         watch.Stop();
         EmguAssert.WriteLine(String.Format("Time used: {0} milliseconds", watch.ElapsedMilliseconds));

         EmguAssert.IsTrue(img1.Equals(img2));
      }

      [Test]
      public void TestRuntimeSerialize()
      {
         Image<Bgr, Byte> img = new Image<Bgr, byte>(100, 80);

         using (MemoryStream ms = new MemoryStream())
         {
            img.SetRandNormal(new MCvScalar(100, 100, 100), new MCvScalar(50, 50, 50));
            img.SerializationCompressionRatio = 9;
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(ms, img);
            Byte[] bytes = ms.GetBuffer();

            using (MemoryStream ms2 = new MemoryStream(bytes))
            {
               Object o = formatter.Deserialize(ms2);
               Image<Bgr, Byte> img2 = (Image<Bgr, Byte>) o;
               EmguAssert.IsTrue(img.Equals(img2));
            }
         }
      }

      [Test]
      public void TestRuntimeSerializeWithROI()
      {
         Image<Bgr, Byte> img = new Image<Bgr, byte>(100, 80);

         using (MemoryStream ms = new MemoryStream())
         {
            img.SetRandNormal(new MCvScalar(100, 100, 100), new MCvScalar(50, 50, 50));
            img.SerializationCompressionRatio = 9;
            img.ROI = new Rectangle(10, 10, 20, 30);

            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(ms, img);
            Byte[] bytes = ms.GetBuffer();

            using (MemoryStream ms2 = new MemoryStream(bytes))
            {
               Object o = formatter.Deserialize(ms2);
               Image<Bgr, Byte> img2 = (Image<Bgr, Byte>) o;
               EmguAssert.IsTrue(img.Equals(img2));
            }
         }
      }

      [Test]
      public void TestSampleLine()
      {
         Image<Bgr, Byte> img = new Image<Bgr, byte>(101, 133);
         img.SetRandUniform(new MCvScalar(), new MCvScalar(255, 255, 255));

         Byte[,] buffer = img.Sample(new LineSegment2D(new Point(0, 0), new Point(0, 100)));
         for (int i = 0; i < 100; i++)
            EmguAssert.IsTrue(img[i, 0].Equals(new Bgr(buffer[i, 0], buffer[i, 1], buffer[i, 2])));

         buffer = img.Sample(new LineSegment2D(new Point(0, 0), new Point(100, 100)), Emgu.CV.CvEnum.CONNECTIVITY.FOUR_CONNECTED);
      }

      [Test]
      public void TestGetSize()
      {
         Image<Bgr, Byte> img = new Image<Bgr, byte>(10, 10, new Bgr(255, 255, 255));
         Size size = img.Size;
         EmguAssert.AreEqual(size, new Size(10, 10));
      }

      [Test]
      public void TestXmlSerialize()
      {
         Image<Bgr, Byte> img = new Image<Bgr, byte>(100, 80);

         img.SetRandNormal(new MCvScalar(100, 100, 100), new MCvScalar(50, 50, 50));
         img.SerializationCompressionRatio = 9;
         XmlDocument doc1 = Toolbox.XmlSerialize<Image<Bgr, Byte>>(img);
         String str = doc1.OuterXml;
         Image<Bgr, Byte> img2 = Toolbox.XmlDeserialize<Image<Bgr, Byte>>(doc1);
         EmguAssert.IsTrue(img.Equals(img2));

         img.SerializationCompressionRatio = 9;
         XmlDocument doc2 = Toolbox.XmlSerialize<Image<Bgr, Byte>>(img);
         Image<Bgr, Byte> img3 = Toolbox.XmlDeserialize<Image<Bgr, Byte>>(doc2);
         EmguAssert.IsTrue(img.Equals(img3));

         XmlDocument doc3 = new XmlDocument();
         doc3.LoadXml(str);
         Image<Bgr, Byte> img4 = Toolbox.XmlDeserialize<Image<Bgr, Byte>>(doc3);
         EmguAssert.IsTrue(img.Equals(img4));

      }

      [Test]
      public void TestRotation()
      {
         Image<Bgr, Byte> img = new Image<Bgr, byte>(100, 80);

         img.SetRandNormal(new MCvScalar(100, 100, 100), new MCvScalar(50, 50, 50));

         Image<Bgr, Byte> imgRotated = img.Rotate(90, new Bgr(), false);
         EmguAssert.AreEqual(img.Width, imgRotated.Height);
         EmguAssert.AreEqual(img.Height, imgRotated.Width);
         imgRotated = img.Rotate(30, new Bgr(255, 255, 255), false);

      }

      [Test]
      public void TestFaceDetect()
      {
         Image<Gray, Byte> image = new Image<Gray, byte>("lena.jpg");
         //using (HaarCascade cascade = new HaarCascade("eye_12.xml"))
         using (HaarCascade cascade = new HaarCascade("haarcascade_eye.xml"))
         //using (HaarCascade cascade = new HaarCascade("haarcascade_frontalface_alt2.xml"))
         {
            MCvAvgComp[] objects = cascade.Detect(image, 1.05, 0, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(10, 10), Size.Empty);
            foreach (MCvAvgComp obj in objects)
               image.Draw(obj.rect, new Gray(0.0), 1);

            using (MemStorage stor = new MemStorage())
            {
               IntPtr objs = CvInvoke.cvHaarDetectObjects(
                             image.Ptr,
                             cascade.Ptr,
                             stor.Ptr,
                             1.05,
                             0,
                             Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                             new Size(10, 10), 
                             Size.Empty);

               if (objs != IntPtr.Zero)
               {
                  Seq<MCvAvgComp> rects = new Seq<MCvAvgComp>(objs, stor);

                  MCvAvgComp[] rect = rects.ToArray();
                  for (int i = 0; i < rects.Total; i++)
                  {
                     EmguAssert.IsTrue(rect[i].rect.Equals(rects[i].rect));
                  }
               }
            }
         }
      }

#if !IOS
      [Test]
      public void TestConstructor()
      {
         for (int i = 0; i < 20; i++)
         {
            Image<Gray, Byte> img = new Image<Gray, Byte>(500, 500, new Gray());
            EmguAssert.AreEqual(0, System.Convert.ToInt32(img.GetSum().Intensity));
         }

         for (int i = 0; i < 20; i++)
         {
            Image<Bgr, Single> img = new Image<Bgr, Single>(500, 500);
            EmguAssert.IsTrue(img.GetSum().Equals(new Bgr(0.0, 0.0, 0.0)));
         }

         Image<Bgr, Byte> img2 = new Image<Bgr, byte>(1, 2);
         EmguAssert.AreEqual(img2.Data.GetLength(1), 4);

         Byte[, ,] data = new Byte[,,] { { { 255, 0, 0 } }, { { 0, 255, 0 } } };
         Image<Bgr, Byte> img3 = new Image<Bgr, byte>(data);

         Image<Gray, Single> img4 = new Image<Gray, float>("stuff.jpg");
         Image<Bgr, Single> img5 = new Image<Bgr, float>("stuff.jpg");

         Bitmap bmp = new Bitmap("stuff.jpg");
         Image<Bgr, Single> img6 = new Image<Bgr, float>(bmp);

         Image<Hsv, Single> img7 = new Image<Hsv, float>("stuff.jpg");
         Image<Hsv, Byte> img8 = new Image<Hsv, byte>("stuff.jpg");

      }
#endif
      [Test]
      public void TestSub()
      {
         Image<Bgr, Byte> img = new Image<Bgr, Byte>(101, 133);
         EmguAssert.IsTrue(img.Not().Equals(255 - img));

         Image<Bgr, Byte> img2 = img - 10;
      }

      [Test]
      public void TestConvolutionAndLaplace()
      {
         Image<Gray, Byte> image = new Image<Gray, byte>(300, 400);
         image.SetRandUniform(new MCvScalar(0.0), new MCvScalar(255.0));

         Image<Gray, float> laplace = image.Laplace(1);

         float[,] k = { {0, 1, 0},
                        {1, -4, 1},
                        {0, 1, 0}};
         ConvolutionKernelF kernel = new ConvolutionKernelF(k);

         Image<Gray, float> convoluted = image * kernel;
         EmguAssert.IsTrue(laplace.Equals(convoluted));

         /*
         try
         {
            Matrix<float> kernel1D = new Matrix<float>(new float[] { 1.0f, -2.0f, 1.0f });
            Image<Gray, float> result = new Image<Gray, float>(image.Width, image.Height);
            CvInvoke.cvFilter2D(image, result, kernel1D, new MCvPoint(0, 1));
         }
         catch (Exception e)
         {
            throw e;
         }*/
      }

      private static String GetTempFileName()
      {
         string filename = Path.GetTempFileName();

         File.Delete(filename);

         return Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
      }

#if !IOS
      [Test]
      public void TestImageSave()
      {
         TestImageSaveHelper(".bmp", System.Drawing.Imaging.ImageFormat.Bmp, 0.0);
         TestImageSaveHelper(".png", System.Drawing.Imaging.ImageFormat.Png, 0.0);
         TestImageSaveHelper(".tiff", System.Drawing.Imaging.ImageFormat.Tiff, 0.0);
         TestImageSaveHelper(".tif", System.Drawing.Imaging.ImageFormat.Tiff, 0.0);
         TestImageSaveHelper(".gif", System.Drawing.Imaging.ImageFormat.Gif, 255.0);
         TestImageSaveHelper(".jpg", System.Drawing.Imaging.ImageFormat.Jpeg, 255.0);
         TestImageSaveHelper(".jpeg", System.Drawing.Imaging.ImageFormat.Jpeg, 255.0);
         //TestImageSaveHelper(".ico", System.Drawing.Imaging.ImageFormat.Icon, 255.0);
      }

      private static void TestImageSaveHelper(String extension, System.Drawing.Imaging.ImageFormat format, double epsilon)
      {
         String fileName = GetTempFileName() + extension;
         try
         {
            using (Image<Bgr, Byte> tmp = new Image<Bgr, byte>(601, 479))
            {
               tmp.SetRandUniform(new MCvScalar(), new MCvScalar(255, 255, 255));

               tmp.Save(fileName);

               using (Image i = Image.FromFile(fileName))
               {
                  /*
                  if (System.Drawing.Imaging.ImageFormat.Jpeg.Equals(i.RawFormat))
                     Trace.WriteLine("jpeg");
                  else if (System.Drawing.Imaging.ImageFormat.Gif.Equals(i.RawFormat))
                     Trace.WriteLine("gif");
                  else if (System.Drawing.Imaging.ImageFormat.Png.Equals(i.RawFormat))
                     Trace.WriteLine("png");
                  else if (System.Drawing.Imaging.ImageFormat.Bmp.Equals(i.RawFormat))
                     Trace.WriteLine("bmp");
                  */
                  Assert.IsTrue(i.RawFormat.Equals(format));
               }
               if (epsilon == 0.0)
                  Assert.IsTrue(tmp.Equals(new Image<Bgr, Byte>(fileName)));
               else
               {
                  /*
                  using (Image<Bgr, Byte> delta = new Image<Bgr, Byte>(tmp.Size))
                  using (Image<Gray, Byte> mask = new Image<Gray, byte>(tmp.Size))
                  {
                     CvInvoke.cvAbsDiff(tmp, new Image<Bgr, Byte>(fileName), delta);
                     for (int i = 0; i < delta.NumberOfChannels; i++)
                     {
                        CvInvoke.cvCmpS(delta[i], epsilon, mask, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_GE);
                        int count = CvInvoke.cvCountNonZero(mask);
                        Assert.AreEqual(0, count);
                     }
                  }*/
               }
            }
         } catch (Exception e)
         {
            throw e;
         } finally
         {
            File.Delete(fileName);
         }

      }

      [Test]
      public void TestBitmapConstructor()
      {
         using (Bitmap bmp0 = new Bitmap(1200, 1080, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
         using (Graphics g = Graphics.FromImage(bmp0))
         {
            g.Clear(Color.Blue);

            Stopwatch watch = Stopwatch.StartNew();
            Image<Bgr, Byte> image0 = new Image<Bgr, byte>(bmp0);
            watch.Stop();
            Trace.WriteLine(String.Format("Convertsion Time: {0} milliseconds", watch.ElapsedMilliseconds));
            Image<Bgr, Byte> imageCmp0 = new Image<Bgr, byte>(image0.Size);
            imageCmp0.SetValue(new Bgr(255, 0, 0));
            Assert.IsTrue(image0.Equals(imageCmp0));
         }

         #region test byte images
         Image<Bgr, Byte> image1 = new Image<Bgr, byte>(201, 401);
         image1.SetRandUniform(new MCvScalar(), new MCvScalar(255.0, 255.0, 255.0));
         Assert.IsTrue(image1.Equals(new Image<Bgr, byte>(image1.ToBitmap())));
         Assert.IsTrue(image1.Equals(new Image<Bgr, byte>(image1.Bitmap)));

         Image<Gray, Byte> image3 = new Image<Gray, byte>(11, 7);
         image3.SetRandUniform(new MCvScalar(), new MCvScalar(255.0, 255.0, 255.0));
         Assert.IsTrue(image3.Equals(new Image<Gray, byte>(image3.ToBitmap())));
         Assert.IsTrue(image3.Equals(new Image<Gray, byte>(image3.Bitmap)));

         Image<Bgra, Byte> image5 = new Image<Bgra, byte>(201, 401);
         image5.SetRandUniform(new MCvScalar(), new MCvScalar(255.0, 255.0, 255.0, 255.0));
         Assert.IsTrue(image5.Equals(new Image<Bgra, byte>(image5.ToBitmap())));
         Assert.IsTrue(image5.Equals(new Image<Bgra, byte>(image5.Bitmap)));
         #endregion

         #region test single images
         Image<Bgr, Single> image7 = new Image<Bgr, Single>(201, 401);
         image7.SetRandUniform(new MCvScalar(), new MCvScalar(255.0, 255.0, 255.0));
         Bitmap bmp = image7.ToBitmap();
         #endregion
      }

      [Test]
      public void TestBitmapSharedDataWithImage()
      {
         Image<Bgr, Byte> img = new Image<Bgr,byte>(480, 320);
         Bitmap bmp = img.Bitmap;
         bmp.SetPixel(0, 0, Color.Red);
         Image<Bgr, Byte> img2 = new Image<Bgr,byte>(bmp);
         Assert.IsTrue(img.Equals(img2));
      }
#endif
      [Test]
      public void TestSplitMerge()
      {
         Image<Bgr, Byte> img1 = new Image<Bgr, byte>(301, 234);
         img1.SetRandUniform(new MCvScalar(), new MCvScalar(255, 255, 255));
         Image<Gray, Byte>[] channels = img1.Split();

         Image<Bgr, Byte> img2 = new Image<Bgr, byte>(channels);
         EmguAssert.IsTrue(img1.Equals(img2));
      }

      [Test]
      public void TestAcc()
      {
         Image<Gray, Single> img1 = new Image<Gray, Single>(300, 200);
         img1.SetRandUniform(new MCvScalar(0), new MCvScalar(255));
         Image<Gray, Single> img2 = new Image<Gray, Single>(300, 200);
         img2.SetRandUniform(new MCvScalar(0), new MCvScalar(255));

         Image<Gray, Single> img3 = img1.Copy();
         img3.Acc(img2);

         EmguAssert.IsTrue(img3.Equals(img1 + img2));
      }

      [Test]
      public void TestCanny()
      {
         Image<Bgr, Byte> image = new Image<Bgr, byte>("stuff.jpg");

         //make sure canny works for multi channel image
         Image<Bgr, Byte> image2 = image.Canny(new Bgr(200, 200, 200), new Bgr(100, 100, 100));

         Size size = image2.Size;
      }

      [Test]
      public void TestInplaceFlip()
      {
         Image<Bgr, byte> image = new Image<Bgr, byte>(20, 20);
         image.SetRandUniform(new MCvScalar(), new MCvScalar(255, 255, 255));

         Image<Bgr, byte> imageOld = image.Copy();
         image._Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);

         for (int i = 0; i < image.Rows; i++)
            for (int j = 0; j < image.Cols; j++)
            {
               Bgr c1 = image[i, j];
               Bgr c2 = imageOld[image.Rows - i - 1, j];
               EmguAssert.IsTrue(c1.Equals(c2));
            }
      }

      [Test]
      public void TestFlipPerformance()
      {
         Image<Bgr, byte> image = new Image<Bgr, byte>(2048, 1024);
         image.SetRandNormal(new MCvScalar(), new MCvScalar(255, 255, 255));
         Stopwatch watch = Stopwatch.StartNew();
         image._Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL | Emgu.CV.CvEnum.FLIP.VERTICAL);
         watch.Stop();
         EmguAssert.WriteLine(String.Format("Time used: {0} milliseconds", watch.ElapsedMilliseconds));
      }

      [Test]
      public void TestMoment()
      {
         Image<Gray, byte> image = new Image<Gray, byte>(100, 200);
         image.SetRandUniform(new MCvScalar(), new MCvScalar(255));
         image.ThresholdToZero(new Gray(120));
         MCvMoments moment = image.GetMoments(true);
         MCvHuMoments huMoment = moment.GetHuMoment();
      }

      [Test]
      public void TestSnake()
      {
         Image<Gray, Byte> img = new Image<Gray, Byte>(100, 100, new Gray());

         Rectangle rect = new Rectangle(40, 30, 20, 40);
         img.Draw(rect, new Gray(255.0), -1);

         using (MemStorage stor = new MemStorage())
         {
            Seq<Point> pts = new Seq<Point>((int) CvEnum.SEQ_TYPE.CV_SEQ_POLYGON, stor);
            pts.Push(new Point(20, 20));
            pts.Push(new Point(20, 80));
            pts.Push(new Point(80, 80));
            pts.Push(new Point(80, 20));

            Image<Gray, Byte> canny = img.Canny(new Gray(100.0), new Gray(40.0));
            Seq<Point> snake = canny.Snake(pts, 1.0f, 1.0f, 1.0f, new Size(21, 21), new MCvTermCriteria(40, 0.0002), stor);

            img.Draw(pts, new Gray(120), 1);
            img.Draw(snake, new Gray(80), 2);
         }
      }

      [Test]
      public void TestWaterShed()
      {
         Image<Bgr, Byte> image = new Image<Bgr, byte>("stuff.jpg");
         Image<Gray, Int32> marker = new Image<Gray, Int32>(image.Width, image.Height);
         Rectangle rect = image.ROI;
         marker.Draw(
            new CircleF(
               new PointF(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f),
         /*(float)(Math.Min(image.Width, image.Height) / 20.0f)*/ 5.0f),
            new Gray(255),
            0);
         Image<Bgr, Byte> result = image.ConcateHorizontal(marker.Convert<Bgr, byte>());
         Image<Gray, Byte> mask = new Image<Gray, byte>(image.Size);
         CvInvoke.cvWatershed(image, marker);
         CvInvoke.cvCmpS(marker, 0.5, mask, CvEnum.CMP_TYPE.CV_CMP_GT);

         //ImageViewer.Show(result.ConcateHorizontal(mask.Convert<Bgr, Byte>()));
      }

      [Test]
      public void TestMatrixDFT()
      {
         //The matrix to be transformed.
         Matrix<float> matB = new Matrix<float>(
            new float[,] { 
            {1.0f / 16.0f, 1.0f / 16.0f, 1.0f / 16.0f}, 
            {1.0f / 16.0f, 8.0f / 16.0f, 1.0f / 16.0f}, 
            {1.0f / 16.0f, 1.0f / 16.0f, 1.0f / 16.0f}});

         Matrix<float> matBDft = new Matrix<float>(
            CvInvoke.cvGetOptimalDFTSize(matB.Rows),
            CvInvoke.cvGetOptimalDFTSize(matB.Cols));
         CvInvoke.cvCopyMakeBorder(matB, matBDft, new Point(0, 0), Emgu.CV.CvEnum.BORDER_TYPE.CONSTANT, new MCvScalar());
         Matrix<float> dftIn = new Matrix<float>(matBDft.Rows, matBDft.Cols, 2);
         CvInvoke.cvMerge(matBDft, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, dftIn);

         Matrix<float> dftOut = new Matrix<float>(dftIn.Rows, dftIn.Cols, 2);
         //perform the Fourior Transform
         CvInvoke.cvDFT(dftIn, dftOut, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, matB.Rows);

         //The real part of the Fourior Transform
         Matrix<float> outReal = new Matrix<float>(matBDft.Size);
         //The imaginary part of the Fourior Transform
         Matrix<float> outIm = new Matrix<float>(matBDft.Size);
         CvInvoke.cvSplit(dftOut, outReal, outIm, IntPtr.Zero, IntPtr.Zero);
      }

      [Test]
      public void TestImageDFT()
      {
         Image<Gray, float> matA = new Image<Gray, float>("stuff.jpg");

         //The matrix to be convolved with matA, a bluring filter
         Matrix<float> matB = new Matrix<float>(
            new float[,] { 
            {1.0f / 16.0f, 1.0f / 16.0f, 1.0f / 16.0f}, 
            {1.0f / 16.0f, 8.0f / 16.0f, 1.0f / 16.0f}, 
            {1.0f / 16.0f, 1.0f / 16.0f, 1.0f / 16.0f}});

         Image<Gray, float> convolvedImage = new Image<Gray, float>(matA.Size + matB.Size - new Size(1, 1));

         Matrix<float> dftA = new Matrix<float>(
            CvInvoke.cvGetOptimalDFTSize(convolvedImage.Rows),
            CvInvoke.cvGetOptimalDFTSize(convolvedImage.Cols));
         matA.CopyTo(dftA.GetSubRect(matA.ROI));

         CvInvoke.cvDFT(dftA, dftA, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, matA.Rows);

         Matrix<float> dftB = new Matrix<float>(dftA.Size);
         matB.CopyTo(dftB.GetSubRect(new Rectangle(Point.Empty, matB.Size)));
         CvInvoke.cvDFT(dftB, dftB, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, matB.Rows);

         CvInvoke.cvMulSpectrums(dftA, dftB, dftA, Emgu.CV.CvEnum.MUL_SPECTRUMS_TYPE.DEFAULT);
         CvInvoke.cvDFT(dftA, dftA, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, convolvedImage.Rows);
         dftA.GetSubRect(new Rectangle(Point.Empty, convolvedImage.Size)).CopyTo(convolvedImage);
      }

      [Test]
      public void TestImageDFT2()
      {
         Image<Gray, float> image = new Image<Gray, float>("stuff.jpg");
         IntPtr complexImage = CvInvoke.cvCreateImage(image.Size, Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_32F, 2);
         CvInvoke.cvSetImageCOI(complexImage, 1);
         CvInvoke.cvCopy(image, complexImage, IntPtr.Zero);
         CvInvoke.cvSetImageCOI(complexImage, 0);

         Matrix<float> dft = new Matrix<float>(image.Rows, image.Cols, 2);

         CvInvoke.cvDFT(complexImage, dft, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, 0);
      }

      [Test]
      public void TestResize()
      {
         Image<Gray, Byte> image = new Image<Gray, byte>(123, 321);
         image.SetRandUniform(new MCvScalar(), new MCvScalar(255));
         image.Resize(512, 512, CvEnum.INTER.CV_INTER_CUBIC);
      }

      [Test]
      public void TestRoi()
      {
         Image<Bgr, Byte> image = new Image<Bgr, byte>(1, 1);
         Rectangle roi = image.ROI;

         EmguAssert.AreEqual(roi.Width, image.Width);
         EmguAssert.AreEqual(roi.Height, image.Height);
      }

      [Test]
      public void TestGetSubRect()
      {
         Image<Bgr, Single> image = new Image<Bgr, float>(200, 100);
         image.SetRandUniform(new MCvScalar(), new MCvScalar(255, 255, 255, 255));
         Rectangle roi = new Rectangle(10, 20, 30, 40);
         Image<Bgr, Single> roi1 = image.Copy(roi);
         Image<Bgr, Single> roi2 = image.GetSubRect(roi);
         EmguAssert.IsTrue(roi1.Equals(roi2));
      }

      [Test]
      public void TestGoodFeature()
      {
         using (Image<Bgr, Byte> img = new Image<Bgr, Byte>("stuff.jpg"))
         {
            PointF[][] pts = img.GoodFeaturesToTrack(100, 0.1, 10, 5);
            img.FindCornerSubPix(pts, new Size(5, 5), new Size(-1, -1), new MCvTermCriteria(20, 0.0001));

            foreach (PointF p in pts[0])
               img.Draw(new CircleF(p, 3.0f), new Bgr(255, 0, 0), 1);
         }

         //using(Util.TbbTaskScheduler scheduler = new Util.TbbTaskScheduler())
         using (Image<Bgr, Byte> img = new Image<Bgr, Byte>("stuff.jpg"))
         {
            Stopwatch watch = Stopwatch.StartNew();
            int runs = 10;
            for (int i = 0; i < runs; i++)
            {
               PointF[][] pts = img.GoodFeaturesToTrack(100, 0.1, 10, 5);
            }
            watch.Stop();
            EmguAssert.WriteLine(String.Format("Avg time to extract good features from image of {0}: {1}", img.Size, watch.ElapsedMilliseconds / runs));
         }
      }

      [Test]
      public void TestContour()
      {
         Image<Gray, Byte> img = new Image<Gray, byte>("stuff.jpg");
         img.SmoothGaussian(3);
         img = img.Canny(new Gray(80), new Gray(50));
         Image<Gray, Byte> res = img.CopyBlank();
         res.SetValue(255);

         Contour<Point> contour = img.FindContours();

         while (contour != null)
         {
            Contour<Point> approx = contour.ApproxPoly(contour.Perimeter * 0.05);

            if (approx.Convex && approx.Area > 20.0)
            {
               Point[] vertices = approx.ToArray();

               LineSegment2D[] edges = PointCollection.PolyLine(vertices, true);

               res.DrawPolyline(vertices, true, new Gray(200), 1);
            }
            contour = contour.HNext;
         }
      }

      public void TestContour2()
      {
         Image<Bgr, Byte> img1 = new Image<Bgr, byte>(200, 200);
         Image<Bgr, Byte> img2 = new Image<Bgr, byte>(200, 200);
         using (MemStorage stor = new MemStorage())
         {
            Point[] polyline = new Point[] {
               new Point(20, 20),
               new Point(20, 30),
               new Point(30, 30),
               new Point(30, 20)};



            Contour<Point> c = new Contour<Point>(stor);
            c.PushMulti(polyline, Emgu.CV.CvEnum.BACK_OR_FRONT.FRONT);

            img1.Draw(c, new Bgr(255, 0, 0), new Bgr(), 0, -1, new Point(0, 0));
            img1.Draw(c, new Bgr(0, 255, 0), new Bgr(), 0, -1, new Point(20, 10));
            img1.Draw(c, new Bgr(0, 0, 255), new Bgr(), 0, 1, new Point(20, 10));

            /*
            for (int i = 0; i < polyline.Length; i++)
            {
               polyline[i].X += 20;
               polyline[i].Y += 10;
            }
            img1.DrawPolyline(polyline, true, new Bgr(0, 0, 255), 1);
             */
         }
      }

      [Test]
      public void TestBayerBG2BGR()
      {
         Image<Gray, Byte> image = new Image<Gray, byte>(200, 200);
         image.SetRandUniform(new MCvScalar(), new MCvScalar(255));
         Image<Bgr, Byte> img = new Image<Bgr, byte>(image.Size);
         CvInvoke.cvCvtColor(image, img, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BayerBG2BGR);
      }

      [Test]
      public void TestGamma()
      {
         Image<Bgr, Byte> img = new Image<Bgr, byte>(320, 240);
         img.SetRandNormal(new MCvScalar(120, 120, 120), new MCvScalar(50, 50, 50));
         img._GammaCorrect(0.5);
      }

      [Test]
      public void TestImageIndexer()
      {
         using (Image<Bgr, Byte> image = new Image<Bgr, byte>(100, 500))
         {
            image.SetRandUniform(new MCvScalar(), new MCvScalar(255.0, 255.0, 255.0));
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < image.Height; i++)
               for (int j = 0; j < image.Width; j++)
               {
                  Bgr color = image[i, j];
               }
            watch.Stop();
            EmguAssert.WriteLine("Time used: " + watch.ElapsedMilliseconds + ".");
            watch = Stopwatch.StartNew();
            for (int i = 0; i < image.Height; i++)
               for (int j = 0; j < image.Width; j++)
                  for (int k = 0; k < image.NumberOfChannels; k++)
                  {
                     Byte b = image.Data[i, j, k];
                  }
            watch.Stop();
            EmguAssert.WriteLine("Time used: " + watch.ElapsedMilliseconds + ".");
         }
      }

      [Test]
      public void TestSetRandomNormal()
      {
         Image<Bgr, Byte> image = new Image<Bgr, byte>(400, 200);
         //image.SetRandUniform(new MCvScalar(), new MCvScalar(255, 255, 255));
         image.SetRandNormal(new MCvScalar(100, 100, 100), new MCvScalar(20, 20, 20));
      }

      [Test]
      public void TestGenericConvert()
      {
         Image<Gray, Single> g = new Image<Gray, Single>(80, 40);
         Image<Gray, Single> g2 = g.Convert<Single>(delegate(Single v, int x, int y) {
            return System.Convert.ToSingle(Math.Sqrt(0.0 + x * x + y * y)); });
      }

      [Test]
      public void TestDrawHorizontalLine()
      {
         Point p1 = new Point(10, 10);
         Point p2 = new Point(20, 10);
         LineSegment2D l1 = new LineSegment2D(p1, p2);
         Image<Bgr, Byte> img = new Image<Bgr, byte>(200, 400, new Bgr(255, 255, 255));
         img.Draw(l1, new Bgr(0.0, 0.0, 0.0), 1);
      }

      [Test]
      public void TestMapDrawRectangle()
      {
         PointF p1 = new PointF(1.1f, 2.2f);
         SizeF p2 = new SizeF(2.2f, 4.4f);
         RectangleF rect = new RectangleF();
         rect.Location = PointF.Empty;
         rect.Size = p2;

         Map<Gray, Byte> map = new Map<Gray, Byte>(new RectangleF(PointF.Empty, new SizeF(4.0f, 8.0f)), new PointF(0.1f, 0.1f), new Gray(255.0));
         map.Draw(rect, new Gray(0.0), 1);
      }

      [Test]
      public void TestGetSubRect2()
      {
         Image<Bgr, Byte> image = new Image<Bgr, byte>(2048, 2048);
         image.SetRandUniform(new MCvScalar(), new MCvScalar(255, 255, 255));
         Rectangle rect = new Rectangle(new Point(99, 99), new Size(105, 103));
         image.ROI = rect;
         Image<Bgr, Byte> region = image.Copy();
         image.ROI = Rectangle.Empty;
         EmguAssert.IsTrue(image.GetSubRect(rect).Equals(region));

         Stopwatch watch = Stopwatch.StartNew();
         for (int i = 0; i < 100000; i++)
         {
            Image<Bgr, Byte> tmp = image.GetSubRect(rect);
         }
         watch.Stop();
         EmguAssert.WriteLine(String.Format("Time used: {0} milliseconds.", watch.ElapsedMilliseconds));

      }

      [Test]
      public void TestImageLoader()
      {
         using (Image<Bgr, Single> img = new Image<Bgr, Single>("stuff.jpg"))
         using (Image<Bgr, Single> img2 = img.Resize(100, 100, CvEnum.INTER.CV_INTER_AREA, true))
         {
            Rectangle r = img2.ROI;
            r.Width >>= 1;
            r.Height >>= 1;
            img2.ROI = r;
         }
      }

      [Test]
      public void TestBgrSplit()
      {
         using (Image<Bgr, Byte> img = new Image<Bgr, byte>(100, 100, new Bgr(0, 100, 200)))
         {
            Image<Gray, Byte>[] channels = img.Split();
            EmguAssert.AreEqual(img.NumberOfChannels, channels.Length);
         }
      }

      [Test]
      public void TestDrawFont()
      {
         using (Image<Gray, Byte> img = new Image<Gray, Byte>(200, 300, new Gray()))
         {
            MCvFont f = new MCvFont(CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, 1.0, 1.0);
            {
               img.Draw("h.", ref f, new Point(100, 10), new Gray(255.0));
               img.Draw("a.", ref f, new Point(100, 50), new Gray(255.0));
            }
         }
      }

      [Test]
      public void TestThreshold()
      {
         using (Image<Gray, Byte> image = new Image<Gray, byte>("stuff.jpg"))
         {
            Image<Gray, Byte> thresh1 = new Image<Gray, byte>(image.Size);
            Image<Gray, Byte> thresh2 = new Image<Gray, byte>(image.Size);
            CvInvoke.cvThreshold(image, thresh1, 0, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU | Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);
            CvInvoke.cvThreshold(image, thresh2, 255, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU | Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);

            EmguAssert.IsTrue(thresh1.Equals(thresh2));
         }
      }

      [Test]
      public void TestBgra()
      {
         Image<Bgra, Byte> img = new Image<Bgra, byte>(100, 100);
         img.SetValue(new Bgra(255.0, 120.0, 0.0, 120.0));
         Image<Gray, Byte>[] channels = img.Split();
      }

      [Test]
      public void TestMixed()
      {
         using (Image<Bgr, Byte> img = new Image<Bgr, byte>("stuff.jpg"))
         {
            using (Image<Hsv, Byte> imgHsv = img.Convert<Hsv, Byte>())
            {
               Image<Gray, Byte>[] imgs = imgHsv.Split();
               using (Image<Hsv, Byte> imgHsv2 = new Image<Hsv, Byte>(imgs))
               {
                  using (Image<Bgr, Byte> imageRGB = imgHsv2.Convert<Bgr, Byte>())
                  {
                     LineSegment2D[][] lines = imgHsv2.HoughLines(
                         new Hsv(50.0, 50.0, 50.0), new Hsv(200.0, 200.0, 200.0),
                         1, Math.PI / 180.0, 50, 50, 10);

                     CircleF[][] circles = img.HoughCircles(
                         new Bgr(200.0, 200.0, 200.0), new Bgr(100.0, 100.0, 100.0),
                         4.0, 1.0, 0, 0);

                     for (int i = 0; i < lines[0].Length; i++)
                     {
                        imageRGB.Draw(lines[0][i], new Bgr(255.0, 0.0, 0.0), 1);
                     }

                     for (int i = 0; i < lines[1].Length; i++)
                     {
                        imageRGB.Draw(lines[1][i], new Bgr(0.0, 255.0, 0.0), 1);
                     }

                     for (int i = 0; i < lines[2].Length; i++)
                     {
                        imageRGB.Draw(lines[2][i], new Bgr(0.0, 0.0, 255.0), 1);
                     }

                     foreach (CircleF[] cs in circles)
                        foreach (CircleF c in cs)
                           imageRGB.Draw(c, new Bgr(0.0, 0.0, 0.0), 1);

                     //Application.Run(new ImageViewer(imageRGB));

                     bool applied = false;
                     foreach (CircleF[] cs in circles)
                        foreach (CircleF c in cs)
                        {
                           if (!applied)
                           {
                              CircleF cir = c;
                              cir.Radius += 30;
                              using (Image<Gray, Byte> mask = new Image<Gray, Byte>(imageRGB.Width, imageRGB.Height, new Gray(0.0)))
                              {
                                 mask.Draw(cir, new Gray(255.0), -1);

                                 using (Image<Bgr, Byte> res = imageRGB.InPaint(mask, 50))
                                 {

                                 }
                              }
                              applied = true;
                           }
                        }
                  }
               }

               foreach (Image<Gray, Byte> i in imgs)
                  i.Dispose();
            }
         }
      }

      [Test]
      public void TestImageConvert()
      {
         try
         {
            Image<Bgr, double> img1 = new Image<Bgr, double>("box.png");
            Image<Gray, double> img2 = img1.Convert<Gray, double>();
         } catch (NotSupportedException)
         {
            return;
         }
         Assert.Fail("NotSupportedException should be thrown");
      }

      /*
      [Test]
      public void TestPlanarObjectDetector()
      {
         Image<Gray, byte> box = new Image<Gray, byte>("box.png");
         Image<Gray, byte> scene = new Image<Gray, byte>("box_in_scene.png");
         //Image<Gray, Byte> scene = box.Rotate(1, new Gray(), false);

         using (PlanarObjectDetector detector = new PlanarObjectDetector())
         {
            Stopwatch watch = Stopwatch.StartNew();
            LDetector keypointDetector = new LDetector();
            keypointDetector.Init();

            PatchGenerator pGen = new PatchGenerator();
            pGen.SetDefaultParameters();

            detector.Train(box, 300, 31, 50, 9, 5000, ref keypointDetector, ref pGen);
            watch.Stop();
            EmguAssert.WriteLine(String.Format("Training time: {0} milliseconds.", watch.ElapsedMilliseconds));

            MKeyPoint[] modelPoints = detector.GetModelPoints();
            int i = modelPoints.Length;

            HomographyMatrix h = new HomographyMatrix();
            watch = Stopwatch.StartNew();
            PointF[] corners = detector.Detect(scene, h);
            watch.Stop();
            EmguAssert.WriteLine(String.Format("Detection time: {0} milliseconds.", watch.ElapsedMilliseconds));

            foreach (PointF c in corners)
            {
               scene.Draw(new CircleF(c, 2), new Gray(255), 1);
            }
            scene.DrawPolyline(Array.ConvertAll<PointF, Point>(corners, Point.Round), true, new Gray(255), 2);

            //ImageViewer.Show(scene);
         }
      }*/

      [Test]
      public void TestSaveImage()
      {
         String fileName = Path.Combine(Path.GetTempPath(), "tmp.jpg");
         DateTime t1 = DateTime.Now;
         for (int i = 0; i < 10; i++)
         {
            Image<Gray, Byte> img = new Image<Gray, byte>("stuff.jpg");
            img.Save(fileName);
         }
         EmguAssert.WriteLine(String.Format("Time needed to save the image {0}", DateTime.Now.Subtract(t1).TotalMilliseconds / 10));
         if (File.Exists(fileName))
            File.Delete(fileName);
      }

      [Test]
      public void PerformanceComparison()
      {
         Image<Gray, Byte> img1 = new Image<Gray, byte>(1920, 1080);
         Image<Gray, Byte> img2 = new Image<Gray, byte>(img1.Size);


         img1.SetRandUniform(new MCvScalar(0), new MCvScalar(50));
         img2.SetRandUniform(new MCvScalar(0), new MCvScalar(50));

         Stopwatch w = Stopwatch.StartNew();
         Image<Gray, Byte> sum1 = img1 + img2;
         w.Stop();
         EmguAssert.WriteLine(String.Format("OpenCV Time:\t\t\t\t\t\t{0} ms", w.ElapsedMilliseconds));


         w.Reset();
         w.Start();
         Image<Gray, Byte> sum2 = new Image<Gray, byte>(img1.Size);
         Byte[, ,] data1 = img1.Data;
         Byte[, ,] data2 = img2.Data;
         Byte[, ,] dataSum = sum2.Data;
         int rows = img1.Rows;
         int cols = img1.Cols;
         for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
               dataSum[i, j, 0] = (Byte) (data1[i, j, 0] + data2[i, j, 0]);
         w.Stop();
         EmguAssert.WriteLine(String.Format(".NET array manipulation Time:\t\t{0} ms", w.ElapsedMilliseconds));

         EmguAssert.IsTrue(sum2.Equals(sum1));

         w.Reset();
         w.Start();
         Func<Byte, Byte, Byte> convertor = delegate(Byte b1, Byte b2) {
            return (Byte) (b1 + b2); };
         Image<Gray, Byte> sum3 = img1.Convert<Byte, Byte>(img2, convertor);
         w.Stop();
         EmguAssert.WriteLine(String.Format("Generic image manipulation Time:\t{0} ms", w.ElapsedMilliseconds));

         EmguAssert.IsTrue(sum3.Equals(sum1));

      }

#if !IOS
      [Test]
      public void TestMultiThreadInMemoryWithBMP()
      {
         if (Emgu.Util.Platform.OperationSystem == Emgu.Util.TypeEnum.OS.Windows)
         {
            int threadCount = 32;

            //Create some random images and save to hard disk
            Bitmap[] imageNames = new Bitmap[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
               using (Image<Bgr, Byte> img = new Image<Bgr, byte>(2048, 1024))
               {
                  img.SetRandNormal(new MCvScalar(100, 100, 100), new MCvScalar(50, 50, 50));
                  imageNames[i] = img.ToBitmap();
               }
            }

            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
               int index = i;
               threads[i] = new Thread(delegate()
               {
                  Image<Gray, Byte> img = new Image<Gray, byte>(imageNames[index]);
                  Image<Gray, Byte> bmpClone = new Image<Gray, byte>(img.Bitmap);
               });

               threads[i].Priority = ThreadPriority.Highest;
               threads[i].Start();
            }

            for (int i = 0; i < threadCount; i++)
            {
               threads[i].Join();
            }
         }
      }

      [Test]
      public void TestMultiThreadWithBMP()
      {
         //TODO: find out why this test fails on unix
         if (Emgu.Util.Platform.OperationSystem == Emgu.Util.TypeEnum.OS.Windows)
         {
            int threadCount = 32;

            //Create some random images and save to hard disk
            String[] imageNames = new String[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
               using (Image<Bgr, Byte> img = new Image<Bgr, byte>(2048, 1024))
               {
                  img.SetRandNormal(new MCvScalar(100, 100, 100), new MCvScalar(50, 50, 50));
                  imageNames[i] = String.Format("tmp{0}.bmp", i);
                  img.Save(imageNames[i]);
               }
            }

            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
               int index = i;
               threads[i] = new Thread(delegate()
               {
                  {
                     Image<Gray, Byte> img = new Image<Gray, byte>(imageNames[index]);
                     Image<Gray, Byte> bmpClone = new Image<Gray, byte>(img.Bitmap);
                  }
               });

               threads[i].Priority = ThreadPriority.Highest;
               threads[i].Start();
            }

            for (int i = 0; i < threadCount; i++)
            {
               threads[i].Join();
            }

            //delete random images;
            foreach (string s in imageNames)
               File.Delete(s);
         }
      }
#endif

      [Test]
      public void TestMorphologyClosing()
      {
         //draw some blobs
         Image<Gray, Byte> img = new Image<Gray, byte>(400, 400);
         MCvBox2D box1 = new MCvBox2D(new PointF(100, 200), new SizeF(60, 80), 30.0f);
         MCvBox2D box2 = new MCvBox2D(new PointF(180, 250), new SizeF(70, 100), 0.0f);
         img.Draw(box1, new Gray(255.0), -1);
         img.Draw(box2, new Gray(255.0), -1);

         Image<Gray, Byte> result = img.ConcateHorizontal(MorphologyClosing(img, 10));
         //ImageViewer.Show(result, "Left: original, Right: merged");
      }

      public static Image<Gray, Byte> MorphologyClosing(Image<Gray, Byte> img, int radius)
      {
         int kernelSize = radius * 2 + 1;
         int[,] kernelMat = new int[kernelSize, kernelSize];
         for (int i = 0; i < kernelSize; i++)
            for (int j = 0; j < kernelSize; j++)
            {
               double dx = i - (radius);
               double dy = j - (radius);
               double dist = Math.Sqrt(dx * dx + dy * dy);
               if (dist <= radius)
               {
                  kernelMat[i, j] = 1;
               }
            }

         //for definition on the close operation, see.
         //http://en.wikipedia.org/wiki/Mathematical_morphology
         using (StructuringElementEx e = new StructuringElementEx(kernelMat, radius, radius))
         {
            return img.MorphologyEx(e, CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);
         }
      }
   }
}
