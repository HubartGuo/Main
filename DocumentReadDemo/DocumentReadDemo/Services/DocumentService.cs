using System.IO;
using System.Collections.Generic;
using DocumentReadDemo.Models;
using Microsoft.Extensions.Options;

namespace DocumentReadDemo.Services
{
    public class DocumentService
    {
        private readonly string _documentsPath;
        private readonly string[] _allowedExtensions = { ".txt", ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx", ".pptx" };

        public DocumentService(IOptions<AppSettings> settings)
        {
            _documentsPath = Path.GetFullPath(settings.Value.DocumentsPath);

            // 确保目录存在
            if (!Directory.Exists(_documentsPath))
            {
                Directory.CreateDirectory(_documentsPath);
            }
        }

        public List<DocumentInfo> GetDocuments()
        {
            var documents = new List<DocumentInfo>();

            try
            {
                var files = Directory.GetFiles(_documentsPath, "*.*")
                    .Where(file => _allowedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .OrderBy(file => Path.GetFileName(file));

                foreach (var filePath in files)
                {
                    var fileInfo = new FileInfo(filePath);
                    documents.Add(new DocumentInfo
                    {
                        Name = Path.GetFileName(filePath),
                        Path = filePath,
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        FileType = Path.GetExtension(filePath).ToLower()
                    });
                }
            }
            catch (Exception ex)
            {
                // 记录日志
                Console.WriteLine($"获取文档列表失败: {ex.Message}");
            }

            return documents;
        }

        //public Stream GetDocumentStream(string fileName)
        //{
        //    // 安全验证：防止目录遍历攻击
        //    var safeFileName = Path.GetFileName(fileName);
        //    if (string.IsNullOrEmpty(safeFileName))
        //        throw new ArgumentException("文件名无效");

        //    var fullPath = Path.Combine(_documentsPath, safeFileName);

        //    // 额外安全检查
        //    if (!Path.GetFullPath(fullPath).StartsWith(_documentsPath))
        //        throw new UnauthorizedAccessException("访问被拒绝");

        //    if (!File.Exists(fullPath))
        //        throw new FileNotFoundException("文件不存在");

        //    return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        //}

        //public Stream GetDocumentStream(string relativePath)
        //{
        //    // 1. 安全检查：防止路径遍历攻击（如 ../../../etc/passwd）
        //    var safeRelativePath = Path.GetRelativePath(".", relativePath);
        //    if (safeRelativePath.StartsWith("..") || Path.IsPathRooted(safeRelativePath))
        //    {
        //        throw new UnauthorizedAccessException("非法路径访问。");
        //    }

        //    // 2. 将相对路径与文档根目录拼接，得到完整物理路径
        //    var fullPath = Path.Combine(_documentsPath, safeRelativePath);

        //    // 3. 验证文件是否存在
        //    if (!File.Exists(fullPath))
        //    {
        //        throw new FileNotFoundException($"文件未找到: {relativePath}");
        //    }

        //    // 4. 返回文件流
        //    return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        //}

        public Stream GetDocumentStream(string relativePath)
        {
            try
            {
                // 1. 对前端URL编码的路径进行解码
                relativePath = Uri.UnescapeDataString(relativePath);

                // 2. 核心修正：直接拼接，但进行严格的安全检查
                // 防止任何形式的路径遍历攻击 (如 ../../../etc/passwd)
                var fullPath = Path.GetFullPath(Path.Combine(_documentsPath, relativePath));

                // 安全检查：确保最终路径确实位于我们允许的文档根目录之下
                if (!fullPath.StartsWith(Path.GetFullPath(_documentsPath)))
                {

                    throw new UnauthorizedAccessException("访问被拒绝：非法路径。");
                }

                // 3. 验证文件是否存在
                if (!File.Exists(fullPath))
                {

                    throw new FileNotFoundException($"文件不存在: {relativePath}");
                }


                return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException) && !(ex is UnauthorizedAccessException))
            {


                throw; // 重新抛出，由控制器处理
            }
        }

        public string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };
        }

        public FolderItem GetDocumentTree()
        {
            var rootFolder = new FolderItem
            {
                Name = "Documents",
                Path = "",
                IsFolder = true
            };
            // 递归构建子树，从根目录开始
            BuildTree(rootFolder, _documentsPath);
            return rootFolder;
        }

        private void BuildTree(FolderItem parentFolder, string currentPhysicalPath)
        {
            try
            {
                // 1. 添加子文件夹
                foreach (var dirPath in Directory.GetDirectories(currentPhysicalPath))
                {
                    var dirInfo = new DirectoryInfo(dirPath);
                    var folderItem = new FolderItem
                    {
                        Name = dirInfo.Name,
                        Path = Path.Combine(parentFolder.Path, dirInfo.Name), // 相对路径
                        IsFolder = true,
                        LastModified = dirInfo.LastWriteTime
                    };
                    parentFolder.Children.Add(folderItem);
                    // 递归调用，深入子文件夹
                    BuildTree(folderItem, dirPath);
                }

                // 2. 添加当前目录下的文件
                foreach (var filePath in Directory.GetFiles(currentPhysicalPath))
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileItem = new FolderItem
                    {
                        Name = fileInfo.Name,
                        Path = Path.Combine(parentFolder.Path, fileInfo.Name), // 文件相对路径
                        IsFolder = false,
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime
                    };
                    parentFolder.Children.Add(fileItem);
                }
                // 可选：对同级项目进行排序，文件夹在前，文件在后
                parentFolder.Children = parentFolder.Children
                    .OrderByDescending(x => x.IsFolder) // 文件夹在前
                    .ThenBy(x => x.Name)
                    .ToList();
            }
            catch (UnauthorizedAccessException)
            {
                // 处理无权限访问的目录
                Console.WriteLine($"无法访问目录: {currentPhysicalPath}");
            }
        }
    }
}
