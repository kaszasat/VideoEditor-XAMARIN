using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoEditor.ViewModel.Helper;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Xabe.FFmpeg.Exceptions;
using Xamarin.Forms;

namespace VideoEditor.Model
{
    public sealed class WrapperFunctions
    {
        private static WrapperFunctions mInstance;
        private static readonly Random random = new Random();

        /// <summary>
        /// Singleton WrapperFunctions osztály példányosítása.
        /// </summary>
        public static WrapperFunctions Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new WrapperFunctions();
                }
                return mInstance;
            }
        }

        static Logger Logger { get; set; }
        public static int Progress { get; set; }
        public static IConversion Conversion { get; set; }

        public static bool FFmpegIsDownloaded { get; set; } = false;

        /// <summary>
        /// WrapperFunctions osztály konstruktora. Megkezdni a bináris futtatható FFmpeg fájl letöltését.
        /// </summary>
        private WrapperFunctions()
        {
            Logger = Logger.Instance;
            Progress = 0;
            if (Device.RuntimePlatform == Device.UWP)
            {
                DownloadFFmpeg();
                Conversion = FFmpeg.Conversions.New();
            }
        }

        /// <summary>
        /// Letölti az FFmpeg bináris futtatható FFmpeg fájlt, kinaplózva a futtatható fájl letöltési helyét.
        /// </summary>
        private async static void DownloadFFmpeg()
        {
            var UWPFFmpegLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            if (!File.Exists(Path.Combine(UWPFFmpegLocation, "ffprobe.exe"))
                && !File.Exists(Path.Combine(UWPFFmpegLocation, "ffmpeg.exe")))
            {
                Log("Downloading latest ffmpeg...");
                Log($"Download location: {UWPFFmpegLocation}");
                try
                {
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, UWPFFmpegLocation);
                    Log("Downloading latest ffmpeg finished.");
                    FFmpegIsDownloaded = true;
                }
                catch (Exception ex)
                {
                    FFmpegIsDownloaded = false;
                    Log(ex.Message);
                }
            }
            else
            {
                FFmpegIsDownloaded = true;
                FFmpeg.SetExecutablesPath(UWPFFmpegLocation);
            }
        }

        /// <summary>
        /// Továbbítja a helyi binárisan futtatható FFmpeg felé a videót amelyhet hatást adunk és paramétereket.
        /// Bemeneti értékek a hatás megnevezése, a médiafájl amely éppen lejátszva van a nézetben, a felső és alsó tartománycsúszka értéke és a cancellation token.
        /// </summary>
        internal async static Task AddEffectToVideo(string effectName, string filePath, string destination, double lowerValue, double uppervalue, CancellationToken token)
        {
            if (!FFmpegIsDownloaded)
            {
                Log("FFmpeg is not downloaded yet.");
                return;
            }
            switch (effectName)
            {
                case "Black and White":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS,eq=contrast=250.0[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Grayscale":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"colorchannelmixer=.3:.4:.3:0:.3:.4:.3:0:.3:.4:.3[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Sepia":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"colorchannelmixer=.393:.769:.189:0:.349:.686:.168:0:.272:.534:.131[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Darker":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"colorlevels=rimin=0.058:gimin=0.058:bimin=0.058[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Increased contrast":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"colorlevels=rimin=0.039:gimin=0.039:bimin=0.039:rimax=0.96:gimax=0.96:bimax=0.96[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Lighter":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"colorlevels=rimax=0.902:gimax=0.902:bimax=0.902[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Increased brightness":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"colorlevels=romin=0.5:gomin=0.5:bomin=0.5[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Sharpen":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"convolution='0 -1 0 -1 5 -1 0 -1 0:0 -1 0 -1 5 -1 0 -1 0:0 -1 0 -1 5 -1 0 -1 0:0 -1 0 -1 5 -1 0 -1 0'[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Laplacian edge detector which includes diagonals":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"convolution='1 1 1 1 -8 1 1 1 1:1 1 1 1 -8 1 1 1 1:1 1 1 1 -8 1 1 1 1:1 1 1 1 -8 1 1 1 1:5:5:5:1:0:128:128:0'[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Blur":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"convolution='1 1 1 1 1 1 1 1 1:1 1 1 1 1 1 1 1 1:1 1 1 1 1 1 1 1 1:1 1 1 1 1 1 1 1 1:1/9:1/9:1/9:1/9'[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Edge enhance":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"convolution='0 0 0 -1 1 0 0 0 0:0 0 0 -1 1 0 0 0 0:0 0 0 -1 1 0 0 0 0:0 0 0 -1 1 0 0 0 0:5:1:1:1:0:128:128:128'[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Edge detect":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"convolution='0 1 0 1 -4 1 0 1 0:0 1 0 1 -4 1 0 1 0:0 1 0 1 -4 1 0 1 0:0 1 0 1 -4 1 0 1 0:5:5:5:1:0:128:128:128'[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Emboss":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"convolution='-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2'[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;

                case "Red color cast to shadows":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"colorbalance=rs=.3[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Slightly increased middle level of blue":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"curves=blue='0/0 0.5/0.58 1/1'[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Vintage effect":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"curves=r='0/0.11 .42/.51 1/0.95':g='0/0 0.50/0.48 1/1':b='0/0.22 .49/.44 1/0.8'[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Denoise with a sigma of 4.5":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"dctdnoiz=4.5[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Weak deblock":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"deblock=filter=weak:block=4[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
                case "Strong deblock":
                    await Conversion.Start($"-y -i \"{filePath}\" " +
                        $"-vf \"split=3[pre][affected][post];" +
                        $"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        $"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        $"deblock=filter=strong:block=4:alpha=0.12:beta=0.07:gamma=0.06:delta=0.05[affected];" +
                        $"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        $"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        $" \"{destination}\"", token);
                    break;
            }
        }

        /// <summary>
        /// Egy útvonalon elhelyezkedő videó hosszát megadja double formában.
        /// Bemeneti paraméter az útvonal.
        /// </summary>
        public async static Task<double> GetVideoLength(string fullPath)
        {
            if (!FFmpegIsDownloaded)
            {
                Log("FFmpeg is not downloaded yet.");
                return 0;
            }
            try
            {
                return (await FFmpeg.GetMediaInfo(fullPath)).Duration.TotalSeconds;
            }
            catch (ConversionException ex)
            {
                Log(ex.Message);
                Log(ex.InputParameters);
                Log(ex.InnerException.Message);
                return 0;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Egy útvonalon elhelyezkedő hang hosszát megadja double formában.
        /// Bemeneti paraméter az útvonal.
        /// </summary>
        internal async static Task<double> GetAudioLength(string fullPath)
        {
            if (!FFmpegIsDownloaded)
            {
                Log("FFmpeg is not downloaded yet.");
                return 0;
            }
            try
            {
                return (await FFmpeg.GetMediaInfo(fullPath)).Duration.TotalSeconds;
            }
            catch (ConversionException ex)
            {
                Log(ex.Message);
                Log(ex.InputParameters);
                Log(ex.InnerException.Message);
                return 0;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Továbbítja a helyi binárisan futtatható FFmpeg felé az újrarendezésre szánt videókat és paramétereket.
        /// Bemeneti értékek a médiafájlok melyek ki lettek választva és a cancellation token.
        /// </summary>
        internal async static Task Rearrange(ItemsGroupViewModel itemsGroupViewModel, string outputFileName, CancellationToken token)
        {
            if (!FFmpegIsDownloaded)
            {
                Log("FFmpeg is not downloaded yet.");
                return;
            }

            string inputCommandParamteres = "";
            string conversionParameters = "";
            int counter = 0;
            string endOfConversionParameters = "";

            foreach (ItemViewModel item in itemsGroupViewModel)
            {
                if (!File.Exists(item.FullPath))
                {
                    throw new FileNotFoundException($"This file was not found. {item.FullPath}");
                }

                inputCommandParamteres += $" -i \"{item.FullPath}\" ";
                conversionParameters += $"[{counter}:v]scale=1920:1080:force_original_aspect_ratio=decrease,pad=1920:1080:-1:-1,setsar=1,fps=30,format=yuv420p[v{counter}];";
                endOfConversionParameters += $"[v{counter}][{counter}:a]";
                counter++;
            }

            await Conversion
                   .Start($"-y {inputCommandParamteres} " +
                   $"-filter_complex \"{conversionParameters}{endOfConversionParameters}concat=n={itemsGroupViewModel.Count}:v=1:a=1[v][a]\"" +
                   $" -map \"[v]\" -map \"[a]\" -c:v libx264 -c:a aac -movflags +faststart  \"{outputFileName}\"", token);
        }

        /// <summary>
        /// Továbbítja a helyi binárisan futtatható FFmpeg felé a hangot melyet a videóhoz adunk és videót melyhez a hangot adjuk és paramétereket.
        /// Bemeneti értékek a médiafájl amely éppen lejátszva van a nézetben, a hangfájl, a felső és alsó tartománycsúszka értéke és a cancellation token.
        /// </summary>
        internal async static Task AddAudioToVideo(string PathToVideoFile, string PathToAudioFile, string outputfilename, double LowerValue, double UpperValue, CancellationToken token)
        {
            if (!FFmpegIsDownloaded)
            {
                Log("FFmpeg is not downloaded yet.");
                return;
            }

            Object[] timeParameters = HoursMinutesSecondsConverter(0, UpperValue);

            string audiotrimcommand = $" -ss " +
                    $"{timeParameters[0]}:{timeParameters[1]}:{timeParameters[2].ToString().Substring(0, 4)} -t " +
                    $"{timeParameters[3]}:{timeParameters[4]}:{timeParameters[5].ToString().Substring(0, 4)} ";

            timeParameters = HoursMinutesSecondsConverter(LowerValue, 0);
            string videoaddeffectcommand = " -itsoffset " +
                    $"{timeParameters[0]}:{timeParameters[1]}:{timeParameters[2]} ";

            await
              Conversion.Start($"-y -i \"{PathToVideoFile}\" {videoaddeffectcommand} {audiotrimcommand} -i  \"{PathToAudioFile}\" " +
            $" -filter_complex amix -map 0:v -map 0:a -map 1:a -c:v copy -c:a aac -strict experimental -async 1 \"{outputfilename}\"", token);

        }

        /// <summary>
        /// Továbbítja a helyi binárisan futtatható FFmpeg felé a montázs készítésére szánt képeket és paramétereket.
        /// Bemeneti értékek a képek melyek ki lettek választva és a cancellation token.
        /// </summary>
        internal async static Task CreateMontage(ItemsGroupViewModel itemsGroupViewModel, string outputfilename, CancellationToken token)
        {
            if (!FFmpegIsDownloaded)
            {
                Log("FFmpeg is not downloaded yet.");
                return;
            }

            string inputCommandParamters = "";
            foreach (ItemViewModel item in itemsGroupViewModel)
            {
                inputCommandParamters += $"-loop 1 -t 3 -i \"{item.FullPath}\" ";
            }
            try
            {
                await
                    Conversion.Start($"" +
                    $"-y {inputCommandParamters} -f lavfi -i anullsrc -filter_complex " +
                    $"\"concat=n={itemsGroupViewModel.Count}:v=1:a=0:unsafe=1,scale=1920:1080:force_original_aspect_ratio=decrease:eval=frame," +
                    $"pad=1920:1080:-1:-1:eval=frame\" -c:v libx264 -pix_fmt yuv420p " +
                    $"-r 25 -movflags +faststart -shortest \"{outputfilename}\"", token);
            }
            catch (ConversionException ex)
            {
                Log(ex.Message);
                Log(ex.InputParameters);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Kinaplóz string üzenetet a jelenlegi dátummal és idővel.
        /// </summary>
        public static void Log(string message)
        {
            var longtimestr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            Logger.LogText.Append($"\n{longtimestr} - {message}");
            Logger.OnPropertyChanged(nameof(Logger.LogText));
        }

        /// <summary>
        /// Továbbítja a helyi binárisan futtatható FFmpeg felé a vágásra szánt videót és paramétereket.
        /// Bemeneti értékek a médiafájl amely éppen lejátszva van a nézetben, a felső és alsó tartománycsúszka értéke és a cancellation token.
        /// </summary>
        public async static Task TrimVideo(string PathToFile, string outputFileName, double LowerValue, double UpperValue, CancellationToken token)
        {
            if (!FFmpegIsDownloaded)
            {
                Log("FFmpeg is not downloaded yet.");
                return;
            }
            Object[] timeParameters = HoursMinutesSecondsConverter(LowerValue, UpperValue);
            try
            {
                await Conversion
                    .Start($"-y -i \"{PathToFile}\" -ss " +
                    $"{timeParameters[0]}:{timeParameters[1]}:{timeParameters[2].ToString().Substring(0, 4)} -t " +
                    $"{timeParameters[3]}:{timeParameters[4]}:{timeParameters[5].ToString().Substring(0, 4)} \"{outputFileName}\"", token);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Átalakít egy alsó és felső tartománycsúszka értéket és visszaadja objektum formájában az alsó értéket és az alsó
        /// értéktől a felső értékig tartó időintervallumot melyet behelyettesítéssel ÓraÓra:PercPerc:MásodpercMásodperc.Törtérték formában megjeleníthető.
        /// </summary>
        public static Object[] HoursMinutesSecondsConverter(double fromSeconds, double upperValue)
        {
            double timespanSeconds = upperValue - fromSeconds;

            int fromHours = (int)fromSeconds / (60 * 60);
            int fromMinutes = (int)fromSeconds / (60) - fromHours * 60;
            int fromRemainingSeconds = (int)fromSeconds - fromHours * 60 * 60 - fromMinutes * 60;

            int spanHours = (int)timespanSeconds / (60 * 60);
            int spanMinutes = (int)timespanSeconds / (60) - spanHours * 60;
            int spanRemainingSeconds = (int)timespanSeconds - spanHours * 60 * 60 - spanMinutes * 60;

            double returnval1 = fromRemainingSeconds +
                fromSeconds - (fromHours * 60 * 60 + fromMinutes * 60 + fromRemainingSeconds);
            double returnval2 = spanRemainingSeconds +
                timespanSeconds - (spanHours * 60 * 60 + spanMinutes * 60 + spanRemainingSeconds);

            if (returnval1.ToString().Length < 4)
            {
                returnval1 += 0.001;
            }
            if (returnval2.ToString().Length < 4)
            {
                returnval2 += 0.001;
            }

            return new Object[] {
                fromHours,
                fromMinutes,
                returnval1.ToString().Substring(0, 4),
                spanHours,
                spanMinutes,
                returnval2.ToString().Substring(0, 4)
            };
        }

        /// <summary>
        /// Egy útvonalon elhelyezkedő videó alapján generál egy thumbnail-t, miniatűrt.
        /// </summary>
        public async static Task<string> GetThumbnail(string path)
        {
            if (!FFmpegIsDownloaded)
            {
                Log("FFmpeg is not downloaded yet.");
                return "";
            }

            string inputfilename = Path.GetFileName(path);
            string inputFolderName = Path.GetDirectoryName(path);

            string outputfilename = $"{inputFolderName}\\thumbnail_{inputfilename}.png";
            if (!File.Exists(outputfilename))
            {
                try
                {
                    IConversionResult result = await
                        FFmpeg.Conversions.New().Start($" -y -i \"{path}\" -ss 00:00:01.000 -vframes 1 \"{outputfilename}\"");
                    _ = result.Duration;
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }
            }
            return outputfilename;
        }

        /// <summary>
        /// N számú hosszú random stringet generál.
        /// </summary>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
