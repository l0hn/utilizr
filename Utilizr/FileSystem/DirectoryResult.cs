namespace Utilizr.FileSystem
{
    public class DirectoryResult
    {
        public string Path { get; private set; }
        public string[] Directories { get; set; }
        public string[] Files { get; set; }

        internal DirectoryResult(string path, string[] directories, string[] files)
        {
            Path = path;
            Directories = directories;
            Files = files;
        }
    }
}
