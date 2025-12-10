using Microsoft.AspNetCore.Mvc;
using DocumentReadDemo.Services;
using DocumentReadDemo.Models;

namespace DocumentReadDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly DocumentService _documentService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(DocumentService documentService, ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        [HttpGet("list")]
        public IActionResult GetDocumentList()
        {
            try
            {
                var documents = _documentService.GetDocuments();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文档列表失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        //[HttpGet("download/{fileName}")]
        //public IActionResult DownloadDocument(string fileName)
        //{
        //    try
        //    {
        //        var stream = _documentService.GetDocumentStream(fileName);
        //        var contentType = _documentService.GetContentType(fileName);

        //        // 设置响应头，让浏览器在新标签页打开
        //        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{Uri.EscapeDataString(fileName)}\"");

        //        return File(stream, contentType);
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        return NotFound(new { error = "文件不存在" });
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Forbid();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "下载文件失败");
        //        return StatusCode(500, new { error = "服务器内部错误" });
        //    }
        //}

        //[HttpGet("download/{*relativePath}")] // 注意这里的 * 捕获，它允许路径中包含斜杠/
        //public IActionResult DownloadDocument(string relativePath)
        //{
        //    try
        //    {
        //        // 对URL编码的路径进行解码（前端用encodeURIComponent编码了）
        //        var decodedPath = Uri.UnescapeDataString(relativePath);

        //        var stream = _documentService.GetDocumentStream(decodedPath);
        //        var contentType = _documentService.GetContentType(Path.GetFileName(decodedPath)); // 仍用文件名获取类型

        //        // 设置响应头，让浏览器在新标签页中打开（预览）而非直接下载
        //        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{Path.GetFileName(decodedPath)}\"");

        //        return File(stream, contentType);
        //    }
        //    catch (FileNotFoundException ex)
        //    {
        //        _logger.LogWarning(ex, "请求的文件不存在。");
        //        return NotFound(new { error = "请求的文档不存在，请检查路径。" });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        _logger.LogWarning(ex, "非法文件路径访问。");
        //        return BadRequest(new { error = "非法的文件路径。" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "下载文件时发生意外错误。");
        //        return StatusCode(500, new { error = "服务器内部错误，无法提供文件。" });
        //    }
        //}

        [HttpGet("download/{*relativePath}")]
        public IActionResult DownloadDocument(string relativePath)
        {
            try
            {
                // 注意：这里不再需要 Uri.UnescapeDataString，因为 GetDocumentStream 内部已处理
                var stream = _documentService.GetDocumentStream(relativePath);
                var fileName = Path.GetFileName(relativePath);
                var contentType = _documentService.GetContentType(fileName);

                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{Uri.EscapeDataString(fileName)}\"");
                return File(stream, contentType);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "请求的文件不存在。");
                return NotFound(new { error = "请求的文档不存在，请检查路径。" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "非法文件路径访问。");
                return BadRequest(new { error = "非法的文件路径。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下载文件时发生意外错误。");
                return StatusCode(500, new { error = "服务器内部错误，无法提供文件。" });
            }
        }

            [HttpGet("preview/{fileName}")]
        public IActionResult PreviewDocument(string fileName)
        {
            try
            {
                var stream = _documentService.GetDocumentStream(fileName);
                var contentType = _documentService.GetContentType(fileName);

                // 对于文本文件，我们可以直接返回内容用于预览
                if (contentType == "text/plain")
                {
                    using var reader = new StreamReader(stream);
                    var content = reader.ReadToEnd();
                    return Content(content, "text/plain; charset=utf-8");
                }

                // 其他文件类型直接返回文件流
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{Uri.EscapeDataString(fileName)}\"");
                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预览文件失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        [HttpGet("tree")]
        public IActionResult GetDocumentTree()
        {
            try
            {
                var tree = _documentService.GetDocumentTree();
                return Ok(tree);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文档树失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }
    }
}
