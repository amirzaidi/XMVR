namespace LibUtil
{
    public class Assert
    {
        public static void FileExists(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException(file);
            }
        }
    }
}
