using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace DublicateFileFinder
{
    internal class CheckForDublicates
    {
        private static Dictionary<Tuple<string, long>, string> _files = new Dictionary<Tuple<string, long>, string>();

        private static Dictionary<Tuple<string, long>, ObservableCollection<string>> _dublets =
            new Dictionary<Tuple<string, long>, ObservableCollection<string>>();

        private static List<string> _usedDirect = new List<string>();
        private static List<string> _notSerached = new List<string>();
        private static List<string> _notCheked = new List<string>();

        public static async Task<Tuple<List<string>, List<string>>> FileTjeck(string directory,
            List<string> skipExtentions,
            ObservableCollection<ObservableCollection<string>> showedResults, CancellationToken token)
        {
            Clearall();
            SynchronizationContext context = SynchronizationContext.Current;
            Action<ObservableCollection<string>> addFunc =
                st => context.Send(std => showedResults.Add(st), null);
            Action<Tuple<string, long>, string> addDubFunc =
                (key, stss) => context.Send(stf => _dublets[key].Add(stss), null);
            try
            {
                Task task = new Task(() => Indexfile(directory, skipExtentions, addFunc, addDubFunc, token));
                task.Start();
                await task;
                return new Tuple<List<string>, List<string>>(_notSerached, _notCheked);
            }
            catch (OperationCanceledException)
            {
                return new Tuple<List<string>, List<string>>(_notSerached, _notCheked);
            }
        }

        private static void Indexfile(string directory, List<string> skipExtentions,
            Action<ObservableCollection<string>> addFunc, Action<Tuple<string, long>, string> addDubFunc,
            CancellationToken token)
        {
            try
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    token.ThrowIfCancellationRequested();
                    string key = "";
                    FileInfo fileinfo = new FileInfo(file);
                    if (skipExtentions.Contains(fileinfo.Extension.ToLower()))
                    {
                        continue;
                    }

                    try
                    {
                        using (SHA512 sha512 = SHA512.Create())
                        {
                            using (FileStream stream = File.OpenRead(file))
                            {
                                byte[] hash = sha512.ComputeHash(stream);
                                key = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                            }
                        }
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        _notSerached.Add(e.Message.Split('\'')[1]);
                    }
                    catch (IOException)
                    {
                        _notCheked.Add(file);
                    }
                    if (!_files.ContainsKey(new Tuple<string, long>(key, fileinfo.Length)))
                    {
                        _files.Add(new Tuple<string, long>(key, fileinfo.Length), file);
                    }
                    else if (_dublets.ContainsKey(new Tuple<string, long>(key, fileinfo.Length)))
                    {
                        addDubFunc(new Tuple<string, long>(key, fileinfo.Length), file);
                    }
                    else
                    {
                        _dublets[new Tuple<string, long>(key, fileinfo.Length)] = new ObservableCollection<string>();
                        addDubFunc(new Tuple<string, long>(key, fileinfo.Length), file);
                        addDubFunc(new Tuple<string, long>(key, fileinfo.Length),
                            _files[new Tuple<string, long>(key, fileinfo.Length)]);
                        addFunc(_dublets[new Tuple<string, long>(key, fileinfo.Length)]);
                    }
                }

                foreach (string directori in Directory.GetDirectories(directory))
                {
                    Indexfile(directori, skipExtentions, addFunc, addDubFunc, token);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                _notSerached.Add(e.Message.Split('\'')[1]);
            }
        }

        /* private static bool serach(string directory, FileInfo searchKriteria)
         {
             try
             {
                 if (DateTime.Now - Lastchange > TimeSpan.FromMilliseconds(100))
                 {
                     if (index > 3)
                         index = 0;
                     int currentLineCursor = Console.CursorTop;
                     Console.SetCursorPosition(0, Console.CursorTop);
                     Console.Write(new string(' ', Console.WindowWidth));
                     Console.SetCursorPosition(0, currentLineCursor);
                     var s = "Loading[" + loading[index] + "]" + "\tCurrent file: " + searchKriteria.FullName + " Current Directory: " + directory;
                     if (s.Length + 6 > Console.WindowWidth)
                     {
                         s = s.Remove(Console.WindowWidth - 9);
                         s = s + "...";
                     }
                     Console.Write(s);
                     index++;
                     Lastchange = DateTime.Now;
                 }
                 UsedDirect.Add(directory);
                 var files = Directory.GetFiles(directory);
                 foreach (var file in files)
                 {
                     var sfile = new FileInfo(file);
                     if (sfile.Exists &&
                         sfile.Name.Equals(searchKriteria.Name) &&
                         sfile.Length.Equals(searchKriteria.Length) &&
                         sfile.Extension.Equals(searchKriteria.Extension))
                     {
                         return true;
                     }
                 }
                 var directories = Directory.GetDirectories(directory);
                 foreach (var directori in directories)
                 {
                     if (!UsedDirect.Contains(directori))
                     {
                         if (serach(directori, searchKriteria))
                         {
                             return true;
                         }
                     }
                 }
                 return false;
             }
             catch (System.UnauthorizedAccessException e)
             {
                 NotSerached.Add(e.Message.Split('\'')[1]);
                 return false;
             }
         }
         */

        private static void Clearall()
        {
            _usedDirect.Clear();
            _notSerached.Clear();
            _notCheked.Clear();
            _dublets.Clear();
            _files.Clear();
        }
    }
}