using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoEditor.ViewModel.Helper;
using Xamarin.Forms;

namespace VideoEditor.Model
{
    public sealed class ClientSideFunctions
    {
        static Logger Logger { get; set; }
        public static string ServerIP { get; set; }
        private static readonly Random random = new Random();
        private static ClientSideFunctions mInstance;
        /// <summary>
        /// Singleton ClientSideFunctions osztály példányosítása
        /// </summary>
        public static ClientSideFunctions Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ClientSideFunctions();
                }
                return mInstance;
            }
        }
        /// <summary>
        /// ClientSideFunctions konstruktora, szerver elérése van itt meghatározva
        /// </summary>
        private ClientSideFunctions()
        { 
            Logger = Logger.Instance;
            ServerIP = "http://192.168.8.100:5000";
        }

        /// <summary>
        /// Szerver számára elküldeni a vágásra szánt videót és paramétereket.
        /// Bemeneti értékek a médiafájl amely éppen lejátszva van a nézetben, a felső és alsó tartománycsúszka értéke és a cancellation token.
        /// </summary>
        internal async static Task TrimVideo(MediaFile media, double lowerValue, double upperValue, CancellationToken token)
        {
            Object[] timeParameters = HoursMinutesSecondsConverter(lowerValue, upperValue);
            string command = $"-ss " +
                    $"{timeParameters[0]}:{timeParameters[1]}:{timeParameters[2].ToString().Substring(0, 4)} -t " +
                    $"{timeParameters[3]}:{timeParameters[4]}:{timeParameters[5].ToString().Substring(0, 4)}";

            MultipartFormDataContent content = new MultipartFormDataContent
            {
                {
                    new StreamContent(media.GetStream()),
                    "\"file\"",
                    $"\"{Path.GetFileName(media.Path)}\""
                },
                {
                    new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(command))),
                    "\"file\"",
                    $"\"command.txt\""
                }
            };

            HttpClient httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"{ServerIP}/trim", content, token);

            Stream videoStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            string filename = $"video_{RandomString(8)}.mp4";
            var helper = DependencyService.Get<IHelper>();
            try
            {
                await helper.saveStreamToFileSystem(videoStream, filename);
                string location = await helper.getSaveFileLocation();
                Log($"File is available at: {location}");
                Log($"Filename is {filename}");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Szerver számára elküldeni az újrarendezésre szánt videókat és paramétereket.
        /// Bemeneti értékek a médiafájlok melyek ki lettek választva és a cancellation token.
        /// </summary>
        internal async static Task Rearrange(ItemsGroupViewModel files, CancellationToken token)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();

            foreach (ItemViewModel item in files)
            {
                content.Add(new StreamContent(item.MediaFile.GetStream()),
                    "\"file\"",
                    $"\"{item.Title}\"");
            }

            HttpClient httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"{ServerIP}/rearrange", content, token);

            Stream videoStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            string filename = $"video_{RandomString(8)}.mp4";

            var helper = DependencyService.Get<IHelper>();
            try
            {
                await helper.saveStreamToFileSystem(videoStream, filename);
                string location = await helper.getSaveFileLocation();
                Log($"File is available at: {location}");
                Log($"Filename is {filename}");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Szerver számára elküldeni a hangot melyet a videóhoz adunk és videót melyhez a hangot adjuk és paramétereket.
        /// Bemeneti értékek a médiafájl amely éppen lejátszva van a nézetben, a hangfájl, a felső és alsó tartománycsúszka értéke és a cancellation token.
        /// </summary>
        internal async static Task AddAudioToVideo(MediaFile videoFile, string audiofilename, Stream audiostream, double lowerValue, double upperValue, CancellationToken token)
        {
            Object[] timeParameters = HoursMinutesSecondsConverter(0, upperValue);// - lowerValue);

            string command = $"-ss " +
                    $"{timeParameters[0]}:{timeParameters[1]}:{timeParameters[2].ToString().Substring(0, 4)} -t " +
                    $"{timeParameters[3]}:{timeParameters[4]}:{timeParameters[5].ToString().Substring(0, 4)}";

            timeParameters = HoursMinutesSecondsConverter(lowerValue, 0);
            command += "\n -itsoffset " +
                    $"{timeParameters[0]}:{timeParameters[1]}:{timeParameters[2]}";

            MultipartFormDataContent content = new MultipartFormDataContent
            {
                {
                    new StreamContent(videoFile.GetStream()),
                    "\"file\"",
                    $"\"{Path.GetFileName(videoFile.Path)}\""
                },
                {
                    new StreamContent(audiostream),
                    "\"file\"",
                    $"\"{audiofilename}\""
                },
                {
                    new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(command))),
                    "\"file\"",
                    $"\"command.txt\""
                }
            };

            HttpClient httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"{ServerIP}/addaudiotovideo", content, token);

            Stream videoStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            string filename = $"video_{RandomString(8)}.mp4";

            var helper = DependencyService.Get<IHelper>();
            try
            {
                await helper.saveStreamToFileSystem(videoStream, filename);
                string location = await helper.getSaveFileLocation();
                Log($"File is available at: {location}");
                Log($"Filename is {filename}");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Szerver számára elküldeni a videót amelyhez hatást adunk és paramétereket.
        /// Bemeneti értékek a hatás megnevezése, a médiafájl amely éppen lejátszva van a nézetben, a felső és alsó tartománycsúszka értéke és a cancellation token.
        /// </summary>
        internal async static Task AddEffectToVideo(string v, MediaFile videoFile, double lowerValue, double upperValue, CancellationToken token)
        {
            string command = $"{v}\n{lowerValue}\n{upperValue}";

            MultipartFormDataContent content = new MultipartFormDataContent
            {
                {
                    new StreamContent(videoFile.GetStream()),
                    "\"file\"",
                    $"\"{Path.GetFileName(videoFile.Path)}\""
                },
                {
                    new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(command))),
                    "\"file\"",
                    $"\"command.txt\""
                }
            };

            HttpClient httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"{ServerIP}/addeffecttovideo", content, token);

            Stream videoStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            var helper = DependencyService.Get<IHelper>();
            string filename = $"video_{RandomString(8)}.mp4";

            try
            {
                await helper.saveStreamToFileSystem(videoStream, filename);
                string location = await helper.getSaveFileLocation();
                Log($"File is available at: {location}");
                Log($"Filename is {filename}");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Szerver számára elküldeni a montázs készítésére szánt képeket és paramétereket.
        /// Bemeneti értékek a képek melyek ki lettek választva és a cancellation token.
        /// </summary>
        internal async static Task CreateMontage(ItemsGroupViewModel files, CancellationToken token)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();

            foreach (ItemViewModel item in files)
            {
                content.Add(new StreamContent(item.MediaFile.GetStream()),
                    "\"file\"",
                    $"\"{item.Title}\"");
            }

            HttpClient httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"{ServerIP}/montage", content, token);

            Stream videoStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            string filename = $"video_{RandomString(8)}.mp4";

            var helper = DependencyService.Get<IHelper>();
            try
            {
                await helper.saveStreamToFileSystem(videoStream, filename);
                string location = await helper.getSaveFileLocation();
                Log($"File is available at: {location}");
                Log($"Filename is {filename}");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        #region helperfunctions
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
        /// Stream-et alakít ás byte array-re
        /// </summary>
        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
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
        /// N számú random stringet generál.
        /// </summary>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion
    }
}
