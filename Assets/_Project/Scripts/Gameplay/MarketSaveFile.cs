using System;
using System.IO;
using System.Text;

namespace ChoDoLa.Gameplay
{
    public sealed class MarketSaveFile
    {
        private readonly string path;

        public MarketSaveFile(string path)
        {
            this.path = Path.GetFullPath(path);
        }

        public string Read() => File.Exists(path) ? File.ReadAllText(path) : null;

        public MarketRun LoadOrStart(string content, int seed, Action<string> onRecovered = null)
        {
            string backupPath = null;
            var run = new MarketRun(content, seed, Read(), Write, () =>
            {
                backupPath = path + ".invalid-" + Guid.NewGuid().ToString("N");
                File.Copy(path, backupPath);
            });

            if (backupPath != null)
                onRecovered?.Invoke(backupPath);
            return run;
        }

        public void Write(string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var temporaryPath = path + ".tmp";
            var bytes = Encoding.UTF8.GetBytes(json);
            using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(path))
                File.Replace(temporaryPath, path, null);
            else
                File.Move(temporaryPath, path);
        }
    }
}
