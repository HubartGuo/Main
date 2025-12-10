namespace DocumentReadDemo.Models
{
    public class FolderItem
    {
        public string Name { get; set; } // 文件或文件夹的名称（不含路径）
        public string Path { get; set; } // 相对于根Documents目录的相对路径
        public bool IsFolder { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public List<FolderItem> Children { get; set; } = new List<FolderItem>(); // 子文件和子文件夹

        // 辅助属性，用于前端获取文件类型图标
        public string FileType => IsFolder ? "folder" : System.IO.Path.GetExtension(Name).ToLower();
    }
}
