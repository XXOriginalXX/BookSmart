using AppointmentSystem.API.Data;
using AppointmentSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/appointments/{appointmentId}/documents")]
    public class DocumentController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly string _uploadPath;

        public DocumentController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _uploadPath = config["Storage:UploadPath"] ?? "uploads";
            Directory.CreateDirectory(_uploadPath);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(int appointmentId, [FromForm] List<IFormFile> files)
        {
            bool exists = await _db.Appointments.AnyAsync(a => a.Id == appointmentId);
            if (!exists)
                return NotFound(new { message = "Appointment not found." });

            if (files == null || files.Count == 0)
                return BadRequest(new { message = "No files provided." });

            var saved = new List<object>();

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName);
                var uniqueName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(_uploadPath, uniqueName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var doc = new AppointmentDocument
                {
                    AppointmentId = appointmentId,
                    FileName = file.FileName,
                    FilePath = fullPath,
                    FileSize = file.Length
                };

                _db.AppointmentDocuments.Add(doc);
                saved.Add(new { doc.FileName, doc.FileSize });
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = $"{saved.Count} file(s) uploaded.", files = saved });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int appointmentId)
        {
            var docs = await _db.AppointmentDocuments
                .Where(d => d.AppointmentId == appointmentId)
                .Select(d => new { d.Id, d.FileName, d.FileSize, d.UploadedAt })
                .ToListAsync();

            return Ok(docs);
        }

        [HttpGet("{documentId}")]
        public async Task<IActionResult> Download(int appointmentId, int documentId)
        {
            var doc = await _db.AppointmentDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.AppointmentId == appointmentId);

            if (doc == null || !System.IO.File.Exists(doc.FilePath))
                return NotFound(new { message = "File not found." });

            var bytes = await System.IO.File.ReadAllBytesAsync(doc.FilePath);
            return File(bytes, "application/octet-stream", doc.FileName);
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> Delete(int appointmentId, int documentId)
        {
            var doc = await _db.AppointmentDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.AppointmentId == appointmentId);

            if (doc == null)
                return NotFound(new { message = "Document not found." });

            if (System.IO.File.Exists(doc.FilePath))
                System.IO.File.Delete(doc.FilePath);

            _db.AppointmentDocuments.Remove(doc);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Document deleted." });
        }
    }
}