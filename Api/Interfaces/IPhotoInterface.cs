using System;
using CloudinaryDotNet.Actions;

namespace Api.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile formFile);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}
