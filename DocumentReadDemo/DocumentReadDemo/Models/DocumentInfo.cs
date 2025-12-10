namespace DocumentReadDemo.Models
{
    public class DocumentInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string FileType { get; set; }
    }
}
